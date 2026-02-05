using System;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Frakture_Tweaks
{
    
    
    
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                Frakture_Tweaks.Services.LocalizationManager.Instance.Initialize();

                LicenseService service = new LicenseService();
                string? savedKey = service.GetSavedKey();

                bool autoLoginSuccess = false;

                if (!string.IsNullOrEmpty(savedKey))
                {
                    
                    var result = await service.RedeemKey(savedKey);
                    if (result.success)
                    {
                        autoLoginSuccess = true;

                        MainWindow main = new MainWindow(false); 
                        this.MainWindow = main;
                        main.Show();
                    }
                }

                if (!autoLoginSuccess)
                {
                    KeyWindow keyWin = new KeyWindow();
                    this.MainWindow = keyWin;
                    keyWin.Show();
                }
            }
            catch (Exception ex)
            {
                
                MessageBox.Show($"Startup Error: {ex.Message}", "Frakture Tweaks Startup", MessageBoxButton.OK, MessageBoxImage.Warning);
                KeyWindow keyWin = new KeyWindow();
                this.MainWindow = keyWin;
                keyWin.Show();
            }
        }
    }
}
