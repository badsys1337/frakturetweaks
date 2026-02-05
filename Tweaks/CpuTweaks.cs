using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Threading.Tasks;

namespace Frakture_Tweaks
{
    public class CpuTweaks
    {
        private LogWindow _logger;

        public CpuTweaks(LogWindow logger)
        {
            _logger = logger;
        }

        public async Task ApplyGlobalCpuTweaksAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    _logger.AddLog("Starting Global CPU Tweaks...");
                    List<string> batchCommands = new List<string>();

                    ApplyAdvancedCpuRegistryTweaks();

                    ApplyAdditionalCpuRegistryTweaks();

                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor IDLEPROMOTE 98");
                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor IDLEDEMOTE 98");
                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor IDLECHECK 20000");

                    batchCommands.Add("powercfg -setacvalueindex scheme_current SUB_SLEEP AWAYMODE 0");
                    batchCommands.Add("powercfg -setacvalueindex scheme_current SUB_SLEEP ALLOWSTANDBY 0");
                    batchCommands.Add("powercfg -setacvalueindex scheme_current SUB_SLEEP HYBRIDSLEEP 0");
                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor PROCTHROTTLEMIN 100");

                    ApplyCpuIdlePowerManagementTweaks();

                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_none DEVICEIDLE 0");

                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor IDLESCALING 1");

                    

                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor PERFEPP 0");

                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor THROTTLING 0");

                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor PERFBOOSTMODE 1");
                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor PERFBOOSTPOL 100");

                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor CPMINCORES 100");

                    batchCommands.Add("powercfg /setACvalueindex scheme_current SUB_PROCESSOR SYSCOOLPOL 1");
                    batchCommands.Add("powercfg /setDCvalueindex scheme_current SUB_PROCESSOR SYSCOOLPOL 1");

                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor PROCTHROTTLEMAX 100");
                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor PROCTHROTTLEMIN 100");

                    batchCommands.Add("powercfg -setdcvalueindex scheme_current sub_processor PROCTHROTTLEMAX 100");
                    batchCommands.Add("powercfg -setdcvalueindex scheme_current sub_processor PROCTHROTTLEMIN 50");
                    
                    
                    

                     var bcdManager = new SystemTweakManager(_logger);
                     await bcdManager.ApplyBootTweaksAsync();
                     await bcdManager.ApplyLatencyTweaksAsync();

                    string guid1 = "95533644-e700-4a79-a56c-a89e8cb109d9";
                    string guid2 = "238c9fa8-0aad-41ed-83f4-97be242c8f20";
                    string guid3 = "25dfa149-5dd1-4736-b5ab-e8a37b5b8187";
                    batchCommands.Add($"powercfg -setacvalueindex {guid1} {guid2} {guid3} 0");
                    batchCommands.Add($"powercfg -setdcvalueindex {guid1} {guid2} {guid3} 0");

                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor PERFAUTONOMOUS 1");
                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor PERFAUTONOMOUSWINDOW 20000");
                    batchCommands.Add("powercfg -setacvalueindex scheme_current sub_processor PERFCHECK 20");

                    batchCommands.Add("powercfg /setacvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 ea062031-0e34-4ff1-9b6d-eb1059334028 100");
                    batchCommands.Add("powercfg /setdcvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 ea062031-0e34-4ff1-9b6d-eb1059334028 100");
                    batchCommands.Add("powercfg /setacvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 68dd2f27-a4ce-4e11-8487-3794e4135dfa 1");
                    batchCommands.Add("powercfg /setdcvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 68dd2f27-a4ce-4e11-8487-3794e4135dfa 1");
                    batchCommands.Add("powercfg /setacvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 c7be0679-2817-4d69-9d02-519a537ed0c6 90");
                    batchCommands.Add("powercfg /setdcvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 c7be0679-2817-4d69-9d02-519a537ed0c6 90");
                    batchCommands.Add("powercfg /setacvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 6c2993b0-8f48-481f-bcc6-00dd2742aa06 1");
                    batchCommands.Add("powercfg /setdcvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 6c2993b0-8f48-481f-bcc6-00dd2742aa06 1");
                    batchCommands.Add("powercfg /setacvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 7b224883-b3cc-4d79-819f-8374152cbe7c 1");
                    batchCommands.Add("powercfg /setdcvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 7b224883-b3cc-4d79-819f-8374152cbe7c 1");
                    batchCommands.Add("powercfg /setacvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 93b8b6dc-0698-4d1c-9ee4-0644e900c85d 5");
                    batchCommands.Add("powercfg /setdcvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 93b8b6dc-0698-4d1c-9ee4-0644e900c85d 5");
                    batchCommands.Add("powercfg /setacvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 bae08b81-2d5e-4688-ad6a-13243356654b 0");
                    batchCommands.Add("powercfg /setdcvalueindex scheme_current 54533251-82be-4824-96c1-47b60b740d00 bae08b81-2d5e-4688-ad6a-13243356654b 0");

                    batchCommands.Add("powercfg /setactive SCHEME_CURRENT");

                    ExecuteBatch(batchCommands);

