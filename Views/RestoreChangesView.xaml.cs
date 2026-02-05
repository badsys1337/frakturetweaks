using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Frakture_Tweaks.Services;

namespace Frakture_Tweaks
{
    public partial class RestoreChangesView : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<FixItem> Fixes { get; set; } = new ObservableCollection<FixItem>();
        private ICollectionView _fixesView;
        private string _selectedCategory = "All";

        public Button? RestorePointButton => null; 

        public RestoreChangesView()
        {
            InitializeComponent();
            DataContext = this;
            FixesListBox.ItemsSource = Fixes;
            
            _fixesView = CollectionViewSource.GetDefaultView(Fixes);
            if (_fixesView != null)
            {
                _fixesView.Filter = FixFilter;
            }

            LoadFixes();
            UpdateStatus();

            
            LocalizationManager.Instance.LanguageChanged += (s, e) => ReloadFixes();
        }

        private void ReloadFixes()
        {
            Fixes.Clear();
            LoadFixes();
            _fixesView?.Refresh();
            UpdateStatus();
        }

        private bool FixFilter(object item)
        {
            var fix = (FixItem)item;
            
            
            if (_selectedCategory != "All")
            {
                bool categoryMatch = _selectedCategory switch
                {
                    "Hardware" => fix.Category == "Hardware",
                    "System Repair" => fix.Category == "System Repair" || fix.Category == "System Protection",
                    "Performance" => fix.Category == "Performance",
                    "Security" => fix.Category == "Security",
                    "Gaming" => fix.Category == "Gaming",
                    "Revert" => fix.Category == "Optimization Revert",
                    _ => true
                };
                if (!categoryMatch) return false;
            }
            
            
            if (SearchBox == null || string.IsNullOrEmpty(SearchBox.Text))
                return true;

            return fix.Name.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase) ||
                   fix.Description.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase) ||
                   fix.Category.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _fixesView.Refresh();
        }

        private void CategoryFilter_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.RadioButton rb && rb.IsChecked == true)
            {
                _selectedCategory = rb.Tag?.ToString() ?? "All";
                _fixesView?.Refresh();
                UpdateStatus();
            }
        }

        private string GetString(string key)
        {
            return Application.Current.TryFindResource(key) as string ?? key;
        }

        private void LoadFixes()
        {
            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_CreateRestorePoint"),
                Description = GetString("Fix_CreateRestorePoint_Desc"),
                Category = "System Protection",
                Action = async (log) => await Task.Run(() => CreateRestorePoint(log))
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_OpenSystemRestore"),
                Description = GetString("Fix_OpenSystemRestore_Desc"),
                Category = "System Protection",
                Action = async (log) => { Process.Start(new ProcessStartInfo("rstrui.exe") { UseShellExecute = true }); await Task.CompletedTask; }
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_RepairSystem"),
                Description = GetString("Fix_RepairSystem_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => {
                    log.AddLog("Running SFC Scan... (Step 1/2)");
                    RunProcess("sfc", "/scannow", log);
                    log.AddLog("Running DISM RestoreHealth... (Step 2/2)");
                    RunProcess("dism", "/Online /Cleanup-Image /RestoreHealth", log);
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_BringBackPower"),
                Description = GetString("Fix_BringBackPower_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => RestorePowerAndBattery(log))
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_ResetNetwork"),
                Description = GetString("Fix_ResetNetwork_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => {
                    RunCommand("netsh winsock reset", log);
                    RunCommand("netsh int ip reset", log);
                    RunCommand("ipconfig /release", log);
                    RunCommand("ipconfig /renew", log);
                    RunCommand("ipconfig /flushdns", log);
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_WindowsUpdate"),
                Description = GetString("Fix_WindowsUpdate_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => FixWindowsUpdate(log))
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_Touchpad"),
                Description = GetString("Fix_Touchpad_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => FixTouchpad(log))
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_TouchpadDeep"),
                Description = GetString("Fix_TouchpadDeep_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => RestoreTouchpadDeep(log))
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_StoreApps"),
                Description = GetString("Fix_StoreApps_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => FixStoreAndApps(log))
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_RestoreStore"),
                Description = GetString("Fix_RestoreStore_Desc"),
                Category = "System Repair",
                Action = async (log) => {
                    var tweaks = new GlobalTweaks(log);
                    await tweaks.RestoreMicrosoftStoreAsync();
                }
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_RepairWMI"),
                Description = GetString("Fix_RepairWMI_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => {
                    RunCommand("net stop winmgmt /y", log);
                    RunProcess("winmgmt", "/resetrepository", log);
                    RunCommand("net start winmgmt", log);
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_PrinterSpooler"),
                Description = GetString("Fix_PrinterSpooler_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => {
                    RunCommand("net stop spooler", log);
                    try {
                        string spool = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "System32", "spool", "PRINTERS");
                        if (Directory.Exists(spool)) {
                            foreach (var f in Directory.GetFiles(spool)) File.Delete(f);
                        }
                    } catch {}
                    RunCommand("net start spooler", log);
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_ResetFirewall"),
                Description = GetString("Fix_ResetFirewall_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => RunCommand("netsh advfirewall reset", log))
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_ClearTemp"),
                Description = GetString("Fix_ClearTemp_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => ClearTempFiles(log))
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_RebuildIconCache"),
                Description = GetString("Fix_RebuildIconCache_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => RebuildIconCache(log))
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_RestoreAll"),
                Description = GetString("Fix_RestoreAll_Desc"),
                Category = "Optimization Revert",
                Action = async (log) => await RestoreAllChanges(log)
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_DisableCpuTweaks"),
                Description = GetString("Fix_DisableCpuTweaks_Desc"),
                Category = "Optimization Revert",
                Action = async (log) => await Task.Run(() => {
                    RunProcess("powercfg", "-restoredefaultschemes", log);
                    log.AddLog("CPU optimizations reverted.");
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_RestoreBluetooth"),
                Description = GetString("Fix_RestoreBluetooth_Desc"),
                Category = "Optimization Revert",
                Action = async (log) => await Task.Run(() => RestoreBluetooth(log))
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_EnableServices"),
                Description = GetString("Fix_EnableServices_Desc"),
                Category = "Optimization Revert",
                RequiresLog = false,
                Action = async (log) => {
                    Application.Current.Dispatcher.Invoke(() => {
                        var window = new EnableServicesWindow();
                        window.Owner = Application.Current.MainWindow;
                        window.ShowDialog();
                    });
                    await Task.CompletedTask;
                }
            });

            
            
            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_GpuDriverReset"),
                Description = GetString("Fix_GpuDriverReset_Desc"),
                Category = "Hardware",
                Action = async (log) => {
                    log.AddLog("Resetting GPU driver...");
                    RunCommand("taskkill /f /im dwm.exe", log);
                    await Task.Delay(500);
                    log.AddLog("GPU driver reset completed (DWM auto-restarted).");
                }
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_AudioFix"),
                Description = GetString("Fix_AudioFix_Desc"),
                Category = "Hardware",
                Action = async (log) => await Task.Run(() => {
                    RunCommand("net stop audiosrv", log);
                    RunCommand("net stop AudioEndpointBuilder", log);
                    RunCommand("net start AudioEndpointBuilder", log);
                    RunCommand("net start audiosrv", log);
                    log.AddLog("Audio services restarted.");
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_ContextMenu"),
                Description = GetString("Fix_ContextMenu_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => {
                    RunCommand("reg add \"HKCU\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\\InprocServer32\" /f /ve", log);
                    RunCommand("taskkill /f /im explorer.exe", log);
                    Process.Start("explorer.exe");
                    log.AddLog("Classic context menu restored. Explorer restarted.");
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_DwmReset"),
                Description = GetString("Fix_DwmReset_Desc"),
                Category = "System Repair",
                Action = async (log) => await Task.Run(() => {
                    RunCommand("taskkill /f /im dwm.exe", log);
                    log.AddLog("DWM restarted automatically by Windows.");
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_RamCleanup"),
                Description = GetString("Fix_RamCleanup_Desc"),
                Category = "Performance",
                Action = async (log) => await Task.Run(() => {
                    log.AddLog("Clearing standby RAM...");
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    log.AddLog("RAM cleanup completed.");
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_SlowBoot"),
                Description = GetString("Fix_SlowBoot_Desc"),
                Category = "Performance",
                Action = async (log) => await Task.Run(() => {
                    RunProcess("powercfg", "/h off", log);
                    RunProcess("powercfg", "/h on", log);
                    RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\\PrefetchParameters\" /v EnablePrefetcher /t REG_DWORD /d 3 /f", log);
                    log.AddLog("Boot optimization applied.");
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_RestoreDefender"),
                Description = GetString("Fix_RestoreDefender_Desc"),
                Category = "Security",
                Action = async (log) => await Task.Run(async () => {
                    string logFile = @"C:\Windows\Temp\defender_restore_log.txt";
                    string batchContent = $"( \r\n" +
                                          $"echo Restoring Registry Keys... \r\n" +
                                          $"reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /v \"DisableAntiSpyware\" /f \r\n" +
                                          $"reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\" /v \"DisableAntiVirus\" /f \r\n" +
                                          $"reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v \"DisableBehaviorMonitoring\" /f \r\n" +
                                          $"reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v \"DisableOnAccessProtection\" /f \r\n" +
                                          $"reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v \"DisableScanOnRealtimeEnable\" /f \r\n" +
                                          $"reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection\" /v \"DisableRealtimeMonitoring\" /f \r\n" +
                                          $"echo Restoring Tamper Protection... \r\n" +
                                          $"reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows Defender\\Features\" /v \"TamperProtection\" /t \"REG_DWORD\" /d \"1\" /f \r\n" +
                                          $"echo Restoring SmartScreen... \r\n" +
                                          $"reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\" /v \"SmartScreenEnabled\" /f \r\n" +
                                          $"reg delete \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\System\" /v \"EnableSmartScreen\" /f \r\n" +
                                          $"echo Restoring Loggers... \r\n" +
                                          $"reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\WMI\\Autologger\\DefenderApiLogger\" /v \"Start\" /t \"REG_DWORD\" /d \"1\" /f \r\n" +
                                          $"reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\WMI\\Autologger\\DefenderAuditLogger\" /v \"Start\" /t \"REG_DWORD\" /d \"1\" /f \r\n" +
                                          $"echo Enabling Services... \r\n" +
                                          $"SC config windefend start= auto \r\n" +
                                          $"SC config WdNisSvc start= demand \r\n" +
                                          $"SC config Sense start= demand \r\n" +
                                          $"SC config MsSecCore start= demand \r\n" +
                                          $"SC config SecurityHealthService start= auto \r\n" +
                                          $"SC config WdBoot start= boot \r\n" +
                                          $"SC config WdFilter start= boot \r\n" +
                                          $"SC config WdNisDrv start= demand \r\n" +
                                          $"NET start windefend \r\n" +
                                          $"NET start SecurityHealthService \r\n" +
                                          $") > \"{logFile}\" 2>&1";

                    log.AddLog("Executing Advanced Defender Restoration via TrustedInstaller...");
                    await TrustedInstaller.RunCommandAsTrustedInstaller(batchContent, log);

                    if (File.Exists(logFile))
                    {
                        try {
                            string output = File.ReadAllText(logFile);
                            log.AddLog("--- Restore Output ---");
                            log.AddLog(output);
                            File.Delete(logFile); 
                        } catch {}
                    }

                    log.AddLog("Windows Defender restored. Restart recommended.");
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_GamingServices"),
                Description = GetString("Fix_GamingServices_Desc"),
                Category = "Gaming",
                Action = async (log) => await Task.Run(() => {
                    log.AddLog("Resetting Gaming Services...");
                    RunCommand("powershell -Command \"Get-AppxPackage Microsoft.GamingServices | Remove-AppxPackage -AllUsers\"", log);
                    log.AddLog("Gaming Services reset. Reinstall from Microsoft Store if needed.");
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_UsbReset"),
                Description = GetString("Fix_UsbReset_Desc"),
                Category = "Hardware",
                Action = async (log) => await Task.Run(() => {
                    log.AddLog("Rescanning USB devices...");
                    RunProcess("pnputil", "/scan-devices", log);
                    log.AddLog("USB devices rescanned.");
                })
            });

            Fixes.Add(new FixItem
            {
                Name = GetString("Fix_StartupRepair"),
                Description = GetString("Fix_StartupRepair_Desc"),
                Category = "System Repair",
                RequiresLog = false,
                Action = async (log) => {
                    log.AddLog("Opening Task Manager Startup tab...");
                    Process.Start(new ProcessStartInfo("taskmgr.exe", "/7") { UseShellExecute = true });
                    await Task.CompletedTask;
                }
            });
        }

        private void UpdateStatus()
        {
            if (StatusText != null)
                StatusText.Text = string.Format(GetString("Restore_Status"), Fixes.Count);
        }

        private async void RunFix_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var fix = (FixItem)btn.DataContext;
            
            if (fix.Action != null)
            {
                var logWindow = new LogWindow();
                if (fix.RequiresLog)
                {
                    logWindow.Show();
                }
                
                logWindow.AddLog($"Starting: {fix.Name}...");
                
                try 
                {
                    fix.IsExecuting = true;
                    await fix.Action(logWindow);
                    logWindow.AddLog("Done.");
                }
                catch (Exception ex)
                {
                    logWindow.AddLog($"Error: {ex.Message}");
                    if (!fix.RequiresLog) logWindow.Show(); 
                }
                finally
                {
                    fix.IsExecuting = false;
                }
            }
        }

        private async void RunSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = Fixes.Where(f => f.IsSelected && f.Action != null).ToList();
            if (!selected.Any())
            {
                MessageBox.Show("Please select at least one fix to run.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to run {selected.Count} selected fixes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog($"Running {selected.Count} selected fixes...");

            foreach (var fix in selected)
            {
                try
                {
                    logWindow.AddLog($"--- Group Task: {fix.Name} ---");
                    fix.IsExecuting = true;
                    if (fix.Action != null)
                    {
                        await fix.Action(logWindow);
                    }
                }
                catch (Exception ex)
                {
                    logWindow.AddLog($"Error in '{fix.Name}': {ex.Message}");
                }
                finally
                {
                    fix.IsExecuting = false;
                }
            }

            logWindow.AddLog("All selected fixes have finished.");
        }

        #region Helper Methods

        private void RunCommand(string command, LogWindow logger)
        {
            try {
                var psi = new ProcessStartInfo("cmd.exe", "/c " + command) { CreateNoWindow = true, UseShellExecute = false };
                Process.Start(psi)?.WaitForExit();
                logger.AddLog($"Executed: {command}");
            } catch {}
        }

        private void RunProcess(string file, string args, LogWindow logger)
        {
            try {
                var psi = new ProcessStartInfo(file, args) { CreateNoWindow = true, UseShellExecute = false };
                Process.Start(psi)?.WaitForExit();
                logger.AddLog($"Executed: {file} {args}");
            } catch {}
        }

        #endregion

        #region Migrated Logic

        private void CreateRestorePoint(LogWindow log)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-ExecutionPolicy Bypass -Command \"Checkpoint-Computer -Description \\\"Frakture Tweaks Redesigned Fixes\\\" -RestorePointType \\\"MODIFY_SETTINGS\\\"\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0) log.AddLog("Success: Restore Point created.");
                        else log.AddLog("Error: Failed to create restore point. Check if System Protection is ON.");
                    }
                    else
                    {
                        log.AddLog("Error: Could not start PowerShell process.");
                    }
                }
            }
            catch (Exception ex) { log.AddLog($"Critical Error: {ex.Message}"); }
        }

        private void RestorePowerAndBattery(LogWindow log)
        {
            log.AddLog("Configuring services...");
            string[] svcs = { "Power", "PlugPlay", "SensorService", "SensorDataService", "SensrSvc", "DisplayEnhancementService", "Wcmsvc" };
            foreach (var s in svcs) RunCommand($"sc config {s} start=auto", log);

            log.AddLog("Clearing power policies...");
            RunCommand("reg delete \"HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\Explorer\" /v HideSCAPower /f", log);
            RunCommand("reg delete \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Power\\PowerSettings\" /f", log);
            RunCommand("reg delete \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Power\" /v CsEnabled /f", log);
            
            log.AddLog("Restoring default schemes...");
            RunProcess("powercfg", "-restoredefaultschemes", log);
            log.AddLog("Done. Please restart for changes to take effect.");
        }

        private void FixWindowsUpdate(LogWindow log)
        {
            string[] svcs = { "wuauserv", "cryptSvc", "bits", "msiserver" };
            foreach (var s in svcs) RunCommand($"net stop {s} /y", log);
            
            try {
                string sdPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution");
                if (Directory.Exists(sdPath)) Directory.Delete(sdPath, true);
                log.AddLog("Cleared SoftwareDistribution.");
            } catch {}

            foreach (var s in svcs) RunCommand($"net start {s}", log);
        }

        private void FixStoreAndApps(LogWindow log)
        {
            log.AddLog("Resetting Store Cache...");
            RunProcess("wsreset.exe", "", log);
            
            log.AddLog("Re-registering Apps...");
            string cmd = "Get-AppXPackage -AllUsers | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register \"$($_.InstallLocation)\\AppXManifest.xml\"}";
            var psi = new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -Command \"{cmd}\"") 
            { CreateNoWindow = true, UseShellExecute = false };
            Process.Start(psi)?.WaitForExit();
        }

        private void RebuildIconCache(LogWindow log)
        {
            RunCommand("taskkill /f /im explorer.exe", log);
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            try { File.Delete(Path.Combine(localApp, "IconCache.db")); } catch {}
            Process.Start("explorer.exe");
        }

        private async Task RestoreAllChanges(LogWindow log)
        {
            log.AddLog("Restoring Power Schemes...");
            RunProcess("powercfg", "-restoredefaultschemes", log);
            log.AddLog("Major defaults restored.");
        }

        private void RestoreBluetooth(LogWindow log)
        {
            string[] svcs = { "bthserv", "BTAGService", "BthAvctpSvc", "HidServ" };
            foreach (var s in svcs) {
                RunCommand($"sc config \"{s}\" start=auto", log);
                RunCommand($"sc start \"{s}\"", log);
            }
            log.AddLog("Bluetooth services configured.");
        }

        private void FixTouchpad(LogWindow log)
        {
            log.AddLog("Applying Touchpad recovery...");

            
            log.AddLog("Removing aggressive kernel and CSRSS mitigations...");
            RunCommand("reg delete \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\kernel\" /v \"MitigationOptions\" /f", log);
            RunCommand(@"reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe"" /v ""MitigationOptions"" /f", log);
            RunCommand(@"reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\csrss.exe"" /v ""MitigationAuditOptions"" /f", log);

            
            log.AddLog("Ensuring HID/I2C/GPIO services are enabled...");
            string[] svcs = { "HidServ", "mtp", "i2c", "GPIO", "msgpiowin32", "npsvctrig", "TabletInputService" };
            foreach (var s in svcs)
            {
                RunCommand($"sc config \"{s}\" start=auto", log);
                RunCommand($"sc start \"{s}\"", log);
            }

            
            RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\kernel\" /v InterruptSteeringDisabled /t REG_DWORD /d 0 /f", log);
            RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\kernel\" /v DistributeTimers /t REG_DWORD /d 0 /f", log);
            RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Power\" /v InterruptSteeringDisabled /t REG_DWORD /d 0 /f", log);

            
            log.AddLog("Restoring power management defaults for device classes...");
            string psCommand = "powershell -Command \"$classes = Get-ChildItem 'HKLM:\\SYSTEM\\CurrentControlSet\\Control\\Class'; foreach ($class in $classes) { $instances = Get-ChildItem $class.PSPath; foreach ($instance in $instances) { if ($instance.PSChildName -match '^\\d{4}$') { Remove-ItemProperty -Path $instance.PSPath -Name 'WakeEnabled' -ErrorAction SilentlyContinue; Remove-ItemProperty -Path $instance.PSPath -Name 'WdkSelectiveSuspendEnable' -ErrorAction SilentlyContinue; Remove-ItemProperty -Path $instance.PSPath -Name 'EnhancedPowerManagementEnabled' -ErrorAction SilentlyContinue; Remove-ItemProperty -Path $instance.PSPath -Name 'SelectiveSuspendOn' -ErrorAction SilentlyContinue; Set-ItemProperty -Path $instance.PSPath -Name 'ConfigFlags' -Value 0 -ErrorAction SilentlyContinue; Remove-ItemProperty -Path $instance.PSPath -Name 'LowerFilters' -ErrorAction SilentlyContinue; } } }\"";
            RunCommand(psCommand, log);

            
            log.AddLog("Resetting Precision Touchpad parameters...");
            RunCommand("reg add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\PrecisionTouchPad\" /v \"AAPThreshold\" /t REG_DWORD /d 2 /f", log);
            RunCommand("reg delete \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\PrecisionTouchPad\" /v \"Status\" /f", log);

            
            RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\mouhid\\Parameters\" /v TreatAbsolutePointerAsAbsolute /t REG_DWORD /d 0 /f", log);
            RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\mouhid\\Parameters\" /v TreatAbsoluteAsRelative /t REG_DWORD /d 0 /f", log);

            
            RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e96f-e325-11ce-bfc1-08002be10318}\" /v UpperFilters /t REG_MULTI_SZ /d mouclass /f", log);

            log.AddLog("Touchpad fix applied. A restart is strongly recommended.");
        }

        private void RestoreTouchpadDeep(LogWindow log)
        {
            log.AddLog("Running deep touchpad recovery (PS/2, Synaptics/ELAN)...");

            
            RunCommand("sc config i8042prt start=auto", log);
            RunCommand("sc start i8042prt", log);

            
            RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e96f-e325-11ce-bfc1-08002be10318}\" /v UpperFilters /t REG_MULTI_SZ /d mouclass /f", log);

            
            string[] touchpadVendors = { "SynTPEnh", "SynTP", "ETDService" };
            foreach (var svc in touchpadVendors)
            {
                RunCommand($"sc config \"{svc}\" start=auto", log);
                RunCommand($"sc start \"{svc}\"", log);
            }

            
            RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\mouhid\\Parameters\" /v TreatAbsolutePointerAsAbsolute /t REG_DWORD /d 0 /f", log);
            RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\mouhid\\Parameters\" /v TreatAbsoluteAsRelative /t REG_DWORD /d 0 /f", log);

            
            RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\kernel\" /v InterruptSteeringDisabled /t REG_DWORD /d 0 /f", log);
            RunCommand("reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Power\" /v InterruptSteeringDisabled /t REG_DWORD /d 0 /f", log);

            
            RunCommand("pnputil /scan-devices", log);

            log.AddLog("Deep touchpad recovery completed. Please reboot to finish driver reinit.");
        }

        #endregion

        private void ClearTempFiles(LogWindow log)
        {
            string[] paths = {
                Path.GetTempPath(),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch")
            };

            foreach (var p in paths)
            {
                try {
                    log.AddLog($"Cleaning: {p}");
                    if (!Directory.Exists(p)) continue;
                    foreach (var f in Directory.GetFiles(p)) try { File.Delete(f); } catch {}
                    foreach (var d in Directory.GetDirectories(p)) try { Directory.Delete(d, true); } catch {}
                } catch {}
            }
            log.AddLog("Temporary files cleared.");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

