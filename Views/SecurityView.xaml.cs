using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Frakture_Tweaks
{
    public partial class SecurityView : UserControl
    {
        public SecurityView()
        {
            InitializeComponent();
        }

        private void DisableDefenderBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will attempt to disable Windows Defender using TrustedInstaller privileges.\n\nContinue?", "Disable Windows Defender", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var logWindow = new LogWindow();
                logWindow.Show();
                logWindow.AddLog("Initializing Windows Defender Disabling (TrustedInstaller Mode)...");

                Task.Run(async () =>
                {
                    try
                    {
                        
                        
                        string logFile = @"C:\Windows\Temp\defender_log.txt";
                        string batchContent = $"( \r\n" +
                                              $"cd /d C:\\Windows\\Temp \r\n" +
                                              $"echo Disabling Services... \r\n" +
                                              $"NET stop windefend /y \r\n" +
                                              $"NET stop WdNisSvc /y \r\n" +
                                              $"NET stop Sense /y \r\n" +
                                              $"NET stop MsSecCore /y \r\n" +
                                              $"NET stop SecurityHealthService /y \r\n" +
                                              $"SC config windefend start= disabled \r\n" +
                                              $"SC config WdNisSvc start= disabled \r\n" +
                                              $"SC config Sense start= disabled \r\n" +
                                              $"SC config MsSecCore start= disabled \r\n" +
                                              $"SC config SecurityHealthService start= disabled \r\n" +
                                              $"SC config WdBoot start= disabled \r\n" +
                                              $"SC config WdFilter start= disabled \r\n" +
                                              $"SC config WdNisDrv start= disabled \r\n" +
                                              $"echo Disabling Registry Keys... \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /v \"DisableAntiSpyware\" /t \"REG_DWORD\" /d \"1\" /f \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /v \"DisableAntiVirus\" /t \"REG_DWORD\" /d \"1\" /f \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v \"DisableBehaviorMonitoring\" /t \"REG_DWORD\" /d \"1\" /f \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v \"DisableOnAccessProtection\" /t \"REG_DWORD\" /d \"1\" /f \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v \"DisableScanOnRealtimeEnable\" /t \"REG_DWORD\" /d \"1\" /f \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v \"DisableRealtimeMonitoring\" /t \"REG_DWORD\" /d \"1\" /f \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows Defender\\Features\" /v \"TamperProtection\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                              $"echo Disabling SmartScreen... \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\" /v \"SmartScreenEnabled\" /t \"REG_SZ\" /d \"Off\" /f \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\System\" /v \"EnableSmartScreen\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                              $") > \"{logFile}\" 2>&1";

                        logWindow.AddLog("Executing Advanced Defender Removal via TrustedInstaller...");
                        await TrustedInstaller.RunCommandAsTrustedInstaller(batchContent, logWindow);

                        
                        if (System.IO.File.Exists(logFile))
                        {
                            string output = System.IO.File.ReadAllText(logFile);
                            logWindow.AddLog("--- Command Output ---");
                            logWindow.AddLog(output);
                            try { System.IO.File.Delete(logFile); } catch {}
                        }

                        
                        logWindow.AddLog("Querying WinDefend status...");
                        RunCommand("sc", "qc \"windefend\"", logWindow);

                        logWindow.AddLog("--------------------------------------------------");
                        logWindow.AddLog("Operation completed. Please restart your computer.");
                    }
                    catch (Exception ex)
                    {
                        logWindow.AddLog($"Error: {ex.Message}");
                    }
                });
            }
        }

        private void DisableUACBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will fully disable User Account Control (UAC).\n\nWarning: This may prevent Universal Windows Platform (UWP) apps from opening.\nA restart is required.\n\nContinue?", "Disable UAC", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var logWindow = new LogWindow();
                logWindow.Show();
                logWindow.AddLog("Disabling UAC...");

                Task.Run(() =>
                {
                    try
                    {
                        
                        RunCommand("reg.exe", @"ADD HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System /v ConsentPromptBehaviorAdmin /t REG_DWORD /d 0 /f", logWindow);
                        
                        
                        RunCommand("reg.exe", @"ADD HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System /v PromptOnSecureDesktop /t REG_DWORD /d 0 /f", logWindow);
                        
                        
                        RunCommand("reg.exe", @"ADD HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System /v EnableLUA /t REG_DWORD /d 0 /f", logWindow);

                        logWindow.AddLog("UAC has been fully disabled. Please Restart your computer to apply changes.");
                    }
                    catch (Exception ex)
                    {
                        logWindow.AddLog($"Error: {ex.Message}");
                    }
                });
            }
        }

        private void DisableVBSBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will disable Virtualization-Based Security (VBS) and Memory Integrity (HVCI).\nThis can significantly improve gaming performance but reduces security.\n\nA restart is required. Continue?", "Disable VBS / Memory Integrity", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var logWindow = new LogWindow();
                logWindow.Show();
                logWindow.AddLog("Initializing VBS / Memory Integrity Disabling...");

                Task.Run(async () =>
                {
                    try
                    {
                        string logFile = @"C:\Windows\Temp\vbs_disable_log.txt";
                        string batchContent = $"( \r\n" +
                                              $"echo Disabling VBS and Memory Integrity via Registry... \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\DeviceGuard\" /v \"EnableVirtualizationBasedSecurity\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\DeviceGuard\\Scenarios\\HypervisorEnforcedCodeIntegrity\" /v \"Enabled\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Lsa\" /v \"LsaCfgFlags\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                              $"echo Disabling Virtualization-Based Protection... \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\DeviceGuard\" /v \"EnableVirtualizationBasedSecurity\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                              $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\DeviceGuard\" /v \"HVCIMATRequired\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                              $") > \"{logFile}\" 2>&1";

                        logWindow.AddLog("Executing VBS Removal via TrustedInstaller...");
                        await TrustedInstaller.RunCommandAsTrustedInstaller(batchContent, logWindow);

                        if (System.IO.File.Exists(logFile))
                        {
                            string output = System.IO.File.ReadAllText(logFile);
                            logWindow.AddLog("--- Command Output ---");
                            logWindow.AddLog(output);
                            try { System.IO.File.Delete(logFile); } catch { }
                        }

                        logWindow.AddLog("--------------------------------------------------");
                        logWindow.AddLog("VBS / Memory Integrity Disabled. Please Restart.");
                    }
                    catch (Exception ex)
                    {
                        logWindow.AddLog($"Error: {ex.Message}");
                    }
                });
            }
        }

        private void RunCommand(string command, string args, LogWindow log)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = command,
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
                        string output = p.StandardOutput.ReadToEnd();
                        string error = p.StandardError.ReadToEnd();
                        p.WaitForExit();

                        if (!string.IsNullOrWhiteSpace(output)) log.AddLog(output.Trim());
                        if (!string.IsNullOrWhiteSpace(error)) log.AddLog($"Error/Info: {error.Trim()}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.AddLog($"Failed to execute {command}: {ex.Message}");
            }
        }
    }
}
