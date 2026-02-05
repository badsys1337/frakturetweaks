using System.Windows;
using System.Windows.Controls;

namespace Frakture_Tweaks
{
    public partial class InputBoostView : UserControl
    {
        private InputBoostService _service;
        private LogWindow _logger;

        public InputBoostView()
        {
            InitializeComponent();
            _logger = new LogWindow();
            _logger.Show(); 
            _logger.Hide(); 
            _service = new InputBoostService(_logger);
        }

        private async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            BtnApply.IsEnabled = false;
            _logger.Show();
            await _service.ApplyInputBoostAsync();
            MessageBox.Show("Input Boost Applied!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            BtnApply.IsEnabled = true;
        }

        private async void BtnRevert_Click(object sender, RoutedEventArgs e)
        {
            BtnRevert.IsEnabled = false;
            _logger.Show();
            await _service.RevertInputBoostAsync();
            MessageBox.Show("Input Boost Reverted!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            BtnRevert.IsEnabled = true;
        }
    }
}

