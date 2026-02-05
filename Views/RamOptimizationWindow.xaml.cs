using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Frakture_Tweaks
{
    public partial class RamOptimizationWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;

        public RamOptimizationWindow()
        {
            InitializeComponent();
            Loaded += RamOptimizationWindow_Loaded;
            this.Opacity = 0;
        }

        private void RamOptimizationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            int useImmersiveDarkMode = 1;
            DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));
            
            int color = 0x001F1F1F;
            DwmSetWindowAttribute(handle, DWMWA_CAPTION_COLOR, ref color, sizeof(int));

            
            var anim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25));
            anim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            this.BeginAnimation(OpacityProperty, anim);
        }



        private async void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int ramSize = 16;
                bool selectionFound = false;

                
                foreach (var child in RamSelectionPanel.Children)
                {
                    if (child is RadioButton rb && rb.IsChecked == true && rb.Tag != null)
                    {
                        if (int.TryParse(rb.Tag.ToString(), out int parsedSize))
                        {
                            ramSize = parsedSize;
                            selectionFound = true;
                            break;
                        }
                    }
                }

                if (!selectionFound)
                {
                    
                }

                ulong thresholdValue = 0;
                switch (ramSize)
                {
                    case 4: thresholdValue = 4194304; break;
                    case 8: thresholdValue = 8388608; break;
                    case 16: thresholdValue = 16777216; break;
                    case 32: thresholdValue = 33554432; break;
                    case 64: thresholdValue = 67108864; break;
                    default: thresholdValue = 16777216; break;
                }

                
                LogWindow logWindow = new LogWindow();
                logWindow.Show();

                
                
                this.Visibility = Visibility.Hidden;

                logWindow.AddLog($"Selected RAM Size: {ramSize} GB");
                logWindow.AddLog($"Applying SvcHost Split Threshold: {thresholdValue} KB");

                try
                {
                    using (var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(@"SYSTEM\ControlSet001\Control"))
                    {
                        if (key != null)
                        {
                            key.SetValue("SvcHostSplitThresholdInKB", thresholdValue, Microsoft.Win32.RegistryValueKind.DWord);
                            logWindow.AddLog("Success: Registry value updated.");
                        }
                        else
                        {
                            logWindow.AddLog("Error: Failed to open registry key.");
                        }
                    }
                }
                catch (Exception ex)
                {
                     logWindow.AddLog($"Error executing optimization: {ex.Message}");
                }
                
                logWindow.AddLog("Optimization complete. Please restart your computer.");
                
                
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Critical Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }
    }
}

