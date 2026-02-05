using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Frakture_Tweaks
{
    public class GlobalTweaks
    {
        private LogWindow _logger;

        public GlobalTweaks(LogWindow logger)
        {
            _logger = logger;
        }

        public async Task ApplyGlobalTweaksAsync()
        {
            await Task.Run(async () =>
            {
                try
                {
                    _logger.AddLog("Starting Global Tweaks (250+)...");
                    List<string> batchCommands = new List<string>();

                    
                    _logger.AddLog("Applying Registry Tweaks (HKLM)...");
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\FTH", "Enabled", 0);
                    DeleteRegistryKeyLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree\MicrosoftEdgeUpdateTaskMachineCore");
                    DeleteRegistryKeyLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\TaskCache\Tree\MicrosoftEdgeUpdateTaskMachineUA");
                    SetRegistryValueLM(@"SOFTWARE\Policies\Google\Chrome", "StartupBoostEnabled", 0);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Google\Chrome", "BackgroundModeEnabled", 0);
                    SetRegistryStringLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "ctfmon", @"C:\Windows\System32\ctfmon.exe");
                    SetRegistryValueLM(@"Software\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0);
                    SetRegistryValueLM(@"Software\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowDeviceNameInTelemetry", 0);
                    SetRegistryValueLM(@"Software\Policies\Microsoft\Windows\safer\codeidentifiers", "authenticodeenabled", 0);
                    SetRegistryValueLM(@"Software\Policies\Microsoft\Windows\Windows Error Reporting", "DontSendAdditionalData", 1);
                    SetRegistryValueLM(@"Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Policies\DataCollection", "AllowTelemetry", 0);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0);
                    SetRegistryValueLM(@"Software\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DownloadMode", 0);
                    SetRegistryValueLM(@"Software\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\NetworkProvider", "RestoreConnection", 0);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "HideSCAMeetNow", 1);
                    SetRegistryValueLM(@"Software\Policies\Microsoft\Windows\CloudContent", "DisableSoftLanding", 1);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\FileSystem", "LongPathsEnabled", 1);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0);
                    
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "DisableExceptionChainValidation", 1);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "EnableCfg", 1);
                    SetRegistryValueLM(@"System\CurrentControlSet\Control\Session Manager", "ProtectionMode", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Services\DXGKrnl", "MonitorLatencyTolerance", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Services\DXGKrnl", "MonitorRefreshLatencyTolerance", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "ThreadDpcEnable", 1);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Session Manager\kernel", "DistributeTimers", 1);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Power", "LowLatencyScalingPercentage", 100);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FlyoutMenuSettings", "ShowSleepOption", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\HardCap0", "CapPercentage", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\HardCap0", "SchedulingType", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\Paused", "CapPercentage", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\Paused", "SchedulingType", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\SoftCapFull", "CapPercentage", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\SoftCapFull", "SchedulingType", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\SoftCapLow", "CapPercentage", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\CPU\SoftCapLow", "SchedulingType", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\BackgroundDefault", "IsLowPriority", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\Frozen", "IsLowPriority", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\FrozenDNCS", "IsLowPriority", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\FrozenDNK", "IsLowPriority", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\FrozenPPLE", "IsLowPriority", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\Paused", "IsLowPriority", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\PausedDNK", "IsLowPriority", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\Pausing", "IsLowPriority", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\PrelaunchForeground", "IsLowPriority", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Flags\ThrottleGPUInterference", "IsLowPriority", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Importance\Critical", "BasePriority", 82);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Importance\Critical", "OverTargetPriority", 50);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Importance\CriticalNoUi", "BasePriority", 82);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Importance\StartHost", "BasePriority", 82);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Importance\StartHost", "OverTargetPriority", 50);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Importance\VeryHigh", "BasePriority", 82);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Importance\VeryHigh", "OverTargetPriority", 50);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Importance\VeryLow", "BasePriority", 82);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Importance\VeryLow", "OverTargetPriority", 50);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\IO\NoCap", "IOBandwidth", 0);
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Memory\NoCap", "CommitLimit", -1); 
                    SetRegistryValueLM(@"SYSTEM\ResourcePolicyStore\ResourceSets\Policies\Memory\NoCap", "CommitTarget", -1);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\PolicyManager\current\device\System", "AllowExperimentation", 0);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\PolicyManager\default\System\AllowExperimentation", "value", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\AppModel", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\Cellcore", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\Circular Kernel Context Logger", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\CloudExperienceHostOobe", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DataMarket", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderApiLogger", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DefenderAuditLogger", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\DiagLog", "Start", 0);

                    
                    _logger.AddLog("Applying Registry Tweaks (HKCU)...");
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Windows\DWM", "UseDpiScaling", 0);
                    SetRegistryValueCU(@"Software\Microsoft\Multimedia\Audio", "UserDuckingPreference", 3);
                    SetRegistryStringCU(@"Control Panel\Mouse", "MouseSpeed", "0");
                    SetRegistryStringCU(@"Control Panel\Mouse", "MouseThreshold1", "0");
                    SetRegistryStringCU(@"Control Panel\Mouse", "MouseThreshold2", "0");
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\VideoSettings", "VideoQualityOnBattery", 1);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "IconsOnly", 0);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewShadow", 0);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Speech_OneCore\Settings\OnlineSpeechPrivacy", "HasAccepted", 0);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Personalization\Settings", "AcceptedPrivacyPolicy", 0);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitInkCollection", 1);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\InputPersonalization", "RestrictImplicitTextCollection", 1);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\InputPersonalization\TrainedDataStore", "HarvestContacts", 0);
                    SetRegistryStringCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location", "Value", "Deny");
                    SetRegistryStringCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\appDiagnostics", "Value", "Deny");
                    SetRegistryStringCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\userAccountInformation", "Value", "Deny");
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", 0);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications", "GlobalUserDisabled", 1);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Search", "BackgroundAppGlobalToggle", 0);
                    
                    SetRegistryBinaryCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "Discord", "0300000066AF9C7C5A46D901");
                    SetRegistryBinaryCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "Synapse3", "030000007DC437B0EA9FD901");
                    SetRegistryBinaryCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "Spotify", "0300000070E93D7B5A46D901");
                    SetRegistryBinaryCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "EpicGamesLauncher", "03000000F51C70A77A48D901");
                    SetRegistryBinaryCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "RiotClient", "03000000A0EA598A88B2D901");
                    SetRegistryBinaryCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run", "Steam", "03000000E7766B83316FD901");
                    
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 0);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ListviewAlphaSelect", 0);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "DisallowShaking", 1);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\People", "PeopleBand", 0);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", 2);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 0);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0);
                    SetRegistryValueCU(@"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Storage\microsoft.microsoftedge_8wekyb3d8bbwe\MicrosoftEdge\Main", "Theme", 1);
                    SetRegistryStringCU(@"Control Panel\Keyboard", "KeyboardSpeed", "31");
                    SetRegistryStringCU(@"Control Panel\Keyboard", "InitialKeyboardIndicators", "2");

                    
                    _logger.AddLog("Applying Registry Tweaks (HKCR)...");
                    SetRegistryValueCR(@"CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}\ShellFolder", "Attributes", 0);
                    SetRegistryValueCR(@"Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}\ShellFolder", "Attributes", 0);

                    
                    _logger.AddLog("Applying Registry Tweaks (Users Default)...");
                    
                    SetRegistryStringUsersDefault(@"Control Panel\Keyboard", "InitialKeyboardIndicators", "2");
                    SetRegistryStringUsersDefault(@"Control Panel\Keyboard", "KeyboardDelay", "0");
                    SetRegistryStringUsersDefault(@"Control Panel\Keyboard", "KeyboardSpeed", "31");

                    
                    _logger.AddLog("Applying Service Tweaks...");
                    RunCommand("sc", "config SysMain start= disabled");

                    
                    _logger.AddLog("Applying Power Config Tweaks...");
                    RunCommand("powercfg", "-setacvalueindex scheme_current SUB_DISK dbc9e238-6de9-49e3-92cd-8c2b4946b472 1");
                    RunCommand("powercfg", "-setacvalueindex scheme_current SUB_DISK fc95af4d-40e7-4b6d-835a-56d131dbc80e 1");

                    
                    _logger.AddLog("Applying Other Tweaks...");
                    RunCommand("lodctr", "/r");
                    RunCommand("FSUTIL", "behavior set disablelastaccess 1");

                    
                    _logger.AddLog("Applying Additional WMI Autologgers...");
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\HolographicDevice", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\iclsClient", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\iclsProxy", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\LwtNetLog", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\Mellanox-Kernel", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\Microsoft-Windows-AssignedAccess-Trace", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\Microsoft-Windows-Setup", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\NBSMBLOGGER", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\PEAuthLog", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\RdrLog", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\ReadyBoot", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\SetupPlatform", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\SetupPlatformTel", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\SocketHeciServer", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\SpoolerLogger", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\SQMLogger", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\TCPIPLOGGER", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\TileStore", "Start", 0);
                    
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\UBPM", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\WdiContextLog", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\WFP-IPsec Trace", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\WiFiDriverIHVSession", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\WiFiDriverIHVSessionRepro", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\WiFiSession", "Start", 0);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\WMI\Autologger\WinPhoneCritical", "Start", 0);

                    
                    _logger.AddLog("Applying Additional HKLM Tweaks...");
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\WUDF", "LogEnable", 0);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\WUDF", "LogLevel", 0);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableThirdPartySuggestions", 1);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1);
                    SetRegistryValueLM(@"SYSTEM\CurrentControlSet\Control\Lsa\Credssp", "DebugLogLevel", 0);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Schedule\Maintenance", "MaintenanceDisabled", 1);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NoLazyMode", 1);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "AlwaysOn", 1);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 10);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", -1);
                    
                    
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowSearchToUseLocation", 0);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchPrivacy", 3);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchSafeSearch", 3);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchUseWeb", 0);
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "ConnectedSearchUseWebOverMeteredConnections", 0);
                    SetRegistryValueLM(@"Software\Microsoft\PolicyManager\default\Experience\AllowCortana", "value", 0);
                    SetRegistryValueLM(@"Software\Policies\Microsoft\SearchCompanion", "DisableContentFileUpdates", 1);
                    SetRegistryValueLM(@"Software\Policies\Microsoft\Windows\Windows Search", "AllowCloudSearch", 0);
                    SetRegistryValueLM(@"Software\Policies\Microsoft\Windows\Windows Search", "AllowCortanaAboveLock", 0);
                    SetRegistryValueLM(@"Software\Policies\Microsoft\Windows\Windows Search", "DisableWebSearch", 1);
                    SetRegistryValueLM(@"Software\Policies\Microsoft\Windows\Windows Search", "DoNotUseWebResults", 1);

                    
                    string gpuKey = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000";
                    SetRegistryValueLM(gpuKey, "LOWLATENCY", 1);

                    
                    SetRegistryValueLM(@"System\CurrentControlSet\Services\LanManServer\Parameters", "DisableCompression", 1);
                    SetRegistryValueLM(@"Software\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\lsass.exe", "AuditLevel", 8);
                    SetRegistryValueLM(@"Software\Policies\Microsoft\Windows\CredentialsDelegation", "AllowProtectedCreds", 1);
                    SetRegistryValueLM(@"System\CurrentControlSet\Control\Lsa", "DisableRestrictedAdminOutboundCreds", 1);
                    SetRegistryValueLM(@"System\CurrentControlSet\Control\Lsa", "DisableRestrictedAdmin", 0);
                    SetRegistryValueLM(@"System\CurrentControlSet\Control\Lsa", "RunAsPPL", 1);
                    SetRegistryValueLM(@"System\CurrentControlSet\Control\SecurityProviders\WDigest", "Negotiate", 0);
                    SetRegistryValueLM(@"System\CurrentControlSet\Control\SecurityProviders\WDigest", "UseLogonCredential", 0);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Serialize", "StartupDelayInMSec", 0);
                    
                    
                    SetRegistryStringLM(@"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", "0");
                    SetRegistryStringLM(@"SOFTWARE\Microsoft\PolicyManager\default\ApplicationManagement\AllowGameDVR", "value", "00000000");

                    
                    _logger.AddLog("Applying Additional HKCU Tweaks...");
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0);
                    SetRegistryStringCU(@"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaCapabilities", "");
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Search", "IsAssignedAccess", 0);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\Search", "IsWindowsHelloActive", 0);
                    
                    
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Games", "FpsAll", 1);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Games", "FpsStatusGames", 10);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Games", "FpsStatusGamesAll", 4);
                    SetRegistryValueCU(@"SOFTWARE\Microsoft\Games", "GameFluidity", 1);

                    
                    SetRegistryValueCU(@"System\GameConfigStore", "GameDVR_Enabled", 0);
                    SetRegistryValueCU(@"System\GameConfigStore", "GameDVR_FSEBehaviorMode", 0);
                    SetRegistryValueCU(@"System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 0);
                    SetRegistryValueCU(@"System\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 0);
                    SetRegistryValueCU(@"System\GameConfigStore", "GameDVR_EFSEFeatureFlags", 0);
                    SetRegistryValueCU(@"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", 0);
                    SetRegistryValueCU(@"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0);

                    
                    _logger.AddLog("Applying FSUTIL Commands...");
                    batchCommands.Add("fsutil behavior set memoryusage 2");
                    batchCommands.Add("fsutil behavior set mftzone 4");
                    batchCommands.Add("fsutil behavior set disabledeletenotify 0");
                    batchCommands.Add("fsutil behavior set encryptpagingfile 0");

                    
                    _logger.AddLog("Applying PowerShell SMB Tweaks...");
                    batchCommands.Add("powershell -Command \"Disable-WindowsOptionalFeature -Online -FeatureName SMB1Protocol\"");
                    batchCommands.Add("powershell -Command \"Disable-WindowsOptionalFeature -Online -FeatureName SMB1Protocol-Client\"");
                    batchCommands.Add("powershell -Command \"Disable-WindowsOptionalFeature -Online -FeatureName SMB1Protocol-Server\"");
                    batchCommands.Add("powershell -Command \"Set-SmbClientConfiguration -RequireSecuritySignature $True -Force\"");
                    batchCommands.Add("powershell -Command \"Set-SmbClientConfiguration -EnableSecuritySignature $True -Force\"");
                    batchCommands.Add("powershell -Command \"Set-SmbServerConfiguration -EncryptData $True -Force\"");
                    batchCommands.Add("powershell -Command \"Set-SmbServerConfiguration -EnableSMB1Protocol $false -Force\"");

                    ExecuteBatch(batchCommands);

                    _logger.AddLog("Global Tweaks (250+) Applied Successfully!");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error applying global tweaks: {ex.Message}");
                }
            });
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
                        
                        var outputTask = p.StandardOutput.ReadToEndAsync();
                        var errorTask = p.StandardError.ReadToEndAsync();

                        p.WaitForExit();
                        
                        Task.WaitAll(outputTask, errorTask);
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
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        private void SetRegistryStringLM(string keyPath, string valueName, string value)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.String);
                    }
                }
            }
            catch { }
        }

        private void SetRegistryBinaryLM(string keyPath, string valueName, string hexString)
        {
            try
            {
                byte[] data = StringToByteArray(hexString);
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, data, RegistryValueKind.Binary);
                    }
                }
            }
            catch { }
        }

        private void DeleteRegistryKeyLM(string keyPath)
        {
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(keyPath, false);
            }
            catch { }
        }

        private void SetRegistryValueCU(string keyPath, string valueName, int value)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        private void SetRegistryStringCU(string keyPath, string valueName, string value)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.String);
                    }
                }
            }
            catch { }
        }

        private void SetRegistryBinaryCU(string keyPath, string valueName, string hexString)
        {
            try
            {
                byte[] data = StringToByteArray(hexString);
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, data, RegistryValueKind.Binary);
                    }
                }
            }
            catch { }
        }

        private void SetRegistryValueCR(string keyPath, string valueName, int value)
        {
            try
            {
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(keyPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        private void SetRegistryStringUsersDefault(string keyPath, string valueName, string value)
        {
            try
            {
                
                using (RegistryKey users = Registry.Users)
                {
                    using (RegistryKey key = users.OpenSubKey(".DEFAULT", true)?.CreateSubKey(keyPath, true))
                    {
                        if (key != null)
                        {
                            key.SetValue(valueName, value, RegistryValueKind.String);
                        }
                    }
                }
            }
            catch { }
        }

        private byte[] StringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("Hex string must have an even length");
                
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        public async Task RestoreMicrosoftStoreAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Starting Advanced Microsoft Store Restoration...");

                    
                    _logger.AddLog("Unblocking Store in registry...");
                    SetRegistryValueLM(@"SOFTWARE\Policies\Microsoft\WindowsStore", "RemoveWindowsStore", 0);
                    SetRegistryValueLM(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoWindowsStore", 0);

                    
                    _logger.AddLog("Ensuring all dependent services are enabled and running...");
                    string[] services = { "ClipSVC", "AppXSVC", "WSService", "LicenseManager", "CryptSvc", "DoSvc", "UsoSvc", "bits" };
                    foreach (var svc in services)
                    {
                        RunCommand("sc", $"config {svc} start= auto");
                        RunCommand("sc", $"start {svc}");
                    }

                    
                    _logger.AddLog("Triggering Store install via wsreset -i...");
                    RunCommand("wsreset.exe", "-i");
                    Task.Delay(3000).Wait(); 

                    
                    _logger.AddLog("Re-registering Microsoft Store packages via PowerShell...");
                    
                    
                    string ps1 = "Get-AppxPackage -AllUsers Microsoft.WindowsStore | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register \"$($_.InstallLocation)\\AppXManifest.xml\" -ErrorAction SilentlyContinue}";
                    RunCommand("powershell", $"-ExecutionPolicy Bypass -Command \"{ps1}\"");

                    
                    string ps2 = "Get-AppxPackage -allusers *WindowsStore* | Foreach {Add-AppxPackage -DisableDevelopmentMode -Register \"$($_.InstallLocation)\\AppXManifest.xml\" -ErrorAction SilentlyContinue}";
                    RunCommand("powershell", $"-ExecutionPolicy Bypass -Command \"{ps2}\"");

                    _logger.AddLog("Restoration sequence finished. If the Store icon doesn't appear in 5 minutes, a system restart is recommended.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error during restoration: {ex.Message}");
                }
            });
        }
    }
}

