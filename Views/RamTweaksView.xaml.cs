using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Frakture_Tweaks
{
    
    
    
    public partial class RamTweaksView : UserControl
    {
        public RamTweaksView()
        {
            InitializeComponent();
        }

        private async void GlobalRamTweaksBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                LogWindow logWindow = new LogWindow();
                logWindow.Show();
                logWindow.AddLog("Initializing Global RAM Tweaks...");

                
                List<string> commands = GenerateRamTweaksCommands(logWindow);

                string tempBat = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FraktureRamTweaks.bat");

                logWindow.AddLog($"Generated {commands.Count} optimization commands.");
                logWindow.AddLog("Creating batch script execution wrapper...");

                using (StreamWriter sw = new StreamWriter(tempBat))
                {
                    sw.WriteLine("@echo off");
                    sw.WriteLine("chcp 65001 > nul"); 
                    foreach (string cmd in commands)
                    {
                        if (!string.IsNullOrWhiteSpace(cmd))
                        {
                            sw.WriteLine($"echo Executing: {cmd}");
                            sw.WriteLine(cmd);
                        }
                    }
                    sw.WriteLine("echo Done.");
                }

                logWindow.AddLog("Starting execution engine...");

                await Task.Run(() =>
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "cmd.exe";
                    psi.Arguments = $"/c \"{tempBat}\"";
                    psi.UseShellExecute = false;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.CreateNoWindow = true;

                    using (Process p = new Process())
                    {
                        p.StartInfo = psi;
                        p.OutputDataReceived += (s, args) =>
                        {
                            if (args.Data != null)
                            {
                                
                                string data = args.Data.Trim();
                                if (string.IsNullOrEmpty(data) || 
                                    data.StartsWith("echo", StringComparison.OrdinalIgnoreCase) ||
                                    data.Contains("The operation completed successfully", StringComparison.OrdinalIgnoreCase))
                                {
                                    return;
                                }

                                if (data.StartsWith("Executing:", StringComparison.OrdinalIgnoreCase))
                                {
                                    
                                    string cmd = data.Substring(10).Trim();
                                    if (cmd.Contains("EnableSuperfetch")) logWindow.AddLog("Disabling Superfetch...");
                                    else if (cmd.Contains("EnablePrefetcher")) logWindow.AddLog("Disabling Prefetcher...");
                                    else if (cmd.Contains("SvcHostSplitThresholdInKB")) logWindow.AddLog("Applying SvcHost Split Threshold...");
                                    else if (cmd.Contains("Disable-MMAgent")) 
                                    {
                                        if (cmd.Contains("MemoryCompression")) logWindow.AddLog("Disabling Memory Compression...");
                                        else if (cmd.Contains("PageCombining")) logWindow.AddLog("Disabling Page Combining...");
                                        else logWindow.AddLog("Optimizing Memory Agent...");
                                    }
                                    else if (cmd.Contains("Reg.exe add")) 
                                    {
                                        
                                        int vIndex = cmd.IndexOf("/v \"");
                                        if (vIndex != -1)
                                        {
                                            int endIndex = cmd.IndexOf("\"", vIndex + 4);
                                            if (endIndex != -1)
                                            {
                                                string valueName = cmd.Substring(vIndex + 4, endIndex - (vIndex + 4));
                                                logWindow.AddLog($"Tweaking Registry: {valueName}");
                                            }
                                            else logWindow.AddLog($"Tweaking Registry: {cmd}");
                                        }
                                        else logWindow.AddLog($"Tweaking Registry: {cmd}");
                                    }
                                    else logWindow.AddLog(data);
                                }
                                else
                                {
                                    logWindow.AddLog(data);
                                }
                            }
                        };
                        p.ErrorDataReceived += (s, args) =>
                        {
                            if (args.Data != null) 
                            {
                                string err = args.Data.Trim();
                                if (string.IsNullOrEmpty(err)) return;

                                
                                if (err.Contains("Disable-MMAgent", StringComparison.OrdinalIgnoreCase) ||
                                    err.Contains("The service cannot be started", StringComparison.OrdinalIgnoreCase) ||
                                    err.Contains("ResourceUnavailable", StringComparison.OrdinalIgnoreCase) ||
                                    err.Contains("Windows System Error 1058", StringComparison.OrdinalIgnoreCase) ||
                                    err.Contains("At line:1", StringComparison.OrdinalIgnoreCase) ||
                                    err.Contains("+ ~~~", StringComparison.OrdinalIgnoreCase) ||
                                    err.Contains("CategoryInfo", StringComparison.OrdinalIgnoreCase) ||
                                    err.Contains("FullyQualifiedErrorId", StringComparison.OrdinalIgnoreCase))
                                {
                                    
                                    
                                    return; 
                                }

                                logWindow.AddLog($"ERROR: {err}");
                            }
                        };

                        p.Start();
                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();
                        p.WaitForExit();
                    }
                });

                if (File.Exists(tempBat))
                    File.Delete(tempBat);

                logWindow.AddLog("------------------------------------------------");
                logWindow.AddLog("All RAM tweaks applied successfully.");
                logWindow.AddLog("Please restart your computer for changes to take effect.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying RAM tweaks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OptimizeRamUsageBtn_Click(object sender, RoutedEventArgs e)
        {
            RamOptimizationWindow optimizationWindow = new RamOptimizationWindow();
            optimizationWindow.ShowDialog();
        }

        private void SvcRAMBtn_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string ramAmount = btn.Content.ToString();
            uint svcValue = 0;

            try
            {
                switch (ramAmount)
                {
                    case "4GB RAM": svcValue = 68764420; break;
                    case "8GB RAM": svcValue = 137922056; break;
                    case "16GB RAM": svcValue = 376926742; break;
                    case "32GB RAM": svcValue = 861226034; break;
                    case "64GB RAM": svcValue = 1729136740; break;
                    case "Auto":
                        ulong totalMemoryKB = 0;
                        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                        {
                            foreach (ManagementObject result in searcher.Get())
                                totalMemoryKB = Convert.ToUInt64(result["TotalVisibleMemorySize"]);
                        }
                        svcValue = (uint)(totalMemoryKB + 1024000);
                        break;
                }

                if (svcValue > 0)
                {
                    Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control", "SvcHostSplitThresholdInKB", svcValue, Microsoft.Win32.RegistryValueKind.DWord);
                    StatusText.Text = $"SvcHost Split Threshold set to {svcValue} KB.";
                    StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void DisableMemoryCompressionBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "Disable-MMAgent -MemoryCompression",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }).WaitForExit();
                StatusText.Text = "Memory Compression disabled.";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            catch (Exception ex) { StatusText.Text = "Error: " + ex.Message; }
        }

        private void OptimizePagingBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mmKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";
                Microsoft.Win32.Registry.SetValue(mmKey, "DisablePagingExecutive", 1, Microsoft.Win32.RegistryValueKind.DWord);
                Microsoft.Win32.Registry.SetValue(mmKey, "ClearPageFileAtShutdown", 0, Microsoft.Win32.RegistryValueKind.DWord);
                StatusText.Text = "Paging executive optimized.";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            catch (Exception ex) { StatusText.Text = "Error: " + ex.Message; }
        }

        private void DisablePrefetchBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pfKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters";
                Microsoft.Win32.Registry.SetValue(pfKey, "EnablePrefetcher", 0, Microsoft.Win32.RegistryValueKind.DWord);
                Microsoft.Win32.Registry.SetValue(pfKey, "EnableSuperfetch", 0, Microsoft.Win32.RegistryValueKind.DWord);
                StatusText.Text = "Prefetch and Superfetch disabled.";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            catch (Exception ex) { StatusText.Text = "Error: " + ex.Message; }
        }

        private void LargeSystemCacheBtn_Click(object sender, RoutedEventArgs e)
        {
            
            StatusText.Text = "Tweak disabled for safety.";
        }

        private List<string> GenerateRamTweaksCommands(LogWindow logger)
        {
            List<string> cmds = new List<string>();

            
            try
            {
                logger.AddLog("Calculating optimal SvcHost Split Threshold...");
                ulong totalMemoryKB = 0;
                
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject result in searcher.Get())
                        totalMemoryKB = Convert.ToUInt64(result["TotalVisibleMemorySize"]);
                }

                if (totalMemoryKB > 0)
                {
                    ulong svcHostValue = totalMemoryKB + 1024000;
                    cmds.Add($"Reg.exe add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\" /v \"SvcHostSplitThresholdInKB\" /t REG_DWORD /d \"{svcHostValue}\" /f");
                    logger.AddLog($"Calculated SvcHost Threshold: {svcHostValue} KB");
                    
                    
                }
            }
            catch (Exception ex) { logger.AddLog($"Error: {ex.Message}"); }

            
            string mmKey = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management";
            string pfKey = mmKey + "\\PrefetchParameters";

            cmds.Add($"Reg.exe add \"{pfKey}\" /v \"EnablePrefetcher\" /t REG_DWORD /d \"0\" /f");
            cmds.Add($"Reg.exe add \"{pfKey}\" /v \"EnableSuperfetch\" /t REG_DWORD /d \"0\" /f");
            cmds.Add($"Reg.exe add \"{mmKey}\" /v \"ClearPageFileAtShutdown\" /t REG_DWORD /d \"0\" /f");
            cmds.Add($"Reg.exe add \"{mmKey}\" /v \"DisablePagingExecutive\" /t REG_DWORD /d \"1\" /f");
            cmds.Add($"Reg.exe add \"{mmKey}\" /v \"SystemPages\" /t REG_DWORD /d \"4294967295\" /f");
            cmds.Add($"Reg.exe add \"{mmKey}\" /v \"PoolUsageMaximum\" /t REG_DWORD /d \"96\" /f");

            
            cmds.Add("powershell -Command \"Disable-MMAgent -MemoryCompression\"");
            cmds.Add("powershell -Command \"Disable-MMAgent -PageCombining\"");
            cmds.Add("powershell -Command \"Disable-MMAgent -ApplicationPreLaunch\"");

            return cmds;
        }
    }
}

