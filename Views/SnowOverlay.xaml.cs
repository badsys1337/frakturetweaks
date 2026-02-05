using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Frakture_Tweaks
{
    public partial class SnowOverlay : UserControl
    {
        private class SnowFlake
        {
            public Ellipse? UIElement { get; set; }
            public TranslateTransform? Transform { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double SpeedY { get; set; }
            public double SpeedX { get; set; }
            public double Size { get; set; }
            public double Phase { get; set; }
        }

        private List<SnowFlake> _flakes = new List<SnowFlake>();
        private Random _random = new Random();
        private int _flakeCount = 80;
        private bool _isInitialized = false;
        private double _time = 0;

        public SnowOverlay()
        {
            InitializeComponent();
            Loaded += SnowOverlay_Loaded;
            Unloaded += SnowOverlay_Unloaded;
            SizeChanged += SnowOverlay_SizeChanged;
        }

        private void SnowOverlay_Loaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void SnowOverlay_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
        }

        private void SnowOverlay_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_isInitialized && ActualWidth > 0 && ActualHeight > 0)
            {
                InitializeSnow();
                _isInitialized = true;
            }
            else if (_isInitialized)
            {
                foreach (var flake in _flakes)
                {
                    if (flake.X > ActualWidth) flake.X = _random.NextDouble() * ActualWidth;
                }
            }
        }

        private void InitializeSnow()
        {
            SnowCanvas.Children.Clear();
            _flakes.Clear();

            for (int i = 0; i < _flakeCount; i++)
            {
                var size = _random.NextDouble() * 2.5 + 1.5;
                var opacity = _random.NextDouble() * 0.3 + 0.08;

                var ellipse = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = new SolidColorBrush(Color.FromArgb(255, 220, 225, 230)),
                    Opacity = opacity,
                    IsHitTestVisible = false
                };

                if (size > 2.5)
                {
                    ellipse.Effect = new BlurEffect { Radius = 1.5, RenderingBias = RenderingBias.Performance };
                }

                var transform = new TranslateTransform();
                ellipse.RenderTransform = transform;

                double x = _random.NextDouble() * ActualWidth;
                double y = _random.NextDouble() * ActualHeight;
                
                double speedY = (size * 0.2 + _random.NextDouble() * 0.2) * 0.6;
                double speedX = (_random.NextDouble() - 0.5) * 0.15;

                var flake = new SnowFlake
                {
                    UIElement = ellipse,
                    Transform = transform,
                    X = x,
                    Y = y,
                    SpeedY = speedY,
                    SpeedX = speedX,
                    Size = size,
                    Phase = _random.NextDouble() * Math.PI * 2
                };

                Canvas.SetLeft(ellipse, 0);
                Canvas.SetTop(ellipse, 0);
                transform.X = x;
                transform.Y = y;

                SnowCanvas.Children.Add(ellipse);
                _flakes.Add(flake);
            }
        }

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            if (!_isInitialized || ActualHeight == 0) return;

            _time += 0.016;

            foreach (var flake in _flakes)
            {
                flake.Y += flake.SpeedY;
                
                double sway = Math.Sin(_time * 0.8 + flake.Phase) * 0.3;
                flake.X += flake.SpeedX + sway;

                if (flake.Y > ActualHeight)
                {
                    flake.Y = -flake.Size - 10;
                    flake.X = _random.NextDouble() * ActualWidth;
                }

                if (flake.X < -10) flake.X = ActualWidth + 5;
                if (flake.X > ActualWidth + 10) flake.X = -5;

                if (flake.Transform != null)
                {
                    flake.Transform.Y = flake.Y;
                    flake.Transform.X = flake.X;
                }
            }
        }
    }
}
