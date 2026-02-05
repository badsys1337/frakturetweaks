using System.Windows;
using System.Windows.Input;

namespace Frakture_Tweaks
{
    public partial class StartupModeWindow : Window
    {
        public bool IsEasyMode { get; private set; } = false;

        public StartupModeWindow()
        {
            InitializeComponent();
        }

        private void EasyMode_Click(object sender, RoutedEventArgs e)
        {
            IsEasyMode = true;
            this.DialogResult = true;
            this.Close();
        }

        private void AdvancedMode_Click(object sender, RoutedEventArgs e)
        {
            IsEasyMode = false;
            this.DialogResult = true;
            this.Close();
        }
    }
}

