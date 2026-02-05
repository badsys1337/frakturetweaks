using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Frakture_Tweaks
{
    public static class TrustedInstaller
    {
        public static async Task RunCommandAsTrustedInstaller(string batchContent, LogWindow log)
        {
            string serviceName = "FraktureTI";
            
            string batchPath = @"C:\Windows\Temp\frakture_ti.bat";

            try
            {
                
                File.WriteAllText(batchPath, "@echo off\r\n" + batchContent + "\r\n");

                log.AddLog("Preparing TrustedInstaller environment...");

                
                RunProcess("sc", $"delete {serviceName}");
                
                
                
                string binPath = $"\"C:\\Windows\\System32\\cmd.exe\" /c \"{batchPath}\"";
                RunProcess("sc", $"create {serviceName} binPath= \"{binPath}\" type= own");
                
                
                RunProcess("sc", $"config {serviceName} obj= \"NT SERVICE\\TrustedInstaller\" password= \"\"");

                
                log.AddLog("Executing commands as TrustedInstaller...");
                
                var startPsi = new ProcessStartInfo
                {
                    FileName = "sc",
                    Arguments = $"start {serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                
                Process.Start(startPsi);

                
                await Task.Delay(5000);

                
                RunProcess("sc", $"control {serviceName} stop"); 
                RunProcess("sc", $"delete {serviceName}");
                
                if (File.Exists(batchPath)) File.Delete(batchPath);
                
                log.AddLog("TrustedInstaller commands executed.");
            }
            catch (Exception ex)
            {
                log.AddLog($"TrustedInstaller Error: {ex.Message}");
            }
        }

        private static void RunProcess(string filename, string args)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (var p = Process.Start(psi))
                {
                    if (p != null)
                    {
                        p.StandardOutput.ReadToEnd();
                        p.StandardError.ReadToEnd();
                        p.WaitForExit();
                    }
                }
            }
            catch { }
        }
    }
}

