using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Frakture_Tweaks
{
    
    
    
    public partial class DebloatView : UserControl
    {
        public DebloatView()
        {
            InitializeComponent();
        }

        private void ListAllAppsBtn_Click(object sender, RoutedEventArgs e)
        {
            
            AppListWindow appListWindow = new AppListWindow();
            appListWindow.Show();
        }

        private async void SystemCleanerBtn_Click(object sender, RoutedEventArgs e)
        {
            SystemCleanerBtn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Starting System Cleanup...");

            await Task.Run(() =>
            {
                var commands = new List<string>
                {
                    "del /s /f /q %temp%\\*.* >nul 2>&1",
                    "rd /s /q %temp% >nul 2>&1",
                    "md %temp% >nul 2>&1",
                    "del /s /f /q C:\\Windows\\Temp\\*.* >nul 2>&1",
                    "rd /s /q C:\\Windows\\Temp >nul 2>&1",
                    "md C:\\Windows\\Temp >nul 2>&1",
                    "del /s /f /q C:\\Windows\\Prefetch\\*.* >nul 2>&1",
                    "del /s /f /q C:\\Windows\\SoftwareDistribution\\Download\\*.* >nul 2>&1",
                    "ipconfig /flushdns >nul 2>&1"
                };
                RunBatchCommands(commands, logWindow);
            });

            logWindow.AddLog("System Cleanup Completed.");
            SystemCleanerBtn.IsEnabled = true;
        }

        private void CleanDiskBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("cleanmgr.exe", "/d C");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not start Disk Cleanup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DisableServicesBtn_Click(object sender, RoutedEventArgs e)
        {
            DisableServicesBtn.IsEnabled = false;
            
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Analyzing services and scheduled tasks...");

            var services = new Dictionary<string, string>
            {
                { "WSearch", "Windows Search" },
                { "SSDPSRV", "SSDP Discovery" },
                { "lfsvc", "Geolocation Service" },
                { "AXInstSV", "ActiveX Installer" },
                { "AJRouter", "AllJoyn Router Service" },
                { "AppReadiness", "App Readiness" },
                { "HomeGroupListener", "HomeGroup Listener" },
                { "HomeGroupProvider", "HomeGroup Provider" },
                { "SharedAccess", "Internet Connection Sharing" },
                { "lltdsvc", "Link-Layer Topology Discovery Mapper" },
                { "diagnosticshub.standardcollector.service", "Microsoft(R) Diagnostics Hub Standard Collector Service" },
                { "wlidsvc", "Microsoft Account Sign-in Assistant" },
                { "SmsRouter", "Microsoft Windows SMS Router Service" },
                { "NcdAutoSetup", "Network Connected Devices Auto-Setup" },
                { "PNRPsvc", "Peer Name Resolution Protocol" },
                { "p2psvc", "Peer Networking Group" },
                { "p2pimsvc", "Peer Networking Identity Manager" },
                { "PNRPAutoReg", "PNRP Machine Name Publication Service" },
                { "WalletService", "WalletService" },
                { "WMPNetworkSvc", "Windows Media Player Network Sharing Service" },
                { "icssvc", "Windows Mobile Hotspot" },
                { "XblAuthManager", "Xbox Live Auth Manager" },
                { "XblGameSave", "Xbox Live Game Save" },
                { "XboxNetApiSvc", "Xbox Live Networking Service" },
                { "DmEnrollmentSvc", "Device Management Enrollment Service" },
                { "RetailDemo", "Retail Demo Service" },
                { "DiagTrack", "Connected User Experiences and Telemetry" },
                { "dmwappushservice", "WAP Push Message Routing Service" },
                { "WerSvc", "Windows Error Reporting Service" },
                { "MapsBroker", "Downloaded Maps Manager" },
                { "Spooler", "Print Spooler" },
                { "SysMain", "SysMain" },
                { "GpuEnergyDrv", "GPU Energy Driver" },
                { "GpuEnergyDr", "GPU Energy Driver (Kernel)" },
                { "bam", "Background Activity Moderator" },
                { "dam", "Desktop Activity Moderator" },
                { "BTAGService", "Bluetooth Audio Gateway Service" },
                { "bthserv", "Bluetooth Support Service" },
                { "BthAvctpSvc", "Bluetooth AVCTP Service" },
                { "BluetoothUserService", "Bluetooth User Support Service" },
                { "XboxGipSvc", "Xbox Accessory Management Service" }
            };

            var scheduledTasks = new List<string>
            {
                "Microsoft\\Windows\\Application Experience\\ProgramDataUpdater",
                "Microsoft\\Windows\\Autochk\\Proxy",
                "Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator",
                "Microsoft\\Windows\\Customer Experience Improvement Program\\UsbCeip",
                "Microsoft\\Windows\\Defrag\\ScheduledDefrag",
                "Microsoft\\Windows\\Device Information\\Device",
                "Microsoft\\Windows\\Device Information\\Device User",
                "Microsoft\\Windows\\DiskDiagnostic\\Microsoft-Windows-DiskDiagnosticDataCollector",
                "Microsoft\\Windows\\DiskFootprint\\Diagnostics",
                "Microsoft\\Windows\\DiskFootprint\\StorageSense",
                "Microsoft\\Windows\\DUSM\\dusmtask",
                "Microsoft\\Windows\\EnterpriseMgmt\\MDMMaintenenceTask",
                "Microsoft\\Windows\\Feedback\\Siuf\\DmClient",
                "Microsoft\\Windows\\Input\\TouchpadSyncDataAvailable",
                "Microsoft\\Windows\\International\\Synchronize Language Settings",
                "Microsoft\\Windows\\LanguageComponentsInstaller\\Installation",
                "Microsoft\\Windows\\LanguageComponentsInstaller\\ReconcileLanguageResources",
                "Microsoft\\Windows\\LanguageComponentsInstaller\\Uninstallation",
                "Microsoft\\Windows\\License Manager\\TempSignedLicenseExchange",
                "Microsoft\\Windows\\Maintenance\\WinSAT",
                "Microsoft\\Windows\\Maps\\MapsToastTask",
                "Microsoft\\Windows\\Maps\\MapsUpdateTask",
                "Microsoft\\Windows\\Mobile Broadband Accounts\\MNO Metadata Parser",
                "Microsoft\\Windows\\MUI\\LPRemove",
                "Microsoft\\Windows\\NetTrace\\GatherNetworkInfo",
                "Microsoft\\Windows\\PI\\Sqm-Tasks",
                "Microsoft\\Windows\\Power Efficiency Diagnostics\\AnalyzeSystem",
                "Microsoft\\Windows\\PushToInstall\\Registration",
                "Microsoft\\Windows\\Ras\\MobilityManager",
                "Microsoft\\Windows\\Feedback\\Siuf\\DmClientOnScenarioDownload",
                "Microsoft\\Windows\\Flighting\\FeatureConfig\\ReconcileFeatures",
                "Microsoft\\Windows\\Flighting\\FeatureConfig\\UsageDataFlushing",
                "Microsoft\\Windows\\Flighting\\FeatureConfig\\UsageDataReporting",
                "Microsoft\\Windows\\Flighting\\OneSettings\\RefreshCache",
                "Microsoft\\Windows\\Input\\LocalUserSyncDataAvailable",
                "Microsoft\\Windows\\Setup\\SetupCleanupTask",
                "Microsoft\\Windows\\Setup\\SnapshotCleanupTask",
                "Microsoft\\Windows\\SpacePort\\SpaceAgentTask",
                "Microsoft\\Windows\\SpacePort\\SpaceManagerTask",
                "Microsoft\\Windows\\Speech\\SpeechModelDownloadTask"
            };

            await Task.Run(() =>
            {
                
                var currentBatch = new List<string>();
                foreach (var service in services)
                {
                    currentBatch.Add($"sc config {service.Key} start=disabled >nul 2>&1");
                    currentBatch.Add($"sc stop {service.Key} >nul 2>&1");
                }
                RunBatchCommands(currentBatch, logWindow);
                logWindow.AddLog($"Disabled {services.Count} unnecessary services.");

                
                currentBatch.Clear();
                foreach (var task in scheduledTasks)
                {
                    currentBatch.Add($"schtasks /Change /TN \"{task}\" /Disable >nul 2>&1");
                }
                RunBatchCommands(currentBatch, logWindow);
                logWindow.AddLog($"Disabled {scheduledTasks.Count} unnecessary scheduled tasks.");
            });

            logWindow.AddLog("All specified services and tasks have been processed.");
            DisableServicesBtn.IsEnabled = true;
        }


        private async void RamUsageDebloaterBtn_Click(object sender, RoutedEventArgs e)
        {
            RamUsageDebloaterBtn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Starting RAM Usage Debloat...");

            await Task.Run(() =>
            {
                
                Dispatcher.Invoke(() => logWindow.AddLog("Step 1/6: Disabling MMAgent features (Compression, Prefetching)..."));
                var mmAgentCommands = new List<string>
                {
                    "powershell -command \"Disable-MMAgent -MemoryCompression\" >nul 2>&1",
                    "powershell -command \"Disable-MMAgent -PageCombining\" >nul 2>&1",
                    "powershell -command \"Disable-MMAgent -ApplicationLaunchPrefetching\" >nul 2>&1",
                    "powershell -command \"Disable-MMAgent -OperationRecorder\" >nul 2>&1"
                };
                RunBatchCommands(mmAgentCommands, logWindow);

                
                Dispatcher.Invoke(() => logWindow.AddLog("Step 2/6: Optimizing Memory Management registry settings..."));
                var memMgmtCommands = new List<string>
                {
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\" /v \"DisablePagingExecutive\" /t REG_DWORD /d 1 /f >nul 2>&1",
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\" /v \"LargeSystemCache\" /t REG_DWORD /d 0 /f >nul 2>&1",
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\" /v \"ClearPageFileAtShutdown\" /t REG_DWORD /d 0 /f >nul 2>&1",
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\" /v \"SystemPages\" /t REG_DWORD /d 4294967295 /f >nul 2>&1",
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\" /v \"PoolUsageMaximum\" /t REG_DWORD /d 96 /f >nul 2>&1"
                };
                RunBatchCommands(memMgmtCommands, logWindow);

                
                Dispatcher.Invoke(() => logWindow.AddLog("Step 3/6: Disabling Prefetcher and Superfetch parameters..."));
                var fetchCommands = new List<string>
                {
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\\PrefetchParameters\" /v \"EnablePrefetcher\" /t REG_DWORD /d 0 /f >nul 2>&1",
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\\PrefetchParameters\" /v \"EnableSuperfetch\" /t REG_DWORD /d 0 /f >nul 2>&1"
                };
                RunBatchCommands(fetchCommands, logWindow);

                
                Dispatcher.Invoke(() => logWindow.AddLog("Step 4/6: Disabling SysMain (Superfetch) service..."));
                var serviceCommands = new List<string>
                {
                    "sc config SysMain start=disabled >nul 2>&1",
                    "sc stop SysMain >nul 2>&1"
                };
                RunBatchCommands(serviceCommands, logWindow);

                
                Dispatcher.Invoke(() => logWindow.AddLog("Step 5/6: Cleanup..."));
                

                
                Dispatcher.Invoke(() => logWindow.AddLog("Step 6/6: Disabling global background apps RAM usage..."));
                var backgroundCommands = new List<string>
                {
                    "reg add \"HKLM\\Software\\Policies\\Microsoft\\Windows\\AppPrivacy\" /v \"LetAppsRunInBackground\" /t REG_DWORD /d 2 /f >nul 2>&1"
                };
                RunBatchCommands(backgroundCommands, logWindow);
            });

            logWindow.AddLog("RAM Usage Debloat Completed. Please restart for changes to take effect.");
            RamUsageDebloaterBtn.IsEnabled = true;
        }

        private async void RemoveEdgeBtn_Click(object sender, RoutedEventArgs e)
        {
            RemoveEdgeBtn.IsEnabled = false;
            
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            
            EdgeRemoval edgeRemoval = new EdgeRemoval(logWindow);
            
            try
            {
                await edgeRemoval.PerformEdgeRemoval();
                logWindow.AddLog("Operation Finished. Please restart your computer.");
            }
            catch (Exception ex)
            {
                logWindow.AddLog($"CRITICAL ERROR: {ex.Message}");
            }
            finally
            {
                RemoveEdgeBtn.IsEnabled = true;
            }
        }

        private async void DisableUpdatesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will fully disable Windows Updates using brute-force methods (Services, Registry, Tasks, and System Files).\n\nContinue?", "Disable Windows Updates", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                return;

            DisableUpdatesBtn.IsEnabled = false;
            
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Initiating Brute-Force Windows Update disable process...");

            await Task.Run(() =>
            {
                string psexecPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Res", "PsExec.exe");
                if (!File.Exists(psexecPath))
                {
                    
                    psexecPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PsExec.exe"); 
                    if (!File.Exists(psexecPath)) psexecPath = "PsExec.exe";
                }

                var commands = new List<string>
                {
                    
                    "for %%i in (wuauserv, UsoSvc, uhssvc, WaaSMedicSvc, bits, dosvc, cryptSvc, msiserver) do (",
                    "    net stop %%i /y >nul 2>&1",
                    "    sc config %%i start= disabled >nul 2>&1",
                    "    sc failure %%i reset= 0 actions= \"\" >nul 2>&1",
                    ")",

                    
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\wuauserv\" /v Start /t REG_DWORD /d 4 /f >nul 2>&1",
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\UsoSvc\" /v Start /t REG_DWORD /d 4 /f >nul 2>&1",
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\WaaSMedicSvc\" /v Start /t REG_DWORD /d 4 /f >nul 2>&1",

                    
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\WaaSMedicSvc\" /v Start /t REG_DWORD /d 4 /f >nul 2>&1",
                    "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Services\\WaaSMedicSvc\" /v FailureActions /t REG_BINARY /d 000000000000000000000000030000001400000000000000c0d4010000000000e09304000000000000000000 /f >nul 2>&1",
                    "reg add \"HKLM\\Software\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU\" /v NoAutoUpdate /t REG_DWORD /d 1 /f >nul 2>&1",
                    "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU\" /v AUOptions /t REG_DWORD /d 2 /f >nul 2>&1",
                    "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\" /v DisableWindowsUpdateAccess /t REG_DWORD /d 1 /f >nul 2>&1",

                    
                    "del /f /s /q c:\\windows\\softwaredistribution\\*.* >nul 2>&1",
                    "rmdir /s /q c:\\windows\\softwaredistribution >nul 2>&1",

                    
                    "powershell -command \"Get-ScheduledTask -TaskPath '\\Microsoft\\Windows\\InstallService\\*' | Disable-ScheduledTask; Get-ScheduledTask -TaskPath '\\Microsoft\\Windows\\UpdateOrchestrator\\*' | Disable-ScheduledTask; Get-ScheduledTask -TaskPath '\\Microsoft\\Windows\\UpdateAssistant\\*' | Disable-ScheduledTask; Get-ScheduledTask -TaskPath '\\Microsoft\\Windows\\WaaSMedic\\*' | Disable-ScheduledTask; Get-ScheduledTask -TaskPath '\\Microsoft\\Windows\\WindowsUpdate\\*' | Disable-ScheduledTask; Get-ScheduledTask -TaskPath '\\Microsoft\\WindowsUpdate\\*' | Disable-ScheduledTask\" >nul 2>&1"
                };

                
                string tempBatch = Path.Combine(Path.GetTempPath(), $"disable_updates_{Guid.NewGuid()}.bat");
                try 
                {
                    File.WriteAllLines(tempBatch, commands);
                    
                    
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = psexecPath,
                        Arguments = $"/accepteula -s cmd.exe /c \"{tempBatch}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (Process? process = Process.Start(psi))
                    {
                        if (process != null)
                        {
                            process.WaitForExit();
                        }
                    }
                    logWindow.AddLog("Brute-force update disabling completed via PsExec (System).");
                }
                catch (Exception ex)
                {
                    logWindow.AddLog($"Error during brute-force: {ex.Message}");
                    
                    RunBatchCommands(commands, logWindow);
                }
                finally
                {
                    if (File.Exists(tempBatch)) File.Delete(tempBatch);
                }
            });

            logWindow.AddLog("Windows Updates have been fully disabled.");
            DisableUpdatesBtn.IsEnabled = true;
        }

        private void RunBatchCommands(List<string> commands, LogWindow? logWindow = null)
        {
            if (commands == null || commands.Count == 0) return;

            
            string tempPath = Path.Combine(Path.GetTempPath(), $"debloat_{Guid.NewGuid()}.bat");
            
            try
            {
                
                var batchContent = new List<string> { "@echo off" };
                batchContent.AddRange(commands);
                File.WriteAllLines(tempPath, batchContent);

                var processInfo = new ProcessStartInfo("cmd.exe", $"/c \"{tempPath}\"")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = new Process { StartInfo = processInfo })
                {
                    process.Start();

                    
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    process.WaitForExit();
                    
                    
                    Task.WaitAll(outputTask, errorTask);
                    
                    string output = outputTask.Result;
                    string error = errorTask.Result;

                    
                    if (!string.IsNullOrWhiteSpace(error) && logWindow != null)
                    {
                        Dispatcher.Invoke(() => logWindow.AddLog($"Errors encountered:\n{error}"));
                    }
                }
            }
            catch (Exception ex)
            {
                if (logWindow != null)
                {
                    Dispatcher.Invoke(() => logWindow.AddLog($"Error running batch: {ex.Message}"));
                }
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }
    }
}
