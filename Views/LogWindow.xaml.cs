using System;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace Frakture_Tweaks
{
    public enum LogWindowMode
    {
        Normal,
        Large
    }

    public partial class LogWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;
        
        public LogWindow() : this(LogWindowMode.Normal)
        {
        }

        public LogWindow(LogWindowMode mode)
        {
            InitializeComponent();
            ApplyMode(mode);
            Loaded += LogWindow_Loaded;
            this.Opacity = 0;
        }

        private void ApplyMode(LogWindowMode mode)
        {
            if (mode == LogWindowMode.Large)
            {
                Width = 1100;
                Height = 760;
            }
        }

        private void LogWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var handle = new WindowInteropHelper(this).Handle;
            int useImmersiveDarkMode = 1;
            
            
            if (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int)) != 0)
            {
                
                DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useImmersiveDarkMode, sizeof(int));
            }
            
            
            int color = 0x001F1F1F; 
            DwmSetWindowAttribute(handle, DWMWA_CAPTION_COLOR, ref color, sizeof(int));
            
            
            var anim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25));
            anim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            this.BeginAnimation(OpacityProperty, anim);
        }
        


        public void AddLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText(message + Environment.NewLine);
                LogTextBox.ScrollToEnd();
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

