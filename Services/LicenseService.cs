using System;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frakture_Tweaks
{
    public class LicenseService
    {
        private const string ApiUrl = "https://frakturetweaks.ru/api.php";
        private static readonly byte[] StorageEntropy = Encoding.UTF8.GetBytes("frakture-tweaks-license-v1");
        private static readonly HttpClient HttpClient = CreateHttpClient();

        public static string GetHWID()
        {
            
            var task = Task.Run(() =>
            {
                try
                {
                    string cpu = "";
                    string hdd = "";

                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                    {
                        searcher.Options.Timeout = TimeSpan.FromSeconds(2);
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            cpu = obj["ProcessorId"]?.ToString() ?? "";
                            break;
                        }
                    }

                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive"))
                    {
                        searcher.Options.Timeout = TimeSpan.FromSeconds(2);
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            hdd = obj["SerialNumber"]?.ToString() ?? "";
                            break;
                        }
                    }

                    string raw = cpu + hdd;
                    using (SHA256 sha = SHA256.Create())
                    {
                        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
                    }
                }
                catch
                {
                    return "fallback-id-" + Environment.MachineName;
                }
            });

            if (task.Wait(TimeSpan.FromSeconds(5)))
            {
                return task.Result;
            }
            else
            {
                return "timeout-id-" + Environment.MachineName;
            }
        }

        public async Task<(bool success, string message)> RedeemKey(string key)
        {
            try
            {
                key = NormalizeKey(key);
                if (string.IsNullOrWhiteSpace(key))
                {
                    return (false, "Please enter a valid key.");
                }

                var hwid = GetHWID();
                var username = Environment.UserName;

                var payload = new
                {
                    action = "redeem",
                    key = key,
                    hwid = hwid,
                    username = username
                };

                string json = JsonSerializer.Serialize(payload);
                using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await HttpClient.SendAsync(request);
                if (response.StatusCode == (HttpStatusCode)429)
                {
                    var delay = GetRetryDelay(response) ?? TimeSpan.FromSeconds(2);
                    if (delay > TimeSpan.Zero)
                    {
                        if (delay > TimeSpan.FromSeconds(5)) delay = TimeSpan.FromSeconds(5);
                        await Task.Delay(delay);
                    }
                    return (false, "Too many requests. Please wait and try again.");
                }

                string responseString = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(responseString))
                {
                    return (false, response.IsSuccessStatusCode ? "Empty server response" : $"Server error ({(int)response.StatusCode})");
                }

                var parsed = TryParseApiResponse(responseString);
                if (parsed.HasValue)
                {
                    var value = parsed.Value;
                    if (value.success)
                    {
                        return (true, value.message);
                    }

                    return (false, MapErrorMessage(value.errorCode, value.message, response.StatusCode));
                }

                return (false, response.IsSuccessStatusCode ? "Invalid server response" : $"Server error ({(int)response.StatusCode})");
            }
            catch (Exception ex)
            {
                return (false, "Connection error: " + ex.Message);
            }
        }

        public void SaveKey(string key)
        {
            try 
            {
                key = NormalizeKey(key);
                if (string.IsNullOrWhiteSpace(key))
                {
                    return;
                }

                var storagePath = GetStoragePath();
                Directory.CreateDirectory(Path.GetDirectoryName(storagePath) ?? AppDomain.CurrentDomain.BaseDirectory);

                var plaintext = Encoding.UTF8.GetBytes(key);
                var ciphertext = ProtectedData.Protect(plaintext, StorageEntropy, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(storagePath, ciphertext);
            } 
            catch { }
        }

        public string? GetSavedKey()
        {
            try 
            {
                var storagePath = GetStoragePath();
                if (File.Exists(storagePath))
                {
                    var ciphertext = File.ReadAllBytes(storagePath);
                    if (ciphertext.Length == 0) return null;

                    var plaintext = ProtectedData.Unprotect(ciphertext, StorageEntropy, DataProtectionScope.CurrentUser);
                    var key = NormalizeKey(Encoding.UTF8.GetString(plaintext));
                    return string.IsNullOrWhiteSpace(key) ? null : key;
                }

                var legacy = TryReadLegacyKey();
                if (!string.IsNullOrWhiteSpace(legacy))
                {
                    SaveKey(legacy);
                    TryDeleteLegacyKey();
                    return NormalizeKey(legacy);
                }

                return null;
            } 
            catch 
            { 
                return null; 
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FraktureTweaks/1.0");

            return client;
        }

        private static string NormalizeKey(string key)
        {
            return (key ?? string.Empty).Trim();
        }

        private static string GetStoragePath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Frakture Tweaks");
            return Path.Combine(dir, "license.dat");
        }

        private static string GetLegacyPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.dat");
        }

        private static string? TryReadLegacyKey()
        {
            try
            {
                var legacyPath = GetLegacyPath();
                if (!File.Exists(legacyPath)) return null;

                var fileBytes = File.ReadAllBytes(legacyPath);
                if (fileBytes.Length < 16) return null;

                var hwid = GetHWID();
                using var sha = SHA256.Create();
                var keyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(hwid));

                using var aes = Aes.Create();
                aes.Key = keyBytes;

                byte[] iv = new byte[16];
                Array.Copy(fileBytes, 0, iv, 0, 16);
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(fileBytes, 16, fileBytes.Length - 16);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                var value = sr.ReadToEnd();

                var key = NormalizeKey(value);
                return string.IsNullOrWhiteSpace(key) ? null : key;
            }
            catch
            {
                return null;
            }
        }

        private static void TryDeleteLegacyKey()
        {
            try
            {
                var legacyPath = GetLegacyPath();
                if (File.Exists(legacyPath))
                {
                    File.Delete(legacyPath);
                }
            }
            catch
            {
            }
        }

        private static TimeSpan? GetRetryDelay(HttpResponseMessage response)
        {
            try
            {
                if (response.Headers?.RetryAfter == null) return null;
                if (response.Headers.RetryAfter.Delta.HasValue) return response.Headers.RetryAfter.Delta.Value;
                if (response.Headers.RetryAfter.Date.HasValue)
                {
                    var delta = response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
                    return delta > TimeSpan.Zero ? delta : TimeSpan.Zero;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private static (bool success, string message, string errorCode)? TryParseApiResponse(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("status", out var statusProp))
                {
                    return null;
                }

                var status = (statusProp.GetString() ?? string.Empty).Trim();
                var message = root.TryGetProperty("message", out var msgProp) ? (msgProp.GetString() ?? string.Empty) : string.Empty;
                var errorCode = root.TryGetProperty("error_code", out var ecProp) ? (ecProp.GetString() ?? string.Empty) : string.Empty;

                var isSuccess = status.Equals("success", StringComparison.OrdinalIgnoreCase);
                return (isSuccess, message, errorCode);
            }
            catch
            {
                return null;
            }
        }

        private static string MapErrorMessage(string errorCode, string message, HttpStatusCode httpStatus)
        {
            var code = (errorCode ?? string.Empty).Trim().ToLower();

            if (code == "rate_limited" || httpStatus == (HttpStatusCode)429)
            {
                return "Too many requests. Please wait and try again.";
            }
            if (code == "invalid_key")
            {
                return "Invalid key.";
            }
            if (code == "banned")
            {
                return "Key is banned.";
            }
            if (code == "hwid_mismatch")
            {
                return "Key is already used on another PC.";
            }
            if (code == "usage_limit_reached")
            {
                return "Key has reached maximum usage limit.";
            }
            if (code == "already_has_key")
            {
                return "You already have an active license on this account.";
            }
            if (code == "already_used")
            {
                return "Key is already activated and cannot be reused.";
            }

            
            if (httpStatus == HttpStatusCode.Conflict && string.IsNullOrEmpty(code))
            {
                 return "Key has reached maximum usage limit.";
            }

            if (!string.IsNullOrWhiteSpace(message) && message.Length <= 120)
            {
                return message;
            }

            return httpStatus >= HttpStatusCode.BadRequest ? $"Server error ({(int)httpStatus})" : "Activation failed.";
        }
    }
}

