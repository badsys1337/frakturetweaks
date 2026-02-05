using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace Frakture_Tweaks
{
    public class AppItem : INotifyPropertyChanged
    {
        private bool _isSelected;
        private ImageSource? _icon;

        public string Name { get; set; } = "";
        public string PackageFullName { get; set; } = "";
        public string InstallLocation { get; set; } = "";
        public bool IsNonRemovable { get; set; }
        public bool IsFramework { get; set; }
        
        private bool _isInstalled = true;
        public bool IsInstalled
        {
            get => _isInstalled;
            set
            {
                if (_isInstalled != value)
                {
                    _isInstalled = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IconOpacity));
                    OnPropertyChanged(nameof(TextColor));
                }
            }
        }

        public double IconOpacity => IsInstalled ? 1.0 : 0.3;
        public Brush TextColor => IsInstalled ? new SolidColorBrush(Color.FromRgb(221, 221, 221)) : new SolidColorBrush(Color.FromRgb(100, 100, 100));

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

        public ImageSource? Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    
    
    
    public partial class AppListWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public ObservableCollection<AppItem> Apps { get; set; } = new ObservableCollection<AppItem>();

        public AppListWindow()
        {
            InitializeComponent();
            DataContext = this;
            AppsListBox.ItemsSource = Apps;
            Loaded += AppListWindow_Loaded;
            this.Opacity = 0;
            
            
            var view = CollectionViewSource.GetDefaultView(Apps);
            view.Filter = AppFilter;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ExecuteSelectAll();
                e.Handled = true;
            }
        }

        private void ExecuteSelectAll()
        {
            if (SearchBox.IsKeyboardFocused) return;

            var view = CollectionViewSource.GetDefaultView(Apps);
            var visibleApps = new List<AppItem>();
            foreach (AppItem item in view)
            {
                visibleApps.Add(item);
            }

            if (visibleApps.Count == 0) return;

            
            bool allSelected = visibleApps.All(a => a.IsSelected);

            foreach (var app in visibleApps)
            {
                app.IsSelected = !allSelected;
            }
        }

        private bool AppFilter(object item)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
                return true;

            var app = (AppItem)item;
            return app.Name.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(Apps).Refresh();
        }

        private void AppListWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyDarkTitleBar();
            LoadApps();

            
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

        private async void LoadApps()
        {
            StatusText.Text = "Loading apps...";
            Apps.Clear();

            await Task.Run(() =>
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell",
                        Arguments = "-Command \"Get-AppxPackage -AllUsers | Where-Object { -not $_.NonRemovable -and -not $_.IsFramework } | ForEach-Object { $_.Name + '|' + $_.PackageFullName + '|' + $_.InstallLocation }\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true, 
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = new Process { StartInfo = psi })
                    {
                        process.Start();

                        
                        var errorTask = process.StandardError.ReadToEndAsync();

                        
                        while (!process.StandardOutput.EndOfStream)
                        {
                            string? line = process.StandardOutput.ReadLine();
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                var parts = line.Split('|');
                                if (parts.Length >= 2)
                                {
                                    string name = parts[0];
                                    string fullName = parts[1];
                                    string location = parts.Length > 2 ? parts[2] : "";

                                    
                                    if (IsCriticalSystemApp(name))
                                        continue;

                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        var appItem = new AppItem 
                                        { 
                                            Name = name, 
                                            PackageFullName = fullName,
                                            InstallLocation = location
                                        };
                                        Apps.Add(appItem);
                                        
                                        _ = LoadIconForApp(appItem);
                                    });
                                }
                            }
                        }
                        
                        process.WaitForExit();
                        
                        
                        Task.WaitAll(errorTask);
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Error loading apps: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            });

            StatusText.Text = $"{Apps.Count} apps found.";
        }

        private bool IsCriticalSystemApp(string name)
        {
            string[] criticalKeywords = {
                "Microsoft.Windows.ShellExperienceHost",
                "Microsoft.Windows.StartMenuExperienceHost",
                "Microsoft.Windows.Cortana",
                "Microsoft.Windows.SecHealthUI",
                "Microsoft.Windows.AuthHost",
                "Microsoft.AccountsControl",
                "Microsoft.BioEnrollment",
                "Microsoft.LockApp",
                "Microsoft.Windows.OOBENetworkCaptivePortal",
                "Microsoft.Windows.OOBENetworkConnectionFlow",
                "Microsoft.Windows.ParentalControls",
                "Microsoft.Windows.PeopleExperienceHost",
                "Microsoft.Windows.PinningConfirmationDialog",
                "Microsoft.Windows.XGpuEjectDialog",
                "Microsoft.XboxGameCallableUI",
                "Microsoft.Windows.CapturePicker",
                "Microsoft.DeskopAppInstaller",
                "Microsoft.Windows.Apprep.ChxApp",
                "Microsoft.Windows.AssignedAccessLockApp",
                "Microsoft.Windows.CallingShellApp",
                "Microsoft.Windows.CloudExperienceHost",
                "Microsoft.Windows.ContentDeliveryManager", 
                "Microsoft.WindowsStore",
                "Microsoft.MSPaint",
                "Microsoft.WindowsNotepad",
                "Microsoft.Paint"
            };

            foreach (var keyword in criticalKeywords)
            {
                if (name.Equals(keyword, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private async Task LoadIconForApp(AppItem app)
        {
            if (string.IsNullOrEmpty(app.InstallLocation) || !Directory.Exists(app.InstallLocation))
                return;

            await Task.Run(() =>
            {
                try
                {
                    string manifestPath = Path.Combine(app.InstallLocation, "AppxManifest.xml");
                    if (!File.Exists(manifestPath)) return;

                    string logoPath = "";
                    
                    
                    try 
                    {
                        XDocument doc = XDocument.Load(manifestPath);
                        XNamespace ns = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";
                        XNamespace uap = "http://schemas.microsoft.com/appx/manifest/uap/windows10";
                        
                        
                        var visualElements = doc.Descendants(uap + "VisualElements").FirstOrDefault();
                        if (visualElements != null)
                        {
                            var logoAttr = visualElements.Attribute("Square44x44Logo") ?? visualElements.Attribute("Square150x150Logo");
                            if (logoAttr != null)
                            {
                                logoPath = logoAttr.Value;
                            }
                        }
                        
                        if (string.IsNullOrEmpty(logoPath))
                        {
                            
                            var logoNode = doc.Descendants(ns + "Logo").FirstOrDefault();
                            if (logoNode != null)
                            {
                                logoPath = logoNode.Value;
                            }
                        }
                    }
                    catch 
                    {
                        
                        string[] commonNames = { "Assets\\StoreLogo.png", "Assets\\Logo.png", "Logo.png", "AppIcon.png" };
                        foreach (var name in commonNames)
                        {
                            if (File.Exists(Path.Combine(app.InstallLocation, name)))
                            {
                                logoPath = name;
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(logoPath))
                    {
                        string fullLogoPath = Path.Combine(app.InstallLocation, logoPath);
                        
                        
                        if (!File.Exists(fullLogoPath))
                        {
                            string dir = Path.GetDirectoryName(fullLogoPath) ?? "";
                            string fileName = Path.GetFileNameWithoutExtension(fullLogoPath);
                            string ext = Path.GetExtension(fullLogoPath);
                            
                            
                            if (Directory.Exists(dir))
                            {
                                var files = Directory.GetFiles(dir, $"{fileName}*{ext}");
                                
                                fullLogoPath = files.FirstOrDefault(f => f.Contains("scale-100")) 
                                            ?? files.FirstOrDefault(f => f.Contains("targetsize-44"))
                                            ?? files.FirstOrDefault();
                            }
                        }

                        if (!string.IsNullOrEmpty(fullLogoPath) && File.Exists(fullLogoPath))
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                try
                                {
                                    BitmapImage bitmap = new BitmapImage();
                                    bitmap.BeginInit();
                                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                                    bitmap.UriSource = new Uri(fullLogoPath);
                                    bitmap.EndInit();
                                    bitmap.Freeze(); 
                                    app.Icon = bitmap;
                                }
                                catch {  }
                            });
                        }
                    }
                }
                catch
                {
                    
                }
            });
        }

        private async void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedApps = Apps.Where(a => a.IsSelected && a.IsInstalled).ToList();
            if (selectedApps.Count == 0)
            {
                MessageBox.Show("Please select installed apps to delete.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Are you sure you want to delete {selectedApps.Count} selected apps?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            await RemoveApps(selectedApps);
        }

        private async void RestoreSelected_Click(object sender, RoutedEventArgs e)
        {
            var selectedApps = Apps.Where(a => a.IsSelected && !a.IsInstalled).ToList();
            if (selectedApps.Count == 0)
            {
                MessageBox.Show("Please select deleted apps to restore.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await RestoreApps(selectedApps);
        }

        private async void DeleteAll_Click(object sender, RoutedEventArgs e)
        {
            var installedApps = Apps.Where(a => a.IsInstalled).ToList();
            if (installedApps.Count == 0) return;

            if (MessageBox.Show("Are you sure you want to delete ALL apps listed? This is potentially dangerous.", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            await RemoveApps(installedApps);
        }

        private async Task RemoveApps(List<AppItem> appsToRemove)
        {
            if (appsToRemove.Count == 0) return;
            StatusText.Text = "Removing apps...";

            await Task.Run(() =>
            {
                
                int batchSize = 25; 
                for (int i = 0; i < appsToRemove.Count; i += batchSize)
                {
                    var batch = appsToRemove.Skip(i).Take(batchSize).ToList();
                    try
                    {
                        var scriptBuilder = new StringBuilder();
                        foreach (var app in batch)
                        {
                            scriptBuilder.Append($"Write-Output 'Removing {app.Name}...'; Remove-AppxPackage -Package '{app.PackageFullName}' -ErrorAction SilentlyContinue; ");
                        }

                        var psi = new ProcessStartInfo
                        {
                            FileName = "powershell",
                            Arguments = $"-Command \"{scriptBuilder.ToString()}\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (var process = new Process { StartInfo = psi })
                        {
                            process.Start();
                            process.StandardOutput.ReadToEnd();
                            process.StandardError.ReadToEnd();
                            process.WaitForExit();
                        }
                        
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var app in batch)
                            {
                                app.IsInstalled = false;
                                app.IsSelected = false;
                            }
                        });
                    }
                    catch { }
                }
            });

            StatusText.Text = "Finished removing apps.";
        }

        private async Task RestoreApps(List<AppItem> appsToRestore)
        {
            if (appsToRestore.Count == 0) return;
            StatusText.Text = "Restoring apps...";

            await Task.Run(() =>
            {
                int batchSize = 25;
                for (int i = 0; i < appsToRestore.Count; i += batchSize)
                {
                    var batch = appsToRestore.Skip(i).Take(batchSize).ToList();
                    try
                    {
                        var psLines = string.Join("; ", batch.Select(a => $"Write-Output 'Restoring {a.Name}...'; Add-AppxPackage -Register '{Path.Combine(a.InstallLocation, "AppxManifest.xml")}' -DisableDevelopmentMode -ErrorAction SilentlyContinue"));
                        
                        var psi = new ProcessStartInfo
                        {
                            FileName = "powershell",
                            Arguments = $"-Command \"{psLines}\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (var process = new Process { StartInfo = psi })
                        {
                            process.Start();
                            process.StandardOutput.ReadToEnd();
                            process.StandardError.ReadToEnd();
                            process.WaitForExit();
                        }

                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var app in batch)
                            {
                                app.IsInstalled = true;
                                app.IsSelected = false;
                            }
                        });
                    }
                    catch { }
                }
            });

            StatusText.Text = "Finished restoring apps.";
        }
    }
}
