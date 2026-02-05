using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Frakture_Tweaks
{
    public class EthernetTweaks
    {
        private LogWindow _logger;

        public EthernetTweaks(LogWindow logger)
        {
            _logger = logger;
        }

        public async Task ApplyGlobalInternetTweaksAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Starting Global Internet Tweaks...");

                    
                    ResetNetworkStack();
                    ResetNetworkInterfaces();

                    
                    ApplyPowerShellNetAdapterTweaks();
                    ApplyNetshTweaks();
                    ApplyTcpIpRegistryTweaks();
                    ApplyNetworkAdapterRegistryTweaks();
                    ApplyLanmanServerTweaks();
                    ApplyMsiModeTweaks();

                    _logger.AddLog("Global Internet Tweaks Applied Successfully!");
                    _logger.AddLog("Please Restart Your Computer for changes to take effect.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error during execution: {ex.Message}");
                }
            });
        }

        public async Task ApplyExtraNetworkOptimizationsAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Applying DNS & Firewall Optimizations...");

                    
                    _logger.AddLog("Setting DNS to Cloudflare (1.1.1.1)...");
                    RunCommand("netsh", "interface ipv4 set dns name=\"Wi-Fi\" static 1.1.1.1");
                    RunCommand("netsh", "interface ipv4 add dns name=\"Wi-Fi\" 1.0.0.1 index=2");
                    RunCommand("netsh", "interface ipv4 set dns name=\"Ethernet\" static 1.1.1.1");
                    RunCommand("netsh", "interface ipv4 add dns name=\"Ethernet\" 1.0.0.1 index=2");

                    
                    _logger.AddLog("Adding Firewall Rules to prevent throttling...");
                    RunCommand("netsh", "advfirewall firewall add rule name=\"StopThrottling\" dir=in action=block remoteip=173.194.55.0/24,206.111.0.0/16 enable=yes");

                    _logger.AddLog("Extra Optimizations Applied.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error applying extra tweaks: {ex.Message}");
                }
            });
        }

        public async Task ResetNetworkAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Resetting Network Settings...");
                    ResetNetworkStack();
                    _logger.AddLog("Network Settings Reset Successfully.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error: {ex.Message}");
                }
            });
        }

        public async Task DisableNaglesAlgorithmAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Disabling Nagles Algorithm...");
                    string tcpParams = @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
                    string interfacesPath = @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces";

                    SetRegistryValue(tcpParams, "TCPDelAckTicks", 1, RegistryValueKind.DWord);
                    SetRegistryValue(tcpParams, "TCPNoDelay", 1, RegistryValueKind.DWord);
                    SetRegistryValue(tcpParams, "TcpAckFrequency", 1, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SOFTWARE\Microsoft\MSMQ\Parameters", "TCPNoDelay", 1, RegistryValueKind.DWord);

                    using (RegistryKey interfacesKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces", true))
                    {
                        if (interfacesKey != null)
                        {
                            foreach (string subKeyName in interfacesKey.GetSubKeyNames())
                            {
                                using (RegistryKey subKey = interfacesKey.OpenSubKey(subKeyName, true))
                                {
                                    if (subKey != null)
                                    {
                                        subKey.SetValue("TcpAckFrequency", 1, RegistryValueKind.DWord);
                                        subKey.SetValue("TCPNoDelay", 1, RegistryValueKind.DWord);
                                        subKey.SetValue("TcpDelAckTicks", 0, RegistryValueKind.DWord);
                                    }
                                }
                            }
                        }
                    }
                    _logger.AddLog("Nagles Algorithm Disabled.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error: {ex.Message}");
                }
            });
        }

        public async Task OptimizeAdapterAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Optimizing Network Adapter...");
                    string[] cmds = new string[]
                    {
                        "int tcp set global autotuninglevel=disable",
                        "int tcp set global ecncapability=disabled",
                        "int teredo set state disabled",
                        "int isatap set state disable",
                        "int ip set global neighborcachelimit=4096",
                        "int tcp set global timestamps=disabled",
                        "int tcp set heuristics disabled",
                        "int tcp set global rss=enabled",
                        "int tcp set global rsc=disabled",
                        "int tcp set global dca=enabled",
                        "int tcp set global netdma=enabled",
                        "int tcp set global nonsackrttresiliency=disabled",
                        "int tcp set security mpp=disabled",
                        "int tcp set security profiles=disabled",
                        "int ip set global icmpredirects=disabled",
                        "int ip set global multicastforwarding=disabled",
                        "int tcp set supplemental internet congestionprovider=ctcp"
                    };

                    foreach (var args in cmds)
                    {
                        RunCommand("netsh", args);
                    }
                    _logger.AddLog("Network Adapter Optimized.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error: {ex.Message}");
                }
            });
        }

        public async Task SetDnsPriorityAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Setting DNS Priority...");
                    string priorityPath = @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\ServiceProvider";
                    SetRegistryValue(priorityPath, "LocalPriority", 4, RegistryValueKind.DWord);
                    SetRegistryValue(priorityPath, "HostsPriority", 5, RegistryValueKind.DWord);
                    SetRegistryValue(priorityPath, "DnsPriority", 6, RegistryValueKind.DWord);
                    SetRegistryValue(priorityPath, "NetbtPriority", 7, RegistryValueKind.DWord);
                    _logger.AddLog("DNS Priority Set.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error: {ex.Message}");
                }
            });
        }

        public async Task SetNetworkTaskOffloadAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Configuring Network Task Offload...");
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces", "DisableTaskOffload", 0, RegistryValueKind.DWord);
                    SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", "DisableTaskOffload", 0, RegistryValueKind.DWord);
                    RunCommand("netsh", "int ip set global taskoffload=disabled");
                    _logger.AddLog("Network Task Offload Configured.");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error: {ex.Message}");
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

        private void ResetNetworkStack()
        {
            _logger.AddLog("Resetting Network Stack...");
            
            RunCommand("ipconfig", "/flushdns");
            RunCommand("netsh", "winsock reset");
            RunCommand("netsh", "int ip reset");
            RunCommand("netsh", "interface ip delete arpcache");
        }

        private void ResetNetworkInterfaces()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    RunCommand("netsh", $"interface set interface name=\"{ni.Name}\" admin=disable");
                    System.Threading.Thread.Sleep(500); 
                    RunCommand("netsh", $"interface set interface name=\"{ni.Name}\" admin=enable");
                }
            }
        }

        private void ApplyPowerShellNetAdapterTweaks()
        {
            string[] commands = new string[]
            {
                "Disable-NetAdapterLso -Name *",
                "ForEach($adapter In Get-NetAdapter){Disable-NetAdapterPowerManagement -Name $adapter.Name -ErrorAction SilentlyContinue}",
                "ForEach($adapter In Get-NetAdapter){Disable-NetAdapterLso -Name $adapter.Name -ErrorAction SilentlyContinue}",
                "Disable-NetAdapterChecksumOffload -Name *",
                "Disable-NetAdapterRsc -Name *",
                "Disable-NetAdapterIPsecOffload -Name *",
                "Disable-NetAdapterQos -Name *"
            };

            foreach (var cmd in commands)
            {
                RunCommand("powershell", $"-Command \"{cmd}\"");
            }
        }

        private void ApplyNetshTweaks()
        {
            _logger.AddLog("Applying Netsh Global TCP/IP Settings...");
            string[] cmds = new string[]
            {
                "interface teredo set state disabled",
                "interface 6to4 set state disabled",
                "int isatap set state disable",
                "int ip set global taskoffload=enabled",
                "int ip set global neighborcachelimit=4096",
                "int tcp set global timestamps=disabled",
                "int tcp set heuristics disabled",
                "int tcp set global autotuninglevel=normal",
                "int tcp set global congestionprovider=ctcp",
                "int tcp set supplemental Internet congestionprovider=CTCP",
                "int tcp set global chimney=disabled",
                "int tcp set global ecncapability=disabled",
                "int tcp set global rss=enabled",
                "int tcp set global rsc=disabled",
                "int tcp set global dca=enabled",
                "int tcp set global netdma=enabled",
                "int tcp set global nonsackrttresiliency=disabled",
                "int tcp set security mpp=disabled",
                "int tcp set security profiles=disabled"
            };

            foreach (var args in cmds)
            {
                RunCommand("netsh", args);
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
                    default:
                        return;
                }

                
                using (RegistryKey key = baseKey.CreateSubKey(subKey, true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, valueKind);
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void ApplyTcpIpRegistryTweaks()
        {
            _logger.AddLog("Applying TCP/IP Registry Parameters...");
            string tcpParams = @"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
            
            SetRegistryValue(tcpParams, "EnableICMPRedirect", 1, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "EnablePMTUDiscovery", 1, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "Tcp1323Opts", 0, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "TcpMaxDupAcks", 2, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "TcpTimedWaitDelay", 32, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "GlobalMaxTcpWindowSize", 8760, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "TcpWindowSize", 8760, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "MaxConnectionsPerServer", 0, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "MaxUserPort", 65534, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "SackOpts", 0, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "DefaultTTL", 64, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "DelayedAckFrequency", 1, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "DelayedAckTicks", 1, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "CongestionAlgorithm", 1, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "MultihopSets", 15, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "FastCopyReceiveThreshold", 16384, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "FastSendDatagramThreshold", 16384, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "DisableTaskOffload", 0, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "TCPDelAckTicks", 1, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "TCPNoDelay", 1, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "TcpAckFrequency", 1, RegistryValueKind.DWord);
            
            
            SetRegistryValue(tcpParams, "MaxUserPort", 65534, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "TcpTimedWaitDelay", 30, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "TCPMaxDataRetransmissions", 5, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "MaxFreeTcbs", 65536, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "MaxHashTableSize", 16384, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "MaxDgramSendTasks", 64, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "EnableWsd", 0, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "DeadGWDetectDefault", 0, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "DontAddDefaultGatewayDefault", 0, RegistryValueKind.DWord);
            SetRegistryValue(tcpParams, "StrictTimeWaitSeqCheck", 1, RegistryValueKind.DWord);

            
            
            
            
            byte[] nsiValue = new byte[] {
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            };
            SetRegistryValue(@"HKLM\System\CurrentControlSet\Control\Nsi\{eb004a03-9b1a-11d4-9123-0050047759bc}\0", "0200", nsiValue, RegistryValueKind.Binary);
            SetRegistryValue(@"HKLM\System\CurrentControlSet\Control\Nsi\{eb004a03-9b1a-11d4-9123-0050047759bc}\0", "1700", nsiValue, RegistryValueKind.Binary);

            
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Winsock", "MinSockAddrLength", 16, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Winsock", "MaxSockAddrLength", 16, RegistryValueKind.DWord);

            
            SetRegistryValue(@"HKLM\SOFTWARE\Microsoft\MSMQ\Parameters", "TCPNoDelay", 1, RegistryValueKind.DWord);

            
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\ServiceProvider", "LocalPriority", 4, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\ServiceProvider", "HostsPriority", 5, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\ServiceProvider", "DnsPriority", 6, RegistryValueKind.DWord);
            SetRegistryValue(@"HKLM\SYSTEM\CurrentControlSet\Services\Tcpip\ServiceProvider", "NetbtPriority", 7, RegistryValueKind.DWord);

            
            _logger.AddLog("Optimizing Interfaces Registry Keys...");
            try 
            {
                using (RegistryKey interfacesKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces", true))
                {
                    if (interfacesKey != null)
                    {
                        foreach (string subKeyName in interfacesKey.GetSubKeyNames())
                        {
                            using (RegistryKey subKey = interfacesKey.OpenSubKey(subKeyName, true))
                            {
                                if (subKey != null)
                                {
                                    subKey.SetValue("InterfaceMetric", 55, RegistryValueKind.DWord);
                                    subKey.SetValue("TCPNoDelay", 1, RegistryValueKind.DWord);
                                    subKey.SetValue("TcpAckFrequency", 1, RegistryValueKind.DWord);
                                    subKey.SetValue("TcpDelAckTicks", 0, RegistryValueKind.DWord);
                                    subKey.SetValue("DisableTaskOffload", 0, RegistryValueKind.DWord);
                                }
                            }
                        }
                    }
                }
            }
            catch {}
        }

        private void ApplyNetworkAdapterRegistryTweaks()
        {
            _logger.AddLog("Configuring Network Adapter Advanced Settings...");
            
            string classPath = @"SYSTEM\CurrentControlSet\Control\Class\{4D36E972-E325-11CE-BFC1-08002bE10318}";
            
            try
            {
                using (RegistryKey classKey = Registry.LocalMachine.OpenSubKey(classPath, true))
                {
                    if (classKey != null)
                    {
                        foreach (string subKeyName in classKey.GetSubKeyNames())
                        {
                            
                            using (RegistryKey subKey = classKey.OpenSubKey(subKeyName, true))
                            {
                                if (subKey != null && subKey.GetValue("*SpeedDuplex") != null)
                                {
                                    
                                    subKey.SetValue("AutoPowerSaveModeEnabled", "0", RegistryValueKind.String);
                                    subKey.SetValue("AutoDisableGigabit", "0", RegistryValueKind.String);
                                    subKey.SetValue("AdvancedEEE", "0", RegistryValueKind.String);
                                    subKey.SetValue("DisableDelayedPowerUp", "2", RegistryValueKind.String);
                                    subKey.SetValue("*EEE", "0", RegistryValueKind.String);
                                    subKey.SetValue("EEE", "0", RegistryValueKind.String);
                                    subKey.SetValue("EnablePME", "0", RegistryValueKind.String);
                                    subKey.SetValue("EEELinkAdvertisement", "0", RegistryValueKind.String);
                                    subKey.SetValue("EnableGreenEthernet", "0", RegistryValueKind.String);
                                    subKey.SetValue("EnableSavePowerNow", "0", RegistryValueKind.String);
                                    subKey.SetValue("EnablePowerManagement", "0", RegistryValueKind.String);
                                    subKey.SetValue("EnableDynamicPowerGating", "0", RegistryValueKind.String);
                                    subKey.SetValue("EnableConnectedPowerGating", "0", RegistryValueKind.String);
                                    subKey.SetValue("EnableWakeOnLan", "0", RegistryValueKind.String);
                                    subKey.SetValue("GigaLite", "0", RegistryValueKind.String);
                                    subKey.SetValue("NicAutoPowerSaver", "2", RegistryValueKind.String);
                                    subKey.SetValue("PowerDownPll", "0", RegistryValueKind.String);
                                    subKey.SetValue("PowerSavingMode", "0", RegistryValueKind.String);
                                    subKey.SetValue("ReduceSpeedOnPowerDown", "0", RegistryValueKind.String);
                                    subKey.SetValue("S5NicKeepOverrideMacAddrV2", "0", RegistryValueKind.String);
                                    subKey.SetValue("S5WakeOnLan", "0", RegistryValueKind.String);
                                    subKey.SetValue("ULPMode", "0", RegistryValueKind.String);
                                    subKey.SetValue("WakeOnDisconnect", "0", RegistryValueKind.String);
                                    subKey.SetValue("*WakeOnMagicPacket", "0", RegistryValueKind.String);
                                    subKey.SetValue("*WakeOnPattern", "0", RegistryValueKind.String);
                                    subKey.SetValue("WakeOnLink", "0", RegistryValueKind.String);
                                    subKey.SetValue("WolShutdownLinkSpeed", "2", RegistryValueKind.String);
                                    subKey.SetValue("JumboPacket", "1514", RegistryValueKind.String);
                                    subKey.SetValue("TransmitBuffers", "4096", RegistryValueKind.String);
                                    subKey.SetValue("ReceiveBuffers", "512", RegistryValueKind.String);
                                    subKey.SetValue("IPChecksumOffloadIPv4", "0", RegistryValueKind.String);
                                    subKey.SetValue("LsoV1IPv4", "0", RegistryValueKind.String);
                                    subKey.SetValue("LsoV2IPv4", "0", RegistryValueKind.String);
                                    subKey.SetValue("LsoV2IPv6", "0", RegistryValueKind.String);
                                    subKey.SetValue("PMARPOffload", "0", RegistryValueKind.String);
                                    subKey.SetValue("PMNSOffload", "0", RegistryValueKind.String);
                                    subKey.SetValue("TCPChecksumOffloadIPv4", "0", RegistryValueKind.String);
                                    subKey.SetValue("TCPChecksumOffloadIPv6", "0", RegistryValueKind.String);
                                    subKey.SetValue("UDPChecksumOffloadIPv6", "0", RegistryValueKind.String);
                                    subKey.SetValue("UDPChecksumOffloadIPv4", "0", RegistryValueKind.String);
                                    
                                    
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.AddLog($"Error iterating network adapters: {ex.Message}");
            }
        }

        private void ApplyLanmanServerTweaks()
        {
            _logger.AddLog("Optimizing LanmanServer Parameters...");
            string key = @"HKLM\SYSTEM\CurrentControlSet\services\LanmanServer\Parameters";
            SetRegistryValue(key, "autodisconnect", unchecked((int)4294967295), RegistryValueKind.DWord);
            SetRegistryValue(key, "Size", 3, RegistryValueKind.DWord);
            SetRegistryValue(key, "EnableOplocks", 0, RegistryValueKind.DWord);
            SetRegistryValue(key, "IRPStackSize", 20, RegistryValueKind.DWord);
            SetRegistryValue(key, "SharingViolationDelay", 0, RegistryValueKind.DWord);
            SetRegistryValue(key, "SharingViolationRetries", 0, RegistryValueKind.DWord);
        }

        private void ApplyMsiModeTweaks()
        {
            _logger.AddLog("Applying MSI Mode and Affinity Policy...");
            
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT PNPDeviceID FROM Win32_NetworkAdapter WHERE PNPDeviceID LIKE 'PCI%'");
                foreach (ManagementObject obj in searcher.Get())
                {
                    string pnpId = obj["PNPDeviceID"]?.ToString();
                    if (!string.IsNullOrEmpty(pnpId))
                    {
                        
                        string basePath = $@"SYSTEM\ControlSet001\Enum\{pnpId}\Device Parameters\Interrupt Management";
                        
                        
                        using (RegistryKey key = Registry.LocalMachine.CreateSubKey($@"{basePath}\Affinity Policy", true))
                        {
                            if (key != null)
                            {
                                
                            }
                        }

                        
                        using (RegistryKey key = Registry.LocalMachine.CreateSubKey($@"{basePath}\MessageSignaledInterruptProperties", true))
                        {
                            if (key != null)
                            {
                                key.SetValue("MessageNumberLimit", 256, RegistryValueKind.DWord);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.AddLog($"MSI Mode tweak partial failure (this is common due to permissions): {ex.Message}");
            }
        }
    }
}

