using System;
using System.Windows;
using System.Windows.Controls;

namespace Frakture_Tweaks
{
    
    
    
    public partial class EthernetTweaksView : UserControl
    {
        public EthernetTweaksView()
        {
            InitializeComponent();
        }

        private async void GlobalInternetTweaksBtn_Click(object sender, RoutedEventArgs e)
        {
            GlobalInternetTweaksBtn.IsEnabled = false;
            StatusText.Text = "Applying Global Internet Tweaks...";

            
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            
            EthernetTweaks tweaks = new EthernetTweaks(logWindow);
            
            try
            {
                await tweaks.ApplyGlobalInternetTweaksAsync();
                StatusText.Text = "Tweaks Applied. Please Restart.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error occurred.";
                logWindow.AddLog($"CRITICAL ERROR: {ex.Message}");
            }
            finally
            {
                GlobalInternetTweaksBtn.IsEnabled = true;
            }
        }

        private async void DnsFirewallBtn_Click(object sender, RoutedEventArgs e)
        {
            DnsFirewallBtn.IsEnabled = false;
            StatusText.Text = "Applying DNS & Firewall Tweaks...";

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            
            EthernetTweaks tweaks = new EthernetTweaks(logWindow);
            
            try
            {
                await tweaks.ApplyExtraNetworkOptimizationsAsync();
                StatusText.Text = "Extra Tweaks Applied.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error occurred.";
                logWindow.AddLog($"CRITICAL ERROR: {ex.Message}");
            }
            finally
            {
                DnsFirewallBtn.IsEnabled = true;
            }
        }

        private async void ResetNetworkBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetNetworkBtn.IsEnabled = false;
            StatusText.Text = "Resetting Network Settings...";
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            EthernetTweaks tweaks = new EthernetTweaks(logWindow);
            try
            {
                await tweaks.ResetNetworkAsync();
                StatusText.Text = "Network Settings Reset.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error occurred.";
                logWindow.AddLog($"ERROR: {ex.Message}");
            }
            finally { ResetNetworkBtn.IsEnabled = true; }
        }

        private async void NaglesAlgorithmBtn_Click(object sender, RoutedEventArgs e)
        {
            NaglesAlgorithmBtn.IsEnabled = false;
            StatusText.Text = "Disabling Nagles Algorithm...";
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            EthernetTweaks tweaks = new EthernetTweaks(logWindow);
            try
            {
                await tweaks.DisableNaglesAlgorithmAsync();
                StatusText.Text = "Nagles Algorithm Disabled.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error occurred.";
                logWindow.AddLog($"ERROR: {ex.Message}");
            }
            finally { NaglesAlgorithmBtn.IsEnabled = true; }
        }

        private async void OptimizeAdapterBtn_Click(object sender, RoutedEventArgs e)
        {
            OptimizeAdapterBtn.IsEnabled = false;
            StatusText.Text = "Optimizing Network Adapter...";
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            EthernetTweaks tweaks = new EthernetTweaks(logWindow);
            try
            {
                await tweaks.OptimizeAdapterAsync();
                StatusText.Text = "Network Adapter Optimized.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error occurred.";
                logWindow.AddLog($"ERROR: {ex.Message}");
            }
            finally { OptimizeAdapterBtn.IsEnabled = true; }
        }

        private async void DnsPriorityBtn_Click(object sender, RoutedEventArgs e)
        {
            DnsPriorityBtn.IsEnabled = false;
            StatusText.Text = "Setting DNS Priority...";
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            EthernetTweaks tweaks = new EthernetTweaks(logWindow);
            try
            {
                await tweaks.SetDnsPriorityAsync();
                StatusText.Text = "DNS Priority Set.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error occurred.";
                logWindow.AddLog($"ERROR: {ex.Message}");
            }
            finally { DnsPriorityBtn.IsEnabled = true; }
        }

        private async void NetworkTaskOffloadBtn_Click(object sender, RoutedEventArgs e)
        {
            NetworkTaskOffloadBtn.IsEnabled = false;
            StatusText.Text = "Configuring Network Task Offload...";
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            EthernetTweaks tweaks = new EthernetTweaks(logWindow);
            try
            {
                await tweaks.SetNetworkTaskOffloadAsync();
                StatusText.Text = "Network Task Offload Configured.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error occurred.";
                logWindow.AddLog($"ERROR: {ex.Message}");
            }
            finally { NetworkTaskOffloadBtn.IsEnabled = true; }
        }
    }
}

