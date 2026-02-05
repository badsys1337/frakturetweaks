using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Frakture_Tweaks
{
    public class SystemBoostService
    {
        private LogWindow _logger;

        public SystemBoostService(LogWindow logger)
        {
            _logger = logger;
        }

        public async Task ApplySystemBoostAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Starting System Boost (Aphrodite Logic)...");

                    
                    _logger.AddLog("Disabling Telemetry & Data Collection...");
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0);

                    
                    _logger.AddLog("Disabling Cortana & Bing Search...");
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", 1);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0);

                    
                    _logger.AddLog("Debloating Explorer UI (Notifications, TaskView)...");
                    SetRegistryValueCU(@"Software\Policies\Microsoft\Windows\Explorer", "DisableNotificationCenter", 1);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\People", "PeopleBand", 0);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0);

                    
                    _logger.AddLog("Disabling Hibernation...");
                    RunCommand("powercfg", "-h off"); 
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 0);

                    
                    _logger.AddLog("Disabling Xbox Game Bar...");
                    SetRegistryValueCU(@"System\GameConfigStore", "GameDVR_Enabled", 0);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0);

                    
                    _logger.AddLog("Disabling Background Apps...");
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1);

                    
                    _logger.AddLog("Disabling Sticky Keys...");
                    SetRegistryStringCU(@"Control Panel\Accessibility\StickyKeys", "Flags", "506");

                    
                    _logger.AddLog("Optimizing NTFS File System...");
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsMftZoneReservation", 1);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\FileSystem", "NTFSDisable8dot3NameCreation", 1);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\FileSystem", "NTFSDisableLastAccessUpdate", 1);

                    
                    _logger.AddLog("Tweaking Windows Update Behavior...");
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoUpdate", 1);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "AUOptions", 2); 

                    _logger.AddLog("System Boost Applied Successfully!");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error applying System Boost: {ex.Message}");
                }
            });
        }

        public async Task RevertSystemBoostAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Reverting System Boost...");
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 1);
                    RunCommand("powercfg", "-h on");
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 0);
                    SetRegistryValueCU(@"System\GameConfigStore", "GameDVR_Enabled", 1);
                    _logger.AddLog("System Boost Reverted.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error reverting System Boost: {ex.Message}");
                }
            });
        }

        private void RunCommand(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                using (Process? p = Process.Start(psi))
                {
                    if (p != null)
                    {
                        p.WaitForExit();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Failed to run {fileName} {arguments}: {ex.Message}");
            }
        }

        private void SetRegistryValueLM(string keyPath, string valueName, int value)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath, true))
                {
                    if (key != null) key.SetValue(valueName, value, RegistryValueKind.DWord);
                }
            }
            catch { _logger.AddLog($"Failed to set HKLM\\{keyPath}"); }
        }

        private void SetRegistryValueCU(string keyPath, string valueName, int value)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath, true))
                {
                    if (key != null) key.SetValue(valueName, value, RegistryValueKind.DWord);
                }
            }
            catch { _logger.AddLog($"Failed to set HKCU\\{keyPath}"); }
        }

        private void SetRegistryStringCU(string keyPath, string valueName, string value)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath, true))
                {
                    if (key != null) key.SetValue(valueName, value, RegistryValueKind.String);
                }
            }
            catch { _logger.AddLog($"Failed to set HKCU\\{keyPath}"); }
        }
    }
}

