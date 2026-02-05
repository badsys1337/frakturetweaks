using System;
using System.Windows;
using System.Windows.Controls;

namespace Frakture_Tweaks
{
    
    
    
    public partial class CpuTweaksView : UserControl
    {
        public CpuTweaksView()
        {
            InitializeComponent();
        }

        private async void GlobalCpuTweaksBtn_Click(object sender, RoutedEventArgs e)
        {
            GlobalCpuTweaksBtn.IsEnabled = false;
            StatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_ApplyingGlobalCpu");

            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            
            CpuTweaks tweaks = new CpuTweaks(logWindow);
            
            try
            {
                await tweaks.ApplyGlobalCpuTweaksAsync();
                StatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_GlobalCpuApplied");
            }
            catch (Exception ex)
            {
                StatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_Error");
                logWindow.AddLog(string.Format(Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_CriticalError"), ex.Message));
            }
            finally
            {
                GlobalCpuTweaksBtn.IsEnabled = true;
            }
        }

        private async void ApplySpecificCpuTweaksBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CpuTypeComboBox.SelectedIndex == -1)
            {
                MessageBox.Show(
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Msg_CpuSelectionRequired"), 
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Msg_SelectionRequired"), 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }

            ApplySpecificCpuTweaksBtn.IsEnabled = false;
            string selected = (CpuTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            LogWindow logWindow = new LogWindow();
            logWindow.Show();
            
            CpuTweaks tweaks = new CpuTweaks(logWindow);
            
            try
            {
                if (selected != null && selected.Contains("Intel"))
                {
                    StatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_ApplyingIntelCpu");
                    await tweaks.ApplyIntelCpuTweaksAsync();
                    StatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_IntelCpuApplied");
                }
                else if (selected != null && selected.Contains("AMD"))
                {
                    StatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_ApplyingAmdCpu");
                    await tweaks.ApplyAmdCpuTweaksAsync();
                    StatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_AmdCpuApplied");
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_Error");
                logWindow.AddLog(string.Format(Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_CriticalError"), ex.Message));
            }
            finally
            {
                ApplySpecificCpuTweaksBtn.IsEnabled = true;
            }
        }
    }
}

