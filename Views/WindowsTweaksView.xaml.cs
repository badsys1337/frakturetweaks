using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Frakture_Tweaks
{
    
    
    
    public partial class WindowsTweaksView : UserControl
    {
        public WindowsTweaksView()
        {
            InitializeComponent();
        }

        private async void DisableAnimationsBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Applying animation tweaks...");

            try
            {
                int changesCount = await Task.Run(() =>
                {
                    int count = 0;
                    string visualEffectsPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects";
                    
                    
                    using (var key = Registry.CurrentUser.CreateSubKey(visualEffectsPath, true))
                    {
                        if (key != null)
                        {
                            key.SetValue("VisualFXSetting", 3, RegistryValueKind.DWord);
                            count++;
                        }
                    }

                    
                    var valuesToSet = new Dictionary<string, int>
                    {
                        { "AnimateMinMax", 1 },
                        { "ComboBoxAnimation", 1 },
                        { "ControlAnimations", 1 },
                        { "CursorShadow", 0 },
                        { "DragFullWindows", 1 },
                        { "DropShadow", 0 },
                        { "ListBoxSmoothScrolling", 1 },
                        { "ListviewAlphaSelect", 1 },
                        { "ListviewShadow", 1 },
                        { "MenuAnimation", 0 },
                        { "SelectionFade", 1 },
                        { "Themes", 1 },
                        { "ThumbnailsOrIcon", 1 },
                        { "TooltipAnimation", 1 }
                    };

                    BatchSetRegistryValues(visualEffectsPath, valuesToSet, "DefaultApplied");
                    count += valuesToSet.Count;

                    return count;
                });

                logWindow.AddLog($"Successfully disabled Windows animations.\nApplied {changesCount} registry changes to optimize visual performance.");
            }
            catch (Exception ex)
            {
                logWindow.AddLog($"Error: {ex.Message}");
                MessageBox.Show($"Error applying tweaks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }

        private async void DirectXTweaksBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Applying DirectX tweaks...");

            try
            {
                int changesCount = await Task.Run(() =>
                {
                    int count = 0;
                    string ddPath = @"SOFTWARE\Microsoft\DirectDraw";
                    string ddWowPath = @"SOFTWARE\Wow6432Node\Microsoft\DirectDraw";
                    string d3dPath = @"SOFTWARE\Microsoft\Direct3D";
                    string d3dWowPath = @"SOFTWARE\Wow6432Node\Microsoft\Direct3D";
                    string d3dDriversPath = @"SOFTWARE\Microsoft\Direct3D\Drivers";
                    string d3dDriversWowPath = @"SOFTWARE\Wow6432Node\Microsoft\Direct3D\Drivers";

                    
                    var ddValues = new Dictionary<string, int>
                    {
                        { "DisableAGPSupport", 0 },
                        { "UseNonLocalVidMem", 1 },
                        { "DisableDDSCAPSInDDSD", 0 },
                        { "EmulationOnly", 0 },
                        { "EmulatePointSprites", 0 },
                        { "EmulateStateBlocks", 0 },
                        { "DisableMMX", 0 },
                        { "ForceNoSysLock", 0 }
                    };
                    BatchSetRegistryValuesLM(ddPath, ddValues);
                    BatchSetRegistryValuesLM(ddWowPath, ddValues);
                    count += ddValues.Count * 2;

                    
                    var d3dValues = new Dictionary<string, int>
                    {
                        { "UseNonLocalVidMem", 1 },
                        { "EnableDebugging", 0 },
                        { "FullDebug", 0 },
                        { "DisableDM", 1 },
                        { "EnableMultimonDebugging", 0 },
                        { "LoadDebugRuntime", 0 },
                        { "FewVertices", 1 },
                        { "DisableMMX", 0 },
                        { "MMX Fast Path", 1 },
                        { "MMXFastPath", 1 },
                        { "UseMMXForRGB", 1 }
                    };
                    BatchSetRegistryValuesLM(d3dPath, d3dValues);
                    BatchSetRegistryValuesLM(d3dWowPath, d3dValues); 
                    count += d3dValues.Count * 2;

                    
                    var d3dDriversValues = new Dictionary<string, int>
                    {
                        { "ForceRgbRasterizer", 0 },
                        { "EnumReference", 1 },
                        { "EnumSeparateMMX", 1 },
                        { "EnumRamp", 1 },
                        { "EnumNullDevice", 1 },
                        { "UseMMXForRGB", 1 }
                    };
                    BatchSetRegistryValuesLM(d3dDriversPath, d3dDriversValues);
                    BatchSetRegistryValuesLM(d3dDriversWowPath, d3dDriversValues);
                    count += d3dDriversValues.Count * 2;

                    return count;
                });

                logWindow.AddLog($"Successfully applied DirectX tweaks.\nApplied {changesCount} registry changes to optimize DirectX performance.");
            }
            catch (Exception ex)
            {
                logWindow.AddLog($"Error: {ex.Message}");
                MessageBox.Show($"Error applying tweaks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }

        private async void GlobalTweaksBtn_Click(object sender, RoutedEventArgs e)
        {
            GlobalTweaksBtn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Applying Global Tweaks (250+)...");
            
            GlobalTweaks tweaks = new GlobalTweaks(logWindow);
            
            try
            {
                await tweaks.ApplyGlobalTweaksAsync();
                logWindow.AddLog("Global Tweaks Applied. Please Restart.");
            }
            catch (Exception ex)
            {
                logWindow.AddLog($"CRITICAL ERROR: {ex.Message}");
            }
            finally
            {
                GlobalTweaksBtn.IsEnabled = true;
            }
        }

        private async void DisableOneDriveBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Disabling OneDrive and applying mitigations...");

            try
            {
                await Task.Run(() =>
                {
                    
                    RunCommand("taskkill", "/IM \"OneDrive.exe\" /F");

                    
                    string oneDriveSetup = Environment.ExpandEnvironmentVariables(@"%windir%\SysWOW64\OneDriveSetup.exe");
                    if (File.Exists(oneDriveSetup))
                    {
                        RunCommand(oneDriveSetup, "/uninstall");
                    }

                    
                    string[] dirs = new string[]
                    {
                        @"%UserProfile%\OneDrive",
                        @"%ProgramData%\Microsoft OneDrive",
                        @"%LocalAppData%\Microsoft\OneDrive",
                        @"C:\OneDriveTemp"
                    };

                    foreach (var dir in dirs)
                    {
                        string expandedDir = Environment.ExpandEnvironmentVariables(dir);
                        if (Directory.Exists(expandedDir))
                        {
                            try { Directory.Delete(expandedDir, true); } catch { }
                        }
                    }

                    
                    DeleteRegistryKeyClassesRoot(@"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");
                    DeleteRegistryKeyClassesRoot(@"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}");

                    
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Biometrics\Credential Provider", "Enabled", 0);

                    
                    var edgePolicies = new Dictionary<string, int>
                    {
                        { "EfficiencyModeEnabled", 0 },
                        { "SleepingTabsEnabled", 0 },
                        { "PerformanceDetectorEnabled", 0 },
                        { "ShowMicrosoftRewards", 0 },
                        { "BrowserSignin", 0 },
                        { "StartupBoostEnabled", 0 },
                        { "HubsSidebarEnabled", 0 },
                        { "StandaloneHubsSidebarEnabled", 0 },
                        { "NewTabPagePrerenderEnabled", 0 },
                        { "WindowOcclusionEnabled", 0 },
                        { "IntensiveWakeUpThrottlingEnabled", 0 },
                        { "MicrosoftEdgeInsiderPromotionEnabled", 0 },
                        { "BackgroundModeEnabled", 0 },
                        { "LocalBrowserDataShareEnabled", 0 },
                        { "WebToBrowserSignInEnabled", 0 },
                        { "AADWebSiteSSOUsingThisProfileEnabled", 0 },
                        { "InternetExplorerIntegrationReloadInIEModeAllowed", 0 },
                        { "RedirectSitesFromInternetExplorerRedirectMode", 0 },
                        { "ComposeInlineEnabled", 0 },
                        { "SpellcheckEnabled", 0 },
                        { "ConfigureFriendlyURLFormat", 1 },
                        { "FamilySafetySettingsEnabled", 0 },
                        { "QuickSearchShowMiniMenu", 0 },
                        { "HideFirstRunExperience", 1 },
                        { "TrackingPrevention", 0 },
                        { "PaymentMethodQueryEnabled", 0 },
                        { "TyposquattingCheckerEnabled", 0 },
                        { "SpotlightExperiencesAndRecommendationsEnabled", 0 },
                        { "EdgeShoppingAssistantEnabled", 0 },
                        { "MouseGestureEnabled", 0 },
                        { "SplitScreenEnabled", 0 },
                        { "TextPredictionEnabled", 0 },
                        { "TranslateEnabled", 0 },
                        { "DefaultShareAdditionalOSRegionSetting", 2 },
                        { "QuickViewOfficeFilesEnabled", 0 },
                        { "AutoImportAtFirstRun", 4 }
                    };
                    BatchSetRegistryValuesLM(@"SOFTWARE\Policies\Microsoft\Edge", edgePolicies);
                    
                    
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\EdgeUpdate", "RemoveDesktopShortcutDefault", 2);
                });

                logWindow.AddLog("Successfully disabled OneDrive and applied mitigations.");
            }
            catch (Exception ex)
            {
                logWindow.AddLog($"Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }

        private void HagsGameModeBtn_Click(object sender, RoutedEventArgs e)
        {
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Enabling HAGS & Game Mode...");

            try
            {
                
                SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2);
                
                SetRegistryValue(@"Software\Microsoft\GameBar", "AllowAutoGameMode", 1);
                SetRegistryValue(@"Software\Microsoft\GameBar", "AutoGameModeEnabled", 1);
                
                logWindow.AddLog("HAGS and Game Mode enabled. Restart required for HAGS.");
            }
            catch (Exception ex) { logWindow.AddLog($"Error: {ex.Message}"); }
        }

        private void GamePriorityBtn_Click(object sender, RoutedEventArgs e)
        {
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Boosting priorities for common games...");

            try
            {
                string[] games = { 
                    "FortniteClient-Win64-Shipping.exe", 
                    "VALORANT-Win64-Shipping.exe", 
                    "cs2.exe", 
                    "r5apex.exe", 
                    "Overwatch.exe", 
                    "Minecraft.exe", 
                    "javaw.exe",
                    "League of Legends.exe",
                    "GTA5.exe"
                };

                foreach (var game in games)
                {
                    SetGamePriority(game);
                }

                logWindow.AddLog("High CPU/IO Priority set for major games.");
            }
            catch (Exception ex) { logWindow.AddLog($"Error: {ex.Message}"); }
        }

        private async void CpuOptimizationBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Applying CPU Performance Optimizations...");

            try
            {
                await Task.Run(() =>
                {
                    
                    var powerSettings = new Dictionary<string, int>
                    {
                        { "HighPerformance", 1 },
                        { "HighestPerformance", 1 },
                        { "MinimumThrottlePercent", 0 },
                        { "MaximumThrottlePercent", 0 },
                        { "MaximumPerformancePercent", 100 },
                        { "InitialUnparkCount", 100 },
                        { "Class1InitialUnparkCount", 100 }
                    };
                    BatchSetRegistryValuesLM(@"SYSTEM\CurrentControlSet\Control\Power", powerSettings);

                    
                    var processorSettings = new Dictionary<string, int>
                    {
                        { "AllowPepPerfStates", 0 }
                    };
                    BatchSetRegistryValuesLM(@"SYSTEM\CurrentControlSet\Control\Processor", processorSettings);

                    
                    var cs001ProcessorSettings = new Dictionary<string, object>
                    {
                        { "ProccesorThrottlingEnabled", 0 },
                        { "CpuIdleThreshold", 1 },
                        { "CpuIdle", 0 },
                        { "CpuLatencyTimer", 0 },
                        { "CpuSlowdown", 0 },
                        { "Threshold", 1 },
                        { "CpuDebuggingEnabled", 0 },
                        { "ProccesorLatencyThrottlingEnabled", 0 },
                        { "CpuIdleScrubDelay", 0 },
                        { "CpuIdleScrubInterval", 0 },
                        { "CpuIdleScrubPower", 18 },
                        { "CpuIdleScrubThreshold", 0 },
                        { "CpuIdleScrubType", 2 },
                        { "CpuIdleScrubValue", 100 },
                        { "CpuIdleScrubValueMaximum", 100 },
                        { "CpuIdleScrubValueMinimum", 100 }
                    };
                    BatchSetRegistryValuesLM(@"SYSTEM\ControlSet001\Control\Processor", cs001ProcessorSettings);

                    
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor", "CPMinCores", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor", "CPMaxCores", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor", "PerfEnergyPreference", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Power\Policy\Settings\Processor", "MaxPerformance", 100);

                    
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1);
                });

                logWindow.AddLog("CPU performance optimizations applied successfully.");
            }
            catch (Exception ex)
            {
                logWindow.AddLog($"Error: {ex.Message}");
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }

        private async void InputLatencyBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Applying Input Latency Reductions...");

            try
            {
                await Task.Run(() =>
                {
                    
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", -1);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0);
                });

                logWindow.AddLog("Input latency reductions applied successfully. Please restart your computer.");
            }
            catch (Exception ex)
            {
                logWindow.AddLog($"Error: {ex.Message}");
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }

        private void SetGamePriority(string exeName)
        {
            string keyPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\{exeName}\PerfOptions";
            using (var key = Registry.LocalMachine.CreateSubKey(keyPath, true))
            {
                if (key != null)
                {
                    key.SetValue("CpuPriorityClass", 3, RegistryValueKind.DWord); 
                    key.SetValue("IoPriority", 3, RegistryValueKind.DWord);        
                }
            }
        }

        private void RunCommand(string command, string args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(command, args);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                Process.Start(psi)?.WaitForExit();
            }
            catch { }
        }

        private void DeleteRegistryKeyClassesRoot(string keyPath)
        {
            try
            {
                Registry.ClassesRoot.DeleteSubKeyTree(keyPath, false);
            }
            catch { }
        }

        private void SetRegistryValue(string keyPath, string valueName, int value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath, true))
            {
                if (key != null)
                {
                    key.SetValue(valueName, value, RegistryValueKind.DWord);
                }
            }
        }

        private async void AdvancedSystemTweaksBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Applying Advanced System Tweaks...");

            try
            {
                await Task.Run(() =>
                {
                    
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1);

                    
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", -1);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0);

                    
                    var gameTasks = new Dictionary<string, object>
                    {
                        { "GPU Priority", 8 },
                        { "Priority", 6 },
                        { "Scheduling Category", "High" },
                        { "SFIO Priority", "High" }
                    };
                    BatchSetRegistryValuesLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", gameTasks);

                    
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", "MaintenanceDisabled", 1);

                    
                    SetRegistryValueCU(@"Control Panel\Desktop", "MenuShowDelay", "0");
                    
                    
                    SetRegistryValueCU(@"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "");

                    
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1);
                });

                logWindow.AddLog("Advanced system tweaks applied successfully.");
            }
            catch (Exception ex)
            {
                logWindow.AddLog($"Error: {ex.Message}");
            }
            finally
            {
                if (btn != null) btn.IsEnabled = true;
            }
        }

        private async void DisableTelemetryBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Disabling Telemetry and Error Reporting...");

            try
            {
                await Task.Run(() =>
                {
                    
                    var errorRep = new Dictionary<string, object>
                    {
                        { "Disabled", 1 },
                        { "DoReport", 0 },
                        { "LoggingDisabled", 1 }
                    };
                    BatchSetRegistryValuesLM(@"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", errorRep);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\PCHealth\ErrorReporting", "DoReport", 0);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\Windows Error Reporting", "Disabled", 1);

                    
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\FTH", "Enabled", 0);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0);
                    SetRegistryValueLM(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0);
                    
                    
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy", "HasAccepted", 0);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1);
                });

                logWindow.AddLog("Telemetry and Error Reporting disabled successfully.");
            }
            catch (Exception ex) { logWindow.AddLog($"Error: {ex.Message}"); }
            finally { if (btn != null) btn.IsEnabled = true; }
        }

        private async void ExplorerTweaksBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Optimizing File Explorer...");

            try
            {
                await Task.Run(() =>
                {
                    var explorerSettings = new Dictionary<string, object>
                    {
                        { "TaskbarAnimations", 0 },
                        { "IconsOnly", 0 },
                        { "ListviewShadow", 0 },
                        { "ShowSyncNotifications", 0 },
                        { "SnapAssist", 0 }
                    };
                    BatchSetRegistryValuesCU(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", explorerSettings);
                    
                    SetRegistryValueCU(@"Software\Microsoft\Windows\DWM", "EnableAeroPeek", 0);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\DWM", "AlwaysHibernateThumbnails", 0);
                    SetRegistryValueCU(@"Control Panel\Desktop\WindowMetrics", "MinAnimate", "0");
                });

                logWindow.AddLog("File Explorer optimized successfully.");
            }
            catch (Exception ex) { logWindow.AddLog($"Error: {ex.Message}"); }
            finally { if (btn != null) btn.IsEnabled = true; }
        }

        private async void SystemOptimizationsBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Applying System Performance Plus tweaks...");

            try
            {
                await Task.Run(() =>
                {
                    
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DriverSearching", "SearchOrderConfig", 0);
                    
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Device Metadata", "PreventDeviceMetadataFromNetwork", 1);
                    
                    
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DownloadMode", 0);
                    
                    
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", 0);
                    
                    
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1);
                });

                logWindow.AddLog("System Performance Plus tweaks applied successfully.");
            }
            catch (Exception ex) { logWindow.AddLog($"Error: {ex.Message}"); }
            finally { if (btn != null) btn.IsEnabled = true; }
        }

        private async void DisableStartupDelayBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Disabling Windows Startup Delay...");

            try
            {
                await Task.Run(() =>
                {
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", 0);
                });

                logWindow.AddLog("Startup delay disabled successfully.");
            }
            catch (Exception ex) { logWindow.AddLog($"Error: {ex.Message}"); }
            finally { if (btn != null) btn.IsEnabled = true; }
        }

        private async void OptimizeResponsivenessBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null) btn.IsEnabled = false;

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog("Optimizing System Responsiveness...");

            try
            {
                await Task.Run(() =>
                {
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 10);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", -1);
                });

                logWindow.AddLog("System responsiveness optimized (Standardized to 10 for Gaming).");
            }
            catch (Exception ex) { logWindow.AddLog($"Error: {ex.Message}"); }
            finally { if (btn != null) btn.IsEnabled = true; }
        }

        private void BatchSetRegistryValuesCU(string keyPath, Dictionary<string, object> values)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath, true))
            {
                if (key != null)
                {
                    foreach (var kvp in values)
                    {
                        if (kvp.Value is int)
                            key.SetValue(kvp.Key, kvp.Value, RegistryValueKind.DWord);
                        else if (kvp.Value is string)
                            key.SetValue(kvp.Key, kvp.Value, RegistryValueKind.String);
                        else
                            key.SetValue(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        private void SetRegistryValueLM(string keyPath, string valueName, object value)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(keyPath, true))
            {
                if (key != null)
                {
                    if (value is int)
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                    else if (value is string)
                        key.SetValue(valueName, value, RegistryValueKind.String);
                    else
                        key.SetValue(valueName, value);
                }
            }
        }

        private void SetRegistryValueCU(string keyPath, string valueName, object value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(keyPath, true))
            {
                if (key != null)
                {
                    if (value is int)
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                    else if (value is string)
                        key.SetValue(valueName, value, RegistryValueKind.String);
                    else
                        key.SetValue(valueName, value);
                }
            }
        }

        private void BatchSetRegistryValues(string baseKeyPath, Dictionary<string, int> values, string subValueName = "DefaultApplied")
        {
            foreach (var kvp in values)
            {
                using (var key = Registry.CurrentUser.CreateSubKey(Path.Combine(baseKeyPath, kvp.Key), true))
                {
                    if (key != null)
                    {
                        key.SetValue(subValueName, kvp.Value, RegistryValueKind.DWord);
                    }
                }
            }
        }

        private void BatchSetRegistryValuesLM(string keyPath, Dictionary<string, object> values)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(keyPath, true))
            {
                if (key != null)
                {
                    foreach (var kvp in values)
                    {
                        if (kvp.Value is int)
                            key.SetValue(kvp.Key, kvp.Value, RegistryValueKind.DWord);
                        else if (kvp.Value is string)
                            key.SetValue(kvp.Key, kvp.Value, RegistryValueKind.String);
                        else
                            key.SetValue(kvp.Key, kvp.Value);
                    }
                }
            }
        }

        private void BatchSetRegistryValuesLM(string keyPath, Dictionary<string, int> values)
        {
            using (var key = Registry.LocalMachine.CreateSubKey(keyPath, true))
            {
                if (key != null)
                {
                    foreach (var kvp in values)
                    {
                        key.SetValue(kvp.Key, kvp.Value, RegistryValueKind.DWord);
                    }
                }
            }
        }
    }
}

