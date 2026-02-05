using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Management; 

namespace Frakture_Tweaks
{
    public class ServiceItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        
        public string Name { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Status { get; set; } = ""; 
        public bool IsEnabled { get; set; } 

        public double IconOpacity => IsEnabled ? 1.0 : 0.5;
        public Brush TextColor => IsEnabled ? new SolidColorBrush(Color.FromRgb(221, 221, 221)) : new SolidColorBrush(Color.FromRgb(150, 150, 150));

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public void RefreshVisuals()
        {
            OnPropertyChanged(nameof(IconOpacity));
            OnPropertyChanged(nameof(TextColor));
        }
    }

    public partial class ServiceListWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public ObservableCollection<ServiceItem> Services { get; set; } = new ObservableCollection<ServiceItem>();

        public ServiceListWindow()
        {
            InitializeComponent();
            DataContext = this;
            ServicesListBox.ItemsSource = Services;
            Loaded += ServiceListWindow_Loaded;
            this.Opacity = 0;

            var view = CollectionViewSource.GetDefaultView(Services);
            view.Filter = ServiceFilter;
        }

        private void ServiceListWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyDarkTitleBar();
            LoadServices();

            var anim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.25));
            anim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            this.BeginAnimation(OpacityProperty, anim);
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

        private async void LoadServices()
        {
            StatusText.Text = "Loading services...";
            Services.Clear();

            await Task.Run(() =>
            {
                try
                {
                    var serviceList = new List<ServiceItem>();

                    
                    using (var searcher = new ManagementObjectSearcher("SELECT Name, DisplayName, State, StartMode FROM Win32_Service"))
                    using (var collection = searcher.Get())
                    {
                        foreach (ManagementObject obj in collection)
                        {
                            string name = obj["Name"]?.ToString() ?? "";
                            string displayName = obj["DisplayName"]?.ToString() ?? "";
                            string state = obj["State"]?.ToString() ?? ""; 
                            string startMode = obj["StartMode"]?.ToString() ?? ""; 

                            
                            bool isEnabled = !string.Equals(startMode, "Disabled", StringComparison.OrdinalIgnoreCase);

                            serviceList.Add(new ServiceItem
                            {
                                Name = name,
                                DisplayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName,
                                Status = state,
                                IsEnabled = isEnabled
                            });
                        }
                    }

                    
                    serviceList.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var s in serviceList)
                        {
                            Services.Add(s);
                        }
                        StatusText.Text = $"{Services.Count} services found.";
                    });
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Error loading services: {ex.Message}");
                        StatusText.Text = "Error loading services.";
                    });
                }
            });
        }

        private bool ServiceFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
                return true;

            var svc = (ServiceItem)item;
            return svc.DisplayName.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase) || 
                   svc.Name.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(Services).Refresh();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
             var view = CollectionViewSource.GetDefaultView(Services);
             foreach(ServiceItem item in view)
             {
                 item.IsSelected = true;
             }
        }

        private async void EnableSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected = Services.Where(s => s.IsSelected).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select services to enable.");
                return;
            }

            var logWindow = new LogWindow();
            logWindow.Show();
            logWindow.AddLog($"Starting to enable {selected.Count} services...");

            StatusText.Text = "Enabling services...";
            int successCount = 0;
            int failCount = 0;

            await Task.Run(() =>
            {
                foreach (var svc in selected)
                {
                    try
                    {
                        logWindow.AddLog($"Enabling: {svc.DisplayName} ({svc.Name})...");

                        
                        var psi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "sc",
                            Arguments = $"config \"{svc.Name}\" start= auto",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        var p = System.Diagnostics.Process.Start(psi);
                        p?.WaitForExit();
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            svc.IsEnabled = true;
                            svc.RefreshVisuals();
                        });
                        
                        successCount++;
                        logWindow.AddLog($"Success: {svc.DisplayName} set to Auto start.");
                    }
                    catch (Exception ex)
                    {
                        failCount++;
                        logWindow.AddLog($"Error enabling {svc.Name}: {ex.Message}");
                    }
                }
                
                logWindow.AddLog("--------------------------------------------------");
                logWindow.AddLog($"Operation complete. Enabled: {successCount}, Failed: {failCount}");
            });

            StatusText.Text = $"Finished. Enabled: {successCount}, Failed: {failCount}";
            
        }
    }
}

