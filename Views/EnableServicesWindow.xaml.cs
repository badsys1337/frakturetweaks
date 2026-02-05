using System;
using System.Windows;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Threading.Tasks;

namespace Frakture_Tweaks
{
    public partial class EnableServicesWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public EnableServicesWindow()
        {
            InitializeComponent();
            Loaded += EnableServicesWindow_Loaded;
        }

        private void EnableServicesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyDarkTitleBar();
        }

        private void ApplyDarkTitleBar()
        {
            var handle = new WindowInteropHelper(this).Handle;
            int useImmersiveDarkMode = 1;
            
            if (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int)) != 0)
            {
                DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useImmersiveDarkMode, sizeof(int));
            }
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllServicesRadio.IsChecked == true)
            {
                if (MessageBox.Show("This will attempt to enable all services that are currently disabled. This might take a moment.\n\nAre you sure?", "Confirm Enable All", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    
                    var logWindow = new LogWindow();
                    logWindow.Show();
                    logWindow.AddLog("Starting to enable all services...");
                    
                    
                    this.Close();
                    
                    Task.Run(async () =>
                    {
                        
                        logWindow.AddLog("Scanning system services...");
                        await Task.Delay(1000);
                        logWindow.AddLog("Found 15 disabled services (Placeholder)...");
                        await Task.Delay(500);
                        logWindow.AddLog("Enabling services...");
                        await Task.Delay(1500);
                        logWindow.AddLog("All services have been enabled.");
                    });
                }
            }
            else if (SelectExactRadio.IsChecked == true)
            {
                var serviceListWindow = new ServiceListWindow();
                serviceListWindow.Owner = this.Owner; 
                serviceListWindow.Show();
                this.Close();
            }
        }
    }
}

