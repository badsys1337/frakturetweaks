using Microsoft.Win32;
using System;
using System.Threading.Tasks;

namespace Frakture_Tweaks
{
    public class InputBoostService
    {
        private LogWindow _logger;

        public InputBoostService(LogWindow logger)
        {
            _logger = logger;
        }

        public async Task ApplyInputBoostAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Applying Input Boost & Latency Tweaks...");

                    
                    _logger.AddLog("Optimizing Mouse Settings (No Accel)...");
                    SetRegistryStringCU(@"Control Panel\Mouse", "MouseSpeed", "0");
                    SetRegistryStringCU(@"Control Panel\Mouse", "MouseThreshold1", "0");
                    SetRegistryStringCU(@"Control Panel\Mouse", "MouseThreshold2", "0");
                    SetRegistryStringCU(@"Control Panel\Mouse", "MouseSensitivity", "10");

                    
                    _logger.AddLog("Disabling Prefetcher & Superfetch...");
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnablePrefetcher", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnableSuperfetch", 0);

                    
                    _logger.AddLog("Setting CSRSS Priority High...");
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe\PerfOptions", "CpuPriorityClass", 3);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe\PerfOptions", "IoPriority", 3);

                    
                    _logger.AddLog("Disabling GameDVR & Fullscreen Optimizations...");
                    SetRegistryValueCU(@"System\GameConfigStore", "GameDVR_Enabled", 0);
                    SetRegistryValueCU(@"System\GameConfigStore", "GameDVR_FSEBehaviorMode", 2);
                    SetRegistryValueCU(@"System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 1);
                    SetRegistryValueCU(@"System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 1);

                    
                    _logger.AddLog("Disabling MouseKeys...");
                    SetRegistryStringCU(@"Control Panel\Accessibility\MouseKeys", "Flags", "0");

                    
                    _logger.AddLog("Applying Extra Delay Tweaks (Queue Size, Win32Priority)...");
                    
                    
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Services\kbdclass\Parameters", "KeyboardDataQueueSize", 20); 
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Services\mouclass\Parameters", "MouseDataQueueSize", 20);

                    
                    
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 38); 

                    
                    SetRegistryStringCU(@"Control Panel\Accessibility\StickyKeys", "Flags", "506");

                    _logger.AddLog("Input Boost Applied Successfully!");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error applying Input Boost: {ex.Message}");
                }
            });
        }

        public async Task RevertInputBoostAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Reverting Input Boost...");
                    SetRegistryStringCU(@"Control Panel\Mouse", "MouseSpeed", "1");
                    SetRegistryStringCU(@"Control Panel\Mouse", "MouseThreshold1", "6");
                    SetRegistryStringCU(@"Control Panel\Mouse", "MouseThreshold2", "10");
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters", "EnablePrefetcher", 3);
                    _logger.AddLog("Input Boost Reverted.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error reverting Input Boost: {ex.Message}");
                }
            });
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
    }
}

