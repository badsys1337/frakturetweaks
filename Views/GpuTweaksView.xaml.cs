using System;
using System.Windows;
using System.Windows.Controls;

namespace Frakture_Tweaks
{
    
    
    
    public partial class GpuTweaksView : UserControl
    {
        public GpuTweaksView()
        {
            InitializeComponent();
        }

        private async void ApplyGpuTweaksBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GpuTypeComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a GPU manufacturer first.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ApplyGpuTweaksBtn.IsEnabled = false;
            string selected = (GpuTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            
            GpuTweaks tweaks = new GpuTweaks(logWindow);
            
            try
            {
                if (selected != null && selected.Contains("Nvidia"))
                {
                    StatusText.Text = "Applying Nvidia GPU Tweaks...";
                    await tweaks.ApplyNvidiaTweaksAsync();
                    StatusText.Text = "Nvidia Tweaks Applied. Please Restart.";
                }
                else if (selected != null && selected.Contains("AMD"))
                {
                    StatusText.Text = "Applying AMD GPU Tweaks...";
                    await tweaks.ApplyAmdTweaksAsync();
                    StatusText.Text = "AMD Tweaks Applied. Please Restart.";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error occurred.";
                logWindow.AddLog($"CRITICAL ERROR: {ex.Message}");
            }
            finally
            {
                ApplyGpuTweaksBtn.IsEnabled = true;
            }
        }
    }
}

