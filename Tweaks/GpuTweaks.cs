using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Frakture_Tweaks
{
    public class GpuTweaks
    {
        private LogWindow _logger;

        public GpuTweaks(LogWindow logger)
        {
            _logger = logger;
        }

        public async Task ApplyNvidiaTweaksAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Applying Nvidia GPU Tweaks...");

                    
                    
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "PowerMizerEnable", 0, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", "Acceleration.Level", 0, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\FTS", "GPUPreemptionLevel", 0, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "RMDisablePostL2Compression", 1, RegistryValueKind.DWord);

                    
                    SetRegistryValue(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 8, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 6, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", "High", RegistryValueKind.String);
                    SetRegistryValue(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "SFIO Priority", "High", RegistryValueKind.String);
                    
                    
                    DisableScheduledTask("NVIDIA GeForce Experience SelfUpdate_{B2FE1952-0186-46C3-BAEC-A80AA35AC5B8}");

                    _logger.AddLog("Nvidia Tweaks Applied Successfully.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error applying Nvidia tweaks: {ex.Message}");
                }
            });
        }

        public async Task ApplyAmdTweaksAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Applying AMD GPU Tweaks...");

                    string classKey = @"HKLM\SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";
                    string controlSet001Key = @"HKLM\SYSTEM\ControlSet001\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";

                    SetRegistryValue(classKey, "KMD_EnableComputePreemption", 0, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "DisableSAMUPowerGating", 1, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "EnableAspmL0s", 0, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "EnableUlps", 0, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "KMD_ChillEnabled", 0, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "PP_ThermalAutoThrottlingEnable", 0, RegistryValueKind.DWord);
                    
                    
                    SetRegistryValue(classKey, "EnableVceSwClockGating", 1, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "EnableUvdClockGating", 1, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "DisableVCEPowerGating", 0, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "DisablePowerGating", 1, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "PP_GPUPowerDownEnabled", 0, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "PP_SclkDeepSleepDisable", 1, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "GCOOPTION_DisableGPIOPowerSaveMode", 1, RegistryValueKind.DWord);
                    SetRegistryValue(classKey, "DisableDMACopy", 1, RegistryValueKind.DWord);
                    
                    SetRegistryValue(controlSet001Key, "KMD_RebarControlMode", 1, RegistryValueKind.DWord);

                    _logger.AddLog("AMD Tweaks Applied Successfully.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error applying AMD tweaks: {ex.Message}");
                }
            });
        }

        private void SetRegistryValue(string keyPath, string valueName, object value, RegistryValueKind kind)
        {
            try
            {
                string root = keyPath.Split('\\')[0];
                string subKey = keyPath.Substring(root.Length + 1);

                RegistryKey regKey = null;
                if (root == "HKLM") regKey = Registry.LocalMachine.CreateSubKey(subKey);
                else if (root == "HKCU") regKey = Registry.CurrentUser.CreateSubKey(subKey);
                else if (root == "HKEY_USERS") regKey = Registry.Users.CreateSubKey(subKey);

                if (regKey != null)
                {
                    regKey.SetValue(valueName, value, kind);
                    regKey.Close();
                    _logger.AddLog($"Set {keyPath}\\{valueName} to {value}");
                }
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Failed to set registry value {keyPath}\\{valueName}: {ex.Message}");
            }
        }

        private void DisableScheduledTask(string taskName)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "schtasks",
                    Arguments = $"/change /disable /tn \"{taskName}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
                _logger.AddLog($"Disabled task: {taskName}");
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Failed to disable task {taskName}: {ex.Message}");
            }
        }
    }
}

