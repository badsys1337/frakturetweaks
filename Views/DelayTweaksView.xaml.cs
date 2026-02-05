using System;
using System.Windows;
using System.Windows.Controls;

namespace Frakture_Tweaks
{
    
    
    
    public partial class DelayTweaksView : UserControl
    {
        public DelayTweaksView()
        {
            InitializeComponent();
        }

        private async void DemolishDelayBtn_Click(object sender, RoutedEventArgs e)
        {
            DemolishDelayBtn.IsEnabled = false;
            StatusText.Text = "Demolishing Delay...";

            
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            
            DelayTweaks tweaks = new DelayTweaks(logWindow);
            
            try
            {
                await tweaks.DemolishDelayAsync();
                StatusText.Text = "Delay Demolished. Please Restart.";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error occurred.";
                logWindow.AddLog($"CRITICAL ERROR: {ex.Message}");
            }
            finally
            {
                DemolishDelayBtn.IsEnabled = true;
            }
        }

        private async void BcdLatencyBtn_Click(object sender, RoutedEventArgs e)
        {
            BcdLatencyBtn.IsEnabled = false;
            StatusText.Text = "Applying BCD Tweaks...";
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            DelayTweaks tweaks = new DelayTweaks(logWindow);
            await tweaks.ApplyBcdLatencyTweaksAsync();
            StatusText.Text = "BCD Tweaks Applied.";
            BcdLatencyBtn.IsEnabled = true;
        }

        private async void GpuMonitorLatencyBtn_Click(object sender, RoutedEventArgs e)
        {
            GpuMonitorLatencyBtn.IsEnabled = false;
            StatusText.Text = "Optimizing GPU/Monitor...";
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            DelayTweaks tweaks = new DelayTweaks(logWindow);
            await tweaks.ApplyGpuMonitorLatencyTweaksAsync();
            StatusText.Text = "GPU/Monitor Optimized.";
            GpuMonitorLatencyBtn.IsEnabled = true;
        }

        private async void SystemMitigationsBtn_Click(object sender, RoutedEventArgs e)
        {
            SystemMitigationsBtn.IsEnabled = false;
            StatusText.Text = "Disabling Mitigations...";
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            DelayTweaks tweaks = new DelayTweaks(logWindow);
            await tweaks.DisableMitigationsAsync();
            StatusText.Text = "Mitigations Disabled.";
            SystemMitigationsBtn.IsEnabled = true;
        }

        private async void EnableTrimBtn_Click(object sender, RoutedEventArgs e)
        {
            EnableTrimBtn.IsEnabled = false;
            StatusText.Text = "Enabling TRIM...";
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            DelayTweaks tweaks = new DelayTweaks(logWindow);
            await tweaks.EnableTrimOptimizationAsync();
            StatusText.Text = "TRIM Enabled.";
            EnableTrimBtn.IsEnabled = true;
        }

        private async void MouseHidBtn_Click(object sender, RoutedEventArgs e)
        {
            MouseHidBtn.IsEnabled = false;
            StatusText.Text = "Optimizing Mouse/HID...";
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            DelayTweaks tweaks = new DelayTweaks(logWindow);
            await tweaks.OptimizeMouseHidAsync();
            StatusText.Text = "Mouse/HID Optimized.";
            MouseHidBtn.IsEnabled = true;
        }
    }
}

