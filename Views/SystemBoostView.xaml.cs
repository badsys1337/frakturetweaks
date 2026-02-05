using System.Windows;
using System.Windows.Controls;

namespace Frakture_Tweaks
{
    public partial class SystemBoostView : UserControl
    {
        private SystemBoostService _service;
        private LogWindow _logger;

        public SystemBoostView()
        {
            InitializeComponent();
            _logger = new LogWindow();
            _logger.Hide();
            _service = new SystemBoostService(_logger);
        }

        private async void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            BtnApply.IsEnabled = false;
            _logger.Show();
            await _service.ApplySystemBoostAsync();
            MessageBox.Show("System Boost Applied!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            BtnApply.IsEnabled = true;
        }

        private async void BtnRevert_Click(object sender, RoutedEventArgs e)
        {
            BtnRevert.IsEnabled = false;
            _logger.Show();
            await _service.RevertSystemBoostAsync();
            MessageBox.Show("Changes Reverted.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            BtnRevert.IsEnabled = true;
        }
    }
}

