using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Frakture_Tweaks
{
    public partial class KeyWindow : Window
    {
        private LicenseService _licenseService;

        public KeyWindow()
        {
            InitializeComponent();
            SecurityGuard.StartProtection(); 
            _licenseService = new LicenseService();
        }

        private async void RedeemBtn_Click(object sender, RoutedEventArgs e)
        {
            string key = KeyBox.Text.Trim();
            if (string.IsNullOrEmpty(key))
            {
                StatusText.Text = "Please enter a valid key.";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                return;
            }

            RedeemBtn.IsEnabled = false;
            RedeemBtn.Content = "VERIFYING...";
            StatusText.Text = "";

            var result = await _licenseService.RedeemKey(key);

            if (result.success)
            {
                StatusText.Text = "Starting Frakture Tweaks...";
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                
                if (RememberCheck.IsChecked == true)
                {
                    _licenseService.SaveKey(key);
                }
                
                
                this.Hide();
                await System.Threading.Tasks.Task.Delay(100); 

                MainWindow main = new MainWindow(false); 
                Application.Current.MainWindow = main;
                main.Show();
                this.Close();
            }
            else
            {
                
                StatusText.Text = result.message; 
                StatusText.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                RedeemBtn.IsEnabled = true;
                RedeemBtn.Content = "ACTIVATE";
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}

