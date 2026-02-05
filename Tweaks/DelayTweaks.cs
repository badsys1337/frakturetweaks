using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Frakture_Tweaks
{
    public class DelayTweaks
    {
        private LogWindow _logger;

        public DelayTweaks(LogWindow logger)
        {
            _logger = logger;
        }

        public async Task DemolishDelayAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    ApplyDataQueueSize();
                    ApplyThreadPriority();
                    ApplyDeviceThreadPriorities();
                    ApplyCsrssRealtime();
                    ApplyKernelLatencyTweaks();
                    DisableMouseAcceleration();
                    ReduceKeyboardRepeatDelay();
                    OptimizeMouseControlPanel();
                    EnableOneToOnePixelMouse();
                    DisableFilterKeys();
                    DisableUSBSelectiveSuspend();
                    OptimizeGPUPriority();
                    OptimizeSystemPriority();
                    ApplyIrqPriorities();
                    ApplyPowerTweaks();
                    ApplyMultimediaTweaks();
                    ApplyNetworkTweaks();
                    ApplyResourcePolicyStoreTweaks();
                    ApplyBcdTweaks();
                    ApplyGpuThreadPriority();
                    DisableFSO();
                    DisableGameBar();
                    DisableMPO();

                    _logger.AddLog("All delay tweaks applied successfully.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error applying tweaks: {ex.Message}");
                }
            });
        }

        public async Task ApplyBcdLatencyTweaksAsync()
        {
            var bcdManager = new SystemTweakManager(_logger);
            await bcdManager.ApplyAllBcdTweaksAsync(false); 
        }

        public async Task ApplyGpuMonitorLatencyTweaksAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Optimizing GPU & Monitor Latency...");
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\DXGKrnl", "MonitorLatencyTolerance", 0, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\DXGKrnl", "MonitorRefreshLatencyTolerance", 0, RegistryValueKind.DWord);
                    _logger.AddLog("GPU & Monitor Latency Optimized.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error: {ex.Message}");
                }
            });
        }

        public async Task DisableMitigationsAsync()
        {
            
            await Task.CompletedTask;
        }

        public async Task EnableTrimOptimizationAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Enabling TRIM Optimization...");
                    RunCommand("fsutil", "behavior set DisableDeleteNotify 0");
                    _logger.AddLog("TRIM Optimization Enabled.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error: {ex.Message}");
                }
            });
        }

        public async Task OptimizeMouseHidAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Optimizing Mouse & HID Response...");
                    SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseSpeed", "0", RegistryValueKind.String);
                    SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseThreshold1", "0", RegistryValueKind.String);
                    SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseThreshold2", "0", RegistryValueKind.String);

                    
                    RunCommand("netsh", "int tcp set global autotuninglevel=normal");
                    RunCommand("netsh", "interface 6to4 set state disabled");
                    RunCommand("netsh", "int tcp set global timestamps=disabled");
                    RunCommand("netsh", "int tcp set heuristics disabled");
                    RunCommand("netsh", "int tcp set global chimney=disabled");
                    RunCommand("netsh", "int tcp set global ecncapability=disabled");
                    RunCommand("netsh", "int tcp set global rsc=disabled");
                    RunCommand("netsh", "int tcp set global nonsackrttresiliency=disabled");

                    
                    RunCommand("powershell", "-Command \"ForEach($adapter In Get-NetAdapter){Disable-NetAdapterPowerManagement -Name $adapter.Name -ErrorAction SilentlyContinue}\"");
                    
                    _logger.AddLog("Mouse & HID Optimized.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error: {ex.Message}");
                }
            });
        }

        private void SetRegistryValue(string keyPath, string valueName, object value, RegistryValueKind kind)
        {
            try
            {
                string root = keyPath.Split('\\')[0];
                string subKey = keyPath.Contains("\\") ? keyPath.Substring(root.Length + 1) : "";

                RegistryKey baseKey;
                switch (root.ToUpper())
                {
                    case "HKLM":
                    case "HKEY_LOCAL_MACHINE":
                        baseKey = Registry.LocalMachine;
                        break;
                    case "HKCU":
                    case "HKEY_CURRENT_USER":
                        baseKey = Registry.CurrentUser;
                        break;
                    case "HKU":
                    case "HKEY_USERS":
                        baseKey = Registry.Users;
                        break;
                    default:
                        _logger.AddLog($"Unknown registry root: {root}");
                        return;
                }

                using (RegistryKey key = baseKey.CreateSubKey(subKey))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, kind);
                        _logger.AddLog($"Registry Set: {keyPath}\\{valueName} = {value}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Registry Error: {keyPath}\\{valueName} - {ex.Message}");
            }
        }

        private void RunCommand(string command, string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(command, arguments)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Verb = "runas"
                };
                
                using (var process = new Process { StartInfo = psi })
                {
                    process.Start();
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();
                    
                    process.WaitForExit();
                    
                    Task.WaitAll(outputTask, errorTask);
                }
                
                _logger.AddLog($"Executed: {command} {arguments}");
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Command Failed: {command} {arguments} - {ex.Message}");
            }
        }

        private void ApplyPowerTweaks()
        {
            _logger.AddLog("Optimizing Power Throttling & Processor...");

            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "PowerThrottlingOff", 1, RegistryValueKind.DWord);

            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "CoalescingTimerInterval", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager", "CoalescingTimerInterval", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "CoalescingTimerInterval", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "CoalescingTimerInterval", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Executive", "CoalescingTimerInterval", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power\ModernSleep", "CoalescingTimerInterval", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "CoalescingTimerInterval", 0, RegistryValueKind.DWord);

            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "EnergyEstimationEnabled", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "EventProcessorEnabled", 0, RegistryValueKind.DWord);
        }

        private void ApplyMultimediaTweaks()
        {
            _logger.AddLog("Optimizing Multimedia System Profile...");

            string profilePath = @"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
            SetRegistryValue(profilePath, "SystemResponsiveness", 10, RegistryValueKind.DWord);
            SetRegistryValue(profilePath, "NetworkThrottlingIndex", -1, RegistryValueKind.DWord);

            string gamesPath = $@"{profilePath}\Tasks\Games";
            SetRegistryValue(gamesPath, "Affinity", 0, RegistryValueKind.DWord);
            SetRegistryValue(gamesPath, "Background Only", "False", RegistryValueKind.String);
            SetRegistryValue(gamesPath, "BackgroundPriority", 0, RegistryValueKind.DWord);
            SetRegistryValue(gamesPath, "Clock Rate", 10000, RegistryValueKind.DWord);
            SetRegistryValue(gamesPath, "GPU Priority", 18, RegistryValueKind.DWord);
            SetRegistryValue(gamesPath, "Priority", 6, RegistryValueKind.DWord);
            SetRegistryValue(gamesPath, "Scheduling Category", "High", RegistryValueKind.String);
            SetRegistryValue(gamesPath, "SFIO Priority", "High", RegistryValueKind.String);
            SetRegistryValue(gamesPath, "Latency Sensitive", "True", RegistryValueKind.String);

            string lowLatPath = $@"{profilePath}\Tasks\Low Latency";
            SetRegistryValue(lowLatPath, "Affinity", 0, RegistryValueKind.DWord);
            SetRegistryValue(lowLatPath, "Background Only", "False", RegistryValueKind.String);
            SetRegistryValue(lowLatPath, "BackgroundPriority", 0, RegistryValueKind.DWord);
            SetRegistryValue(lowLatPath, "Clock Rate", 10000, RegistryValueKind.DWord);
            SetRegistryValue(lowLatPath, "GPU Priority", 8, RegistryValueKind.DWord);
            SetRegistryValue(lowLatPath, "Priority", 2, RegistryValueKind.DWord);
            SetRegistryValue(lowLatPath, "Scheduling Category", "Medium", RegistryValueKind.String);
            SetRegistryValue(lowLatPath, "SFIO Priority", "High", RegistryValueKind.String);
            SetRegistryValue(lowLatPath, "Latency Sensitive", "True", RegistryValueKind.String);

            string wmPath = $@"{profilePath}\Tasks\Window Manager";
            SetRegistryValue(wmPath, "Background Only", "True", RegistryValueKind.String);
            SetRegistryValue(wmPath, "Clock Rate", 10000, RegistryValueKind.DWord);
            SetRegistryValue(wmPath, "GPU Priority", 8, RegistryValueKind.DWord);
            SetRegistryValue(wmPath, "Priority", 5, RegistryValueKind.DWord);
            SetRegistryValue(wmPath, "Scheduling Category", "Medium", RegistryValueKind.String);
        }

        private void ApplyNetworkTweaks()
        {
            _logger.AddLog("Optimizing Network Latency (TCP)...");
            
            try 
            {
                SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "TcpAckFrequency", 1, RegistryValueKind.DWord);
                SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces", "TcpAckFrequency", 1, RegistryValueKind.DWord);

                using (RegistryKey? interfaces = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces"))
                {
                    if (interfaces != null)
                    {
                        foreach (string subkeyName in interfaces.GetSubKeyNames())
                        {
                            string path = $@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces\{subkeyName}";
                            SetRegistryValue(path, "TcpNoDelay", 1, RegistryValueKind.DWord);
                            SetRegistryValue(path, "TcpAckFrequency", 1, RegistryValueKind.DWord);
                            SetRegistryValue(path, "TcpDelAckTicks", 0, RegistryValueKind.DWord);
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                _logger.AddLog($"Network Tweaks Warning: {ex.Message}");
            }
        }

        private void ApplyBcdTweaks()
        {
            var bcdManager = new SystemTweakManager(_logger);
            bcdManager.ApplyLatencyTweaksAsync().Wait(); 
        }

        private void ApplyKernelLatencyTweaks()
        {
            _logger.AddLog("Applying Kernel Latency Tweaks...");
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "DistributeTimers", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "ThreadDpcEnable", 1, RegistryValueKind.DWord);
        }
 
        private void ApplyResourcePolicyStoreTweaks()
        {
            _logger.AddLog("Applying ResourcePolicyStore CPU Tweaks...");
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\HardCap0", "CapPercentage", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\HardCap0", "SchedulingType", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\Paused", "CapPercentage", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\Paused", "SchedulingType", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\SoftCapFull", "CapPercentage", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\SoftCapFull", "SchedulingType", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\SoftCapLow", "CapPercentage", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\SoftCapLow", "SchedulingType", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\BackgroundDefault", "IsLowPriority", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\Frozen", "IsLowPriority", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\FrozenDNCS", "IsLowPriority", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\FrozenDNK", "IsLowPriority", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\PrelaunchForeground", "IsLowPriority", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\ThrottleGPUInterference", "IsLowPriority", 0, RegistryValueKind.DWord);
        }

        private void ApplyDeviceThreadPriorities()
        {
            _logger.AddLog("Optimizing Device Thread Priorities...");
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\usbxhci\Parameters", "ThreadPriority", 15, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\USBHUB3\Parameters", "ThreadPriority", 15, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\NDIS\Parameters", "ThreadPriority", 15, RegistryValueKind.DWord);
        }

        private void ExecuteBatch(List<string> commands)
        {
            if (commands == null || commands.Count == 0) return;

            string tempFile = Path.GetTempFileName() + ".bat";
            try
            {
                var batchContent = new List<string> { "@echo off" };
                batchContent.AddRange(commands);
                File.WriteAllLines(tempFile, batchContent);
                
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{tempFile}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    Verb = "runas"
                };

                using (var process = new Process { StartInfo = psi })
                {
                    process.Start();

                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    process.WaitForExit();
                    
                    Task.WaitAll(outputTask, errorTask);
                }
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Batch execution failed: {ex.Message}");
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }
            }
        }

        private void ApplyGpuThreadPriority()
        {
            _logger.AddLog("Optimizing GPU Driver Thread Priorities...");
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\DXGKrnl\Parameters", "ThreadPriority", 15, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\nvlddmkm\Parameters", "ThreadPriority", 15, RegistryValueKind.DWord);
        }

        private void ApplyDataQueueSize()
        {
            _logger.AddLog("Optimizing Data Queue Sizes...");
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\mouclass\Parameters", "MouseDataQueueSize", 19, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\kbdclass\Parameters", "KeyboardDataQueueSize", 19, RegistryValueKind.DWord);
        }

        private void ApplyThreadPriority()
        {
            _logger.AddLog("Optimizing Thread Priorities...");
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\mouclass\Parameters", "ThreadPriority", 15, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\kbdclass\Parameters", "ThreadPriority", 15, RegistryValueKind.DWord);
        }
 
        private void ApplyCsrssRealtime()
        {
            _logger.AddLog("Setting CSRSS to High Priority...");
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "DebugPollInterval", 1000, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe\PerfOptions", "CpuPriorityClass", 3, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe\PerfOptions", "IoPriority", 3, RegistryValueKind.DWord);
        }

        private void DisableMouseAcceleration()
        {
            _logger.AddLog("Disabling Mouse Acceleration...");
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseSpeed", "0", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseThreshold1", "0", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseThreshold2", "0", RegistryValueKind.String);
            
            _logger.AddLog("Optimizing Mouse HID (safe defaults)...");
            
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\mouhid\Parameters", "TreatAbsolutePointerAsAbsolute", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\mouhid\Parameters", "TreatAbsoluteAsRelative", 0, RegistryValueKind.DWord);
        }

        private void ReduceKeyboardRepeatDelay()
        {
            _logger.AddLog("Reducing Keyboard Repeat Delay...");
            SetRegistryValue(@"HKCU\Control Panel\Keyboard", "KeyboardDelay", "0", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Keyboard", "KeyboardSpeed", "31", RegistryValueKind.String);
        }

        private void OptimizeMouseControlPanel()
        {
            _logger.AddLog("Optimizing Control Panel Mouse Settings...");
            
            
            SetRegistryValue(@"HKU\.DEFAULT\Control Panel\Desktop", "ForegroundLockTimeout", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKU\.DEFAULT\Control Panel\Desktop", "MenuShowDelay", "0", RegistryValueKind.String);
            SetRegistryValue(@"HKU\.DEFAULT\Control Panel\Desktop", "MouseWheelRouting", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKU\.DEFAULT\Control Panel\Mouse", "Beep", "No", RegistryValueKind.String);
            SetRegistryValue(@"HKU\.DEFAULT\Control Panel\Mouse", "ExtendedSounds", "No", RegistryValueKind.String);
            SetRegistryValue(@"HKU\.DEFAULT\Control Panel\Sound", "Beep", "no", RegistryValueKind.String);
            SetRegistryValue(@"HKU\.DEFAULT\Control Panel\Sound", "ExtendedSounds", "no", RegistryValueKind.String);

            
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "ActiveWindowTracking", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "Beep", "No", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "DoubleClickHeight", "4", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "DoubleClickSpeed", "500", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "DoubleClickWidth", "4", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "ExtendedSounds", "No", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseHoverHeight", "4", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseHoverWidth", "4", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseSensitivity", "10", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseSpeed", "0", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseThreshold1", "0", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseThreshold2", "0", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseTrails", "0", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "SnapToDefaultButton", "0", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "SwapMouseButtons", "0", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseHoverTime", "8", RegistryValueKind.String);

            
        }

        private void EnableOneToOnePixelMouse()
        {
            _logger.AddLog("Enabling 1:1 Pixel Mouse Movements...");
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "MouseSensitivity", "10", RegistryValueKind.String);
            
            
            
            byte[] zeros = new byte[40]; 
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "SmoothMouseXCurve", zeros, RegistryValueKind.Binary);
            SetRegistryValue(@"HKCU\Control Panel\Mouse", "SmoothMouseYCurve", zeros, RegistryValueKind.Binary);
        }

        private void DisableFilterKeys()
        {
            _logger.AddLog("Disabling Filter Keys & Accessibility delays...");
            SetRegistryValue(@"HKCU\Control Panel\Accessibility\Keyboard Response", "Flags", "122", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Accessibility\ToggleKeys", "Flags", "58", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Accessibility\StickyKeys", "Flags", "506", RegistryValueKind.String);
            SetRegistryValue(@"HKCU\Control Panel\Accessibility\MouseKeys", "Flags", "0", RegistryValueKind.String);
        }

        private void DisableUSBSelectiveSuspend()
        {
            _logger.AddLog("Disabling USB Selective Suspend...");
            try
            {
                using (RegistryKey? usbKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB"))
                {
                    if (usbKey != null)
                    {
                        foreach (string deviceId in usbKey.GetSubKeyNames())
                        {
                            using (RegistryKey? deviceKey = usbKey.OpenSubKey(deviceId))
                            {
                                if (deviceKey != null)
                                {
                                    foreach (string instanceId in deviceKey.GetSubKeyNames())
                                    {
                                        string deviceParamPath = $@"HKLM\SYSTEM\CurrentControlSet\Enum\USB\{deviceId}\{instanceId}\Device Parameters";
                                        SetRegistryValue(deviceParamPath, "SelectiveSuspendOn", 0, RegistryValueKind.DWord);
                                        SetRegistryValue(deviceParamPath, "AllowIdleIrpInD3", 0, RegistryValueKind.DWord);
                                        SetRegistryValue(deviceParamPath, "EnhancedPowerManagementEnabled", 0, RegistryValueKind.DWord);
                                        SetRegistryValue(deviceParamPath, "DeviceSelectiveSuspended", 0, RegistryValueKind.DWord);

                                        string wdfPath = deviceParamPath + @"\WDF";
                                        SetRegistryValue(wdfPath, "IdleInWorkingState", 0, RegistryValueKind.DWord);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Error iterating USB devices: {ex.Message}");
            }
        }

        private void OptimizeGPUPriority()
        {
            _logger.AddLog("Optimizing GPU Latency...");
            try
            {
                string classPath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
                using (RegistryKey? classKey = Registry.LocalMachine.OpenSubKey(classPath))
                {
                    if (classKey != null)
                    {
                        foreach (string subKeyName in classKey.GetSubKeyNames())
                        {
                            
                            if (subKeyName.Length == 4 && int.TryParse(subKeyName, out _))
                            {
                                string fullPath = $@"HKLM\{classPath}\{subKeyName}";
                                SetRegistryValue(fullPath, "LOWLATENCY", 1, RegistryValueKind.DWord);
                                SetRegistryValue(fullPath, "Node3DLowLatency", 1, RegistryValueKind.DWord);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Error optimizing GPU: {ex.Message}");
            }
        }

        private void OptimizeSystemPriority()
        {
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 38, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ControlSet001\Control\PriorityControl", "Win32PrioritySeparation", 38, RegistryValueKind.DWord);
        }

        private void ApplyIrqPriorities()
        {
            
        }

        private void DisableFSO()
        {
            _logger.AddLog("Disabling Fullscreen Optimizations (FSO)...");
            SetRegistryValue(@"HKCU\System\GameConfigStore", "GameDVR_FSEBehavior", 2, RegistryValueKind.DWord);
            SetRegistryValue(@"HKCU\System\GameConfigStore", "GameDVR_Enabled", 0, RegistryValueKind.DWord);
        }

        private void DisableGameBar()
        {
            _logger.AddLog("Disabling Game Bar & Captures...");
            SetRegistryValue(@"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR", "AudioCaptureEnabled", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKCU\SOFTWARE\Microsoft\GameBar", "UseNexusForGameBarEnabled", 0, RegistryValueKind.DWord);
        }

        private void DisableMPO()
        {
            _logger.AddLog("Disabling Multi-Plane Overlay (MPO) - Reduces stuttering...");
            SetRegistryValue(@"HKLM\SOFTWARE\Microsoft\Windows\Dwm", "OverlayTestMode", 0x00000005, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "MpoEnable", 0, RegistryValueKind.DWord);
        }
    }
}