                    _logger.AddLog("Global CPU Tweaks Applied Successfully!");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error applying global tweaks: {ex.Message}");
                }
            });
        }

        private void ApplyAdditionalCpuRegistryTweaks()
        {
            try
            {
                SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "DistributeTimers", 1, RegistryValueKind.DWord);

                SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "Class2InitialUnparkCount", 100, RegistryValueKind.DWord);
                SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "EnergyEstimationDisabled", 1, RegistryValueKind.DWord);
                SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "PerfBoostAtGuaranteed", 1, RegistryValueKind.DWord);
                SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "PpmMfBufferingThreshold", 0, RegistryValueKind.DWord);
                SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "MfOverridesDisabled", 1, RegistryValueKind.DWord);
                SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "PpmMfOverridesDisabled", 1, RegistryValueKind.DWord);
                SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "CoalescingTimerInterval", 0, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Error applying additional CPU registry tweaks: {ex.Message}");
            }
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
                    RedirectStandardError = true
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

        public async Task ApplyIntelCpuTweaksAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    _logger.AddLog("Applying Intel CPU Tweaks...");

                     var bcdManager = new SystemTweakManager(_logger);
                     await bcdManager.ApplyMemoryTweaksAsync();

                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "DistributeTimers", 1, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "DisableTsx", 0, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power", "CoalescingTimerInterval", 0, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "EnergyEstimationEnabled", 0, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "EventProcessorEnabled", 0, RegistryValueKind.DWord);

                    _logger.AddLog("Intel CPU Tweaks Applied Successfully!");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error applying Intel tweaks: {ex.Message}");
                }
            });
        }

        public async Task ApplyAmdCpuTweaksAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Applying AMD CPU Tweaks...");

                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "DistributeTimers", 1, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "DisableTsx", 1, RegistryValueKind.DWord);

                    
                    ApplyAmdClassTweaks();

                    _logger.AddLog("AMD CPU Tweaks Applied Successfully!");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error applying AMD tweaks: {ex.Message}");
                }
            });
        }

        private void ApplyAdvancedCpuRegistryTweaks()
        {
             _logger.AddLog("Applying Advanced CPU Registry Tweaks...");
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings\54533251-82be-4824-96c1-47b60b740d00\943c8cb6-6f93-4227-ad87-e9a3feec08d1", "Attributes", 2, RegistryValueKind.DWord);
            
            
            string pwrSettingsPath = @"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerSettings";
            string pwrValuesPath = @"DefaultPowerSchemeValues\381b4222-f694-41f0-9685-ff5bb260df2e";
            string pwrValuesPath2 = @"DefaultPowerSchemeValues\8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c";
            
            SetRegistryValue($@"{pwrSettingsPath}\2a737441-1930-4402-8d77-b2bebba308a3\d4e98f31-5ffe-4ce1-be31-1b38b384c009\{pwrValuesPath}", "ACSettingIndex", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{pwrSettingsPath}\2a737441-1930-4402-8d77-b2bebba308a3\d4e98f31-5ffe-4ce1-be31-1b38b384c009\{pwrValuesPath}", "DCSettingIndex", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{pwrSettingsPath}\2a737441-1930-4402-8d77-b2bebba308a3\d4e98f31-5ffe-4ce1-be31-1b38b384c009\{pwrValuesPath2}", "ACSettingIndex", 0, RegistryValueKind.DWord);
            
            SetRegistryValue($@"{pwrSettingsPath}\54533251-82be-4824-96c1-47b60b740d00\3b04d4fd-1cc7-4f23-ab1c-d1337819c4bb\{pwrValuesPath}", "ACSettingIndex", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{pwrSettingsPath}\54533251-82be-4824-96c1-47b60b740d00\3b04d4fd-1cc7-4f23-ab1c-d1337819c4bb\{pwrValuesPath}", "DCSettingIndex", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{pwrSettingsPath}\54533251-82be-4824-96c1-47b60b740d00\3b04d4fd-1cc7-4f23-ab1c-d1337819c4bb\{pwrValuesPath2}", "ACSettingIndex", 0, RegistryValueKind.DWord);

            
            
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "HighPerformance", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "HighestPerformance", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "MinimumThrottlePercent", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "MaximumThrottlePercent", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "MaximumPerformancePercent", 100, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "Class1InitialUnparkCount", 100, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power", "InitialUnparkCount", 100, RegistryValueKind.DWord);
            
            SetRegistryValue(@"HKLM\SOFTWARE\Policies\Microsoft\Windows\WcmSvc\GroupPolicy", "fDisablePowerManagement", 1, RegistryValueKind.DWord);
            
            
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power\PDC\Activators\Default\VetoPolicy", "EA:EnergySaverEngaged", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power\PDC\Activators\28\VetoPolicy", "EA:PowerStateDischarging", 0, RegistryValueKind.DWord);
            
            string policySettings = @"HKLM\SYSTEM\CurrentControlSet\Control\Power\Policy\Settings";
            SetRegistryValue($@"{policySettings}\Misc", "DeviceIdlePolicy", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "PerfEnergyPreference", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "CPMinCores", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "CPMaxCores", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "CPMinCores1", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "CPMaxCores1", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "CpLatencyHintUnpark1", 100, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "CPDistribution", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "CpLatencyHintUnpark", 100, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "MaxPerformance1", 100, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "MaxPerformance", 100, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "CPDistribution1", 0, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "CPHEADROOM", 0, RegistryValueKind.DWord);

            byte[] policiesBinary = new byte[] {
                0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
                0x2c, 0x01, 0x00, 0x00, 0x32, 0x32, 0x03, 0x03, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x84, 0x03, 0x00, 0x00, 0x2c, 0x01, 0x00, 0x00, 
                0x00, 0x00, 0x00, 0x00, 0x84, 0x03, 0x00, 0x00, 0x00, 0x01, 0x64, 0x64, 0x64, 0x64, 0x00, 0x00
            };
            SetRegistryValue(@"HKCU\Control Panel\PowerCfg\GlobalPowerPolicy", "Policies", policiesBinary, RegistryValueKind.Binary);

            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1, RegistryValueKind.DWord);
            SetRegistryValue($@"{policySettings}\Processor", "CPCONCURRENCY", 0, RegistryValueKind.DWord);

            SetRegistryValue(@"HKLM\SYSTEM\ControlSet001\Control\Processor", "ProcessorThrottlingEnabled", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ControlSet001\Control\Processor", "CpuIdleThreshold", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ControlSet001\Control\Processor", "CpuIdle", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ControlSet001\Control\Processor", "CpuLatencyTimer", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ControlSet001\Control\Processor", "CpuSlowdown", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ControlSet001\Control\Processor", "Threshold", 1, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ControlSet001\Control\Processor", "CpuDebuggingEnabled", 0, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\ControlSet001\Control\Processor", "ProcessorLatencyThrottlingEnabled", 0, RegistryValueKind.DWord);
        }

        private void ApplyCpuIdlePowerManagementTweaks()
        {
             
             
             _logger.AddLog("CPU Idle Power Management: Using default Windows settings.");
        }

        private void ApplyAmdClassTweaks()
        {
             
             try 
             {
                 using (RegistryKey root = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class"))
                 {
                     if (root != null)
                     {
                         foreach (string classGuid in root.GetSubKeyNames())
                         {
                             using (RegistryKey classKey = root.OpenSubKey(classGuid))
                             {
                                 if (classKey != null)
                                 {
                                     foreach (string instanceId in classKey.GetSubKeyNames())
                                     {
                                         
                                         if (instanceId.Length == 4 && int.TryParse(instanceId, out _))
                                         {
                                             using (RegistryKey instanceKey = classKey.OpenSubKey(instanceId, true))
                                             {
                                                 if (instanceKey != null)
                                                 {
                                                     SetIfValueExists(instanceKey, "WakeEnabled", 0);
                                                     SetIfValueExists(instanceKey, "WdkSelectiveSuspendEnable", 0);
                                                 }
                                             }
                                         }
                                     }
                                 }
                             }
                         }
                     }
                 }
             }
             catch(Exception ex)
             {
                 _logger.AddLog($"Error applying AMD Class tweaks: {ex.Message}");
             }
        }

        private void SetIfValueExists(RegistryKey key, string valueName, int value)
        {
            if (key.GetValue(valueName) != null)
            {
                try {
                    key.SetValue(valueName, value, RegistryValueKind.DWord);
                    _logger.AddLog($"Set {valueName}=0 in {key.Name}");
                } catch {}
            }
        }

        private void RunPowerCfg(string arguments)
        {
            RunProcess("powercfg.exe", arguments);
        }

        private void RunProcess(string fileName, string arguments)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(fileName, arguments);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;

                using (Process? p = Process.Start(psi))
                {
                    if (p != null)
                    {
                        
                        var outputTask = p.StandardOutput.ReadToEndAsync();
                        var errorTask = p.StandardError.ReadToEndAsync();

                        p.WaitForExit();
                        
                        Task.WaitAll(outputTask, errorTask);
                    }
                }
                _logger.AddLog($"Executed: {fileName} {arguments}");
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Failed: {fileName} {arguments} - {ex.Message}");
            }
        }

        private void RunCommand(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                
                using (Process? p = Process.Start(psi))
                {
                    if (p != null)
                    {
                        
                        var outputTask = p.StandardOutput.ReadToEndAsync();
                        var errorTask = p.StandardError.ReadToEndAsync();

                        p.WaitForExit();
                        
                        Task.WaitAll(outputTask, errorTask);
                    }
                }
                _logger.AddLog($"Executed: {command}");
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Failed: {command} - {ex.Message}");
            }
        }

        private void SetRegistryValue(string keyPath, string valueName, object value, RegistryValueKind valueKind)
        {
            try
            {
                string root = keyPath.Split('\\')[0];
                string subKey = keyPath.Substring(root.Length + 1);

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
                        key.SetValue(valueName, value, valueKind);
                        _logger.AddLog($"Registry Set: {valueName} = {value}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Registry Error ({keyPath}): {ex.Message}");
            }
        }
    }
}

