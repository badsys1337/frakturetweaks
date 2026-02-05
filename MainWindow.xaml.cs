using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;

namespace Frakture_Tweaks
{
    
    
    
    public partial class MainWindow : Window
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;

        public ImageSource TelegramLogoSource { get; set; }

        private TaskCompletionSource<bool>? _wizardTcs;
        private TaskCompletionSource<bool>? _restorePromptTcs;
        private bool _suppressModeEvents;
        private UIElement? _lastAdvancedView;
        private CancellationTokenSource? _applyAllCts;

        private sealed class AppState
        {
            public bool RestoreNotificationShown { get; set; }
            public bool ModeSelectionShown { get; set; }
            public string? SelectedMode { get; set; }
        }

        public MainWindow(bool? isEasyMode = null)
        {
            InitializeComponent();

            if (isEasyMode.HasValue)
            {
                var state = LoadAppState();
                state.SelectedMode = isEasyMode.Value ? "Easy" : "Advanced";
                state.ModeSelectionShown = true;
                SaveAppState(state);
            }

            TelegramLogoSource = LoadTelegramLogo();

            
            this.DataContext = this;

            Loaded += MainWindow_Loaded;
            ContentRendered += (_, __) =>
            {
                if (Opacity < 0.99)
                {
                    BeginAnimation(OpacityProperty, null);
                    Opacity = 1;
                }
            };

            
            this.Opacity = 1;

            
            this.Closed += (s, e) => Environment.Exit(0);
        }

        private static ImageSource LoadTelegramLogo()
        {
            try
            {
                var packUri = new Uri("pack://application:,,,/Resources/telegram-logo.png", UriKind.Absolute);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = packUri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                var gray = new FormatConvertedBitmap(bitmap, PixelFormats.Gray8, null, 0);
                gray.Freeze();
                return gray;
            }
            catch
            {
                
                return new DrawingImage();
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var handle = new WindowInteropHelper(this).Handle;
                int useImmersiveDarkMode = 1;

                if (DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int)) != 0)
                {
                    DwmSetWindowAttribute(handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useImmersiveDarkMode, sizeof(int));
                }

                int color = 0x001F1F1F;
                DwmSetWindowAttribute(handle, DWMWA_CAPTION_COLOR, ref color, sizeof(int));

                
                this.Opacity = 0;
                var anim = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.4));
                anim.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };

                
                anim.Completed += (s, ev) => { this.Opacity = 1; };
                BeginAnimation(OpacityProperty, anim);

                var state = LoadAppState();
                if (!state.ModeSelectionShown)
                {
                    bool easy;
                    try
                    {
                        easy = await ShowWizardAsync(
                            "Mode",
                            "Choose how you want to use Frakture Tweaks.",
                            "Easy Mode",
                            "Advanced Mode"
                        );
                    }
                    catch (OperationCanceledException)
                    {
                        easy = false;
                    }

                    state.ModeSelectionShown = true;
                    state.SelectedMode = easy ? "Easy" : "Advanced";
                    SaveAppState(state);
                }

                await ApplyModeAsync(state.SelectedMode ?? "Advanced", animate: false);
                
                
                string currentLang = Frakture_Tweaks.Services.LocalizationManager.Instance.CurrentLanguage;
                if (currentLang == "ru-RU") 
                {
                    RussianRadio.IsChecked = true;
                    AnimateLangPill(false);
                }
                else 
                {
                    EnglishRadio.IsChecked = true; 
                    AnimateLangPill(true);
                }

                CheckAndShowRestoreNotification(state);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MainWindow Load Error: {ex.Message}");
                BeginAnimation(OpacityProperty, null);
                this.Opacity = 1;
                SwitchView(HomeView);
            }
        }

        private void CheckAndShowRestoreNotification(AppState state)
        {
            try
            {
                if (!state.RestoreNotificationShown)
                {
                    NotificationOverlay.Opacity = 0;
                    NotificationOverlay.Visibility = Visibility.Visible;
                    var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                    fadeIn.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                    NotificationOverlay.BeginAnimation(OpacityProperty, fadeIn);

                    state.RestoreNotificationShown = true;
                    SaveAppState(state);
                }
            }
            catch { }
        }

        private static string GetAppStatePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return System.IO.Path.Combine(appData, "Frakture Tweaks", "app_state.json");
        }

        private static AppState LoadAppState()
        {
            try
            {
                var path = GetAppStatePath();
                if (!System.IO.File.Exists(path)) return new AppState();
                var json = System.IO.File.ReadAllText(path);
                return JsonSerializer.Deserialize<AppState>(json) ?? new AppState();
            }
            catch
            {
                return new AppState();
            }
        }

        private static void SaveAppState(AppState state)
        {
            try
            {
                var path = GetAppStatePath();
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
                var json = JsonSerializer.Serialize(state);
                System.IO.File.WriteAllText(path, json);
            }
            catch { }
        }

        private void NotificationNo_Click(object sender, RoutedEventArgs e)
        {
            _restorePromptTcs?.TrySetResult(false);
            FadeOut(NotificationOverlay, () => NotificationOverlay.Visibility = Visibility.Collapsed);
        }

        private void NotificationYes_Click(object sender, RoutedEventArgs e)
        {
            _restorePromptTcs?.TrySetResult(true);
            FadeOut(NotificationOverlay, () =>
            {
                NotificationOverlay.Visibility = Visibility.Collapsed;

                
                foreach (var child in NavPanel.Children)
                {
                    if (child is RadioButton rb && rb.Tag?.ToString() == "RestoreChangesView")
                    {
                        rb.IsChecked = true;
                        break;
                    }
                }

                SwitchView(RestoreChangesView);
                ShowSpotlight();
            });
        }

        private async Task<bool> ShowRestoreRecommendationAsync(CancellationToken token)
        {
            if (_restorePromptTcs != null) return false;

            NotificationOverlay.Opacity = 0;
            NotificationOverlay.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
            fadeIn.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            NotificationOverlay.BeginAnimation(OpacityProperty, fadeIn);

            _restorePromptTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            try
            {
                using var reg = token.Register(() => _restorePromptTcs.TrySetCanceled(token));
                bool result = await _restorePromptTcs.Task;
                return result;
            }
            finally
            {
                _restorePromptTcs = null;
                ForceHideOverlay(NotificationOverlay);
            }
        }

        private async void ShowSpotlight()
        {
            await Task.Delay(500);

            try
            {
                var btn = RestoreChangesView.RestorePointButton;
                if (btn == null) return;

                Point relativePoint = btn.TransformToAncestor(this).Transform(new Point(0, 0));
                Rect btnRect = new Rect(relativePoint, btn.RenderSize);

                var screenRect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
                var geometry = new CombinedGeometry(GeometryCombineMode.Exclude,
                    new RectangleGeometry(screenRect),
                    new RectangleGeometry(btnRect, 4, 4)
                );

                SpotlightPath.Data = geometry;

                SpotlightOverlay.Opacity = 0;
                SpotlightOverlay.Visibility = Visibility.Visible;
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                fadeIn.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                SpotlightOverlay.BeginAnimation(OpacityProperty, fadeIn);
            }
            catch { }
        }

        private void Spotlight_Click(object sender, MouseButtonEventArgs e)
        {
            FadeOut(SpotlightOverlay, () => SpotlightOverlay.Visibility = Visibility.Collapsed);
        }

        private void FadeOut(UIElement element, Action? onCompleted = null)
        {
            var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(600));
            anim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
            anim.Completed += (s, _) => onCompleted?.Invoke();
            element.BeginAnimation(OpacityProperty, anim);
        }

        private void ForceHideOverlay(UIElement element)
        {
            try
            {
                element.BeginAnimation(OpacityProperty, null);
            }
            catch { }

            element.Opacity = 0;
            element.Visibility = Visibility.Collapsed;
        }

        private Task FadeOutAsync(UIElement element, int durationMs = 250)
        {
            var tcs = new TaskCompletionSource<bool>();
            var anim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(durationMs));
            anim.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
            anim.Completed += (_, __) => tcs.TrySetResult(true);
            element.BeginAnimation(OpacityProperty, anim);
            return tcs.Task;
        }

        private void TelegramBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "https://t.me/dekutweaks",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch { }
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string viewName)
            {
                UIElement targetView = null;

                switch (viewName)
                {
                    case "HomeView": targetView = HomeView; break;
                    case "WindowsTweaksView": targetView = WindowsTweaksView; break;
                    case "RamTweaksView": targetView = RamTweaksView; break;
                    case "DelayTweaksView": targetView = DelayTweaksView; break;
                    case "InputBoostView": targetView = InputBoostView; break;
                    case "SystemBoostView": targetView = SystemBoostView; break;
                    case "EthernetTweaksView": targetView = EthernetTweaksView; break;
                    case "CpuTweaksView": targetView = CpuTweaksView; break;
                    case "GpuTweaksView": targetView = GpuTweaksView; break;
                    case "DebloatView": targetView = DebloatView; break;
                    case "PowerView": targetView = PowerView; break;
                    case "SecurityView": targetView = SecurityView; break;
                    case "RestoreChangesView": targetView = RestoreChangesView; break;
                }

                if (targetView != null && targetView.Visibility != Visibility.Visible)
                {
                    _lastAdvancedView = targetView;
                    if (EasyModeRadio.IsChecked == true)
                    {
                        _ = ApplyModeAsync("Advanced", animate: true, targetAdvancedView: targetView);
                    }
                    else
                    {
                        SwitchView(targetView);
                    }
                }
            }
        }

        private void EasyModeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_suppressModeEvents) return;
            _ = ApplyModeAsync("Easy", animate: true);
        }

        private void AdvancedModeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (_suppressModeEvents) return;
            _ = ApplyModeAsync("Advanced", animate: true);
        }

        private void EnglishRadio_Checked(object sender, RoutedEventArgs e)
        {
            Frakture_Tweaks.Services.LocalizationManager.Instance.CurrentLanguage = "en-US";
            AnimateLangPill(isEnglish: true);
        }

        private void RussianRadio_Checked(object sender, RoutedEventArgs e)
        {
            Frakture_Tweaks.Services.LocalizationManager.Instance.CurrentLanguage = "ru-RU";
            AnimateLangPill(isEnglish: false);
        }

        private void AnimateLangPill(bool isEnglish)
        {
            double targetX = isEnglish ? 0 : 50;
            var anim = new DoubleAnimation
            {
                To = targetX,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            LangSelectionTransform.BeginAnimation(TranslateTransform.XProperty, anim);
        }

        private async Task ApplyModeAsync(string mode, bool animate, UIElement? targetAdvancedView = null)
        {
            mode = mode.Equals("Easy", StringComparison.OrdinalIgnoreCase) ? "Easy" : "Advanced";

            var state = LoadAppState();
            state.SelectedMode = mode;
            state.ModeSelectionShown = true;
            SaveAppState(state);

            if (_lastAdvancedView == null)
            {
                _lastAdvancedView = HomeView;
            }

            if (mode == "Easy")
            {
                var current = GetCurrentVisibleView();
                if (current != null && current != ApplyAllView)
                {
                    _lastAdvancedView = current;
                }

                Sidebar.IsEnabled = false;

                if (animate)
                {
                    AnimateModeSelection(isEasy: true);
                    AnimateSidebarOpacity(0.2);
                }
                else
                {
                    ModeSelectionTransform.X = 0;
                    Sidebar.Opacity = 0.2;
                }

                SwitchView(ApplyAllView);
            }
            else
            {
                Sidebar.IsEnabled = true;

                if (SpotlightOverlay.Visibility == Visibility.Visible)
                {
                    FadeOut(SpotlightOverlay, () => SpotlightOverlay.Visibility = Visibility.Collapsed);
                }

                if (animate)
                {
                    AnimateModeSelection(isEasy: false);
                    AnimateSidebarOpacity(1.0);
                }
                else
                {
                    ModeSelectionTransform.X = 120;
                    Sidebar.Opacity = 1.0;
                }

                var target = targetAdvancedView ?? _lastAdvancedView ?? HomeView;
                _lastAdvancedView = target;
                SwitchView(target);
            }

            _suppressModeEvents = true;
            try
            {
                EasyModeRadio.IsChecked = mode == "Easy";
                AdvancedModeRadio.IsChecked = mode == "Advanced";
            }
            finally
            {
                _suppressModeEvents = false;
            }

            
        }

        private UIElement? GetCurrentVisibleView()
        {
            var views = new UIElement[] { HomeView, ApplyAllView, WindowsTweaksView, RamTweaksView, DelayTweaksView, InputBoostView, SystemBoostView, EthernetTweaksView, CpuTweaksView, GpuTweaksView, DebloatView, PowerView, SecurityView, RestoreChangesView };
            foreach (var v in views)
            {
                if (v.Visibility == Visibility.Visible) return v;
            }
            return null;
        }

        private void AnimateModeSelection(bool isEasy)
        {
            double targetX = isEasy ? 0 : 120;
            var anim = new DoubleAnimation
            {
                To = targetX,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            ModeSelectionTransform.BeginAnimation(TranslateTransform.XProperty, anim);
        }

        private void AnimateSidebarOpacity(double targetOpacity)
        {
            var anim = new DoubleAnimation
            {
                To = targetOpacity,
                Duration = TimeSpan.FromMilliseconds(220),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Sidebar.BeginAnimation(OpacityProperty, anim);
        }

        private void SwitchView(UIElement newView)
        {
            if (newView == null || (newView.Visibility == Visibility.Visible && newView.Opacity > 0.99)) return;

            var views = new UIElement[] { HomeView, ApplyAllView, WindowsTweaksView, RamTweaksView, DelayTweaksView, InputBoostView, SystemBoostView, EthernetTweaksView, CpuTweaksView, GpuTweaksView, DebloatView, PowerView, SecurityView, RestoreChangesView };

            
            UIElement? currentView = null;
            foreach (var v in views)
            {
                if (v.Visibility == Visibility.Visible)
                {
                    currentView = v;
                    break;
                }
            }

            
            AnimateTelegramVisibility(newView == HomeView);

            
            if (currentView != null && currentView != newView)
            {
                
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.12));
                fadeOut.Completed += (s, e) =>
                {
                    currentView.Visibility = Visibility.Collapsed;
                    currentView.BeginAnimation(OpacityProperty, null); 

                    
                    PerformFadeIn(newView);
                };
                currentView.BeginAnimation(OpacityProperty, fadeOut);
            }
            else
            {
                
                PerformFadeIn(newView);
            }

            
            
            foreach (var v in views)
            {
                if (v != newView && v != currentView)
                {
                    v.Visibility = Visibility.Collapsed;
                    v.Opacity = 0;
                    v.BeginAnimation(OpacityProperty, null);
                }
            }
        }

        private void PerformFadeIn(UIElement view)
        {
            view.Opacity = 0;
            view.Visibility = Visibility.Visible;

            var translate = new TranslateTransform(0, 15);
            view.RenderTransform = translate;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.2));
            var slideUp = new DoubleAnimation(15, 0, TimeSpan.FromSeconds(0.2));
            slideUp.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };

            view.BeginAnimation(OpacityProperty, fadeIn);
            translate.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        private void AnimateTelegramVisibility(bool show)
        {
            var targetOpacity = show ? 0.4 : 0.0;
            var currentOpacity = TelegramBtn.Opacity;

            if (Math.Abs(currentOpacity - targetOpacity) < 0.01) return;

            if (show) TelegramBtn.Visibility = Visibility.Visible;

            var anim = new DoubleAnimation
            {
                To = targetOpacity,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            anim.Completed += (s, e) =>
            {
                if (!show) TelegramBtn.Visibility = Visibility.Collapsed;
            };

            TelegramBtn.BeginAnimation(OpacityProperty, anim);
        }

        private async void StartTweakingBtn_Click(object sender, RoutedEventArgs e)
        {
            
            if (_applyAllCts != null)
            {
                _applyAllCts.Cancel();
                ApplyAllStatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Stat_Canceling");
                ForceHideOverlay(WizardOverlay);
                ForceHideOverlay(NotificationOverlay);
                return;
            }

            _applyAllCts = new CancellationTokenSource();
            var token = _applyAllCts.Token;

            
            StartTweakingBtnText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Btn_Cancel");
            ApplyAllStatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Stat_AnswerQuestions");

            LogWindow? logWindow = null;
            bool canceled = false;

            try
            {
                var options = new ApplyAllOptions();

                options.IsLaptop = await AskPlatformAsync(token);

                options.CreateRestorePoint = await ShowWizardAsync(
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Wiz_SysProt_Title"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Wiz_SysProt_Msg"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Btn_YesCreate"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Btn_Skip"),
                    token
                );

                options.DisableMicrosoftStore = await ShowWizardAsync(
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Wiz_Store_Title"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Wiz_Store_Msg"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Btn_YesDisable"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Btn_Keep"),
                    token
                );
                options.DisableMitigations = await ShowWizardAsync(
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Wiz_Mitigations_Title"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Wiz_Mitigations_Msg"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Btn_YesDisable"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Btn_Keep"),
                    token
                );
                options.DisableWindowsUpdate = await ShowWizardAsync(
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Wiz_WinUpdate_Title"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Wiz_WinUpdate_Msg"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Btn_YesDisable"),
                    Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Btn_Keep"),
                    token
                );

                
                

                ApplyAllStatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Stat_OpeningLog");
                logWindow = new LogWindow(LogWindowMode.Large);
                logWindow.Show();
                logWindow.AddLog(Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Stat_EasyModeStarted"));

                ApplyAllStatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Stat_Applying");
                await ApplyAllTweaksAsync(options, logWindow, token);
                ApplyAllStatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Stat_Completed");
            }
            catch (OperationCanceledException)
            {
                canceled = true;
                ApplyAllStatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Stat_Canceled");
                logWindow?.AddLog(Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_CanceledByUser"));
            }
            catch (Exception ex)
            {
                ApplyAllStatusText.Text = Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Stat_Error");
                if (logWindow == null)
                {
                    logWindow = new LogWindow(LogWindowMode.Large);
                    logWindow.Show();
                }
                logWindow.AddLog(string.Format(Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Log_CriticalError"), ex.Message));
            }
            finally
            {
                try
                {
                    _applyAllCts?.Dispose();
                }
                catch { }
                _applyAllCts = null;

                
                StartTweakingBtnText.Text = Application.Current.Resources["Easy_StartBtn"] as string ?? "Start Optimization";

                ForceHideOverlay(WizardOverlay);
                ForceHideOverlay(NotificationOverlay);

                if (canceled)
                {
                    ForceHideOverlay(SpotlightOverlay);
                }
            }
        }

        private async Task<bool> AskPlatformAsync(CancellationToken token)
        {
            bool pc = await ShowWizardAsync(
                Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Wiz_Platform_Title"),
                Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Wiz_Platform_Msg"),
                Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Btn_PC"),
                Frakture_Tweaks.Services.LocalizationManager.Instance.GetString("Btn_Laptop"),
                token
            );
            return !pc;
        }

        private Task<bool> ShowWizardAsync(string title, string message, string primaryText, string secondaryText)
        {
            return ShowWizardAsync(title, message, primaryText, secondaryText, CancellationToken.None);
        }

        private async Task<bool> ShowWizardAsync(string title, string message, string primaryText, string secondaryText, CancellationToken token)
        {
            if (_wizardTcs != null) return false;

            WizardTitleText.Text = title;
            WizardMessageText.Text = message;
            WizardPrimaryBtn.Content = primaryText;
            WizardSecondaryBtn.Content = secondaryText;
            WizardOverlay.Opacity = 0;
            WizardOverlay.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
            fadeIn.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
            WizardOverlay.BeginAnimation(OpacityProperty, fadeIn);

            _wizardTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            try
            {
                using var reg = token.Register(() => _wizardTcs.TrySetCanceled(token));
                bool result = await _wizardTcs.Task;
                return result;
            }
            finally
            {
                _wizardTcs = null;
                await FadeOutAsync(WizardOverlay);
                WizardOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void WizardPrimaryBtn_Click(object sender, RoutedEventArgs e)
        {
            _wizardTcs?.TrySetResult(true);
        }

        private void WizardSecondaryBtn_Click(object sender, RoutedEventArgs e)
        {
            _wizardTcs?.TrySetResult(false);
        }

        private void WizardCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_applyAllCts != null)
            {
                ForceHideOverlay(WizardOverlay);
                _applyAllCts.Cancel();
                return;
            }

            _wizardTcs?.TrySetCanceled();
        }

        private async Task CreateRestorePointAsync(LogWindow logWindow, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            logWindow.AddLog("Creating System Restore Point...");

            string psCommand = "Checkpoint-Computer -Description \"Frakture Tweaks Easy Mode\" -RestorePointType \"MODIFY_SETTINGS\"";

            try
            {
                await Task.Run(() =>
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-ExecutionPolicy Bypass -Command \"{psCommand}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    using (var process = Process.Start(psi))
                    {
                        if (process != null)
                        {
                            process.WaitForExit();
                            if (process.ExitCode == 0) logWindow.AddLog("Restore Point created successfully.");
                            else logWindow.AddLog("Failed to create Restore Point. Ensure System Protection is enabled.");
                        }
                        else
                        {
                            logWindow.AddLog("Error: Could not start PowerShell for Restore Point.");
                        }
                    }
                }, token);
            }
            catch (Exception ex)
            {
                logWindow.AddLog($"Error creating Restore Point: {ex.Message}");
            }
        }

        private async Task ApplyAllTweaksAsync(ApplyAllOptions options, LogWindow logWindow, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var hw = DetectHardware(logWindow);
            logWindow.AddLog($"Platform: {(options.IsLaptop ? "Laptop" : "PC")}");
            logWindow.AddLog($"RAM: {hw.RamGb} GB");
            logWindow.AddLog($"CPU: {hw.CpuVendor} {hw.CpuName}");
            logWindow.AddLog("GPU: {hw.GpuVendor} {hw.GpuName}");

            logWindow.AddLog("--------------------------------------------------");

            if (options.CreateRestorePoint)
            {
                await CreateRestorePointAsync(logWindow, token);
                token.ThrowIfCancellationRequested();
            }

            var global = new GlobalTweaks(logWindow);
            await global.ApplyGlobalTweaksAsync();
            token.ThrowIfCancellationRequested();

            if (options.DisableMicrosoftStore)
            {
                await DisableMicrosoftStoreAsync(logWindow, token);
            }

            token.ThrowIfCancellationRequested();

            await ApplyGlobalRamTweaksAsync(logWindow, token);
            token.ThrowIfCancellationRequested();

            var delay = new DelayTweaks(logWindow);
            await delay.DemolishDelayAsync();
            token.ThrowIfCancellationRequested();
            if (options.DisableMitigations)
            {
                await delay.DisableMitigationsAsync();
            }

            token.ThrowIfCancellationRequested();

            var input = new InputBoostService(logWindow);
            await input.ApplyInputBoostAsync();
            token.ThrowIfCancellationRequested();

            var sys = new SystemBoostService(logWindow);
            await sys.ApplySystemBoostAsync();
            token.ThrowIfCancellationRequested();

            var net = new EthernetTweaks(logWindow);
            await net.ApplyGlobalInternetTweaksAsync();
            await net.ApplyExtraNetworkOptimizationsAsync();
            await net.OptimizeAdapterAsync();
            await net.SetDnsPriorityAsync();
            await net.SetNetworkTaskOffloadAsync();
            await net.DisableNaglesAlgorithmAsync();

            token.ThrowIfCancellationRequested();

            await ApplyAppDebloatAsync(logWindow, token);
            await ApplyDebloatServicesAndTasksAsync(logWindow, options.IsLaptop, token);
            token.ThrowIfCancellationRequested();
            await ApplySystemCleanupAsync(logWindow, token);
            token.ThrowIfCancellationRequested();

            
            

            token.ThrowIfCancellationRequested();

            if (options.DisableWindowsUpdate)
            {
                await DisableWindowsUpdateAsync(logWindow, token);
            }

            token.ThrowIfCancellationRequested();

            var cpu = new CpuTweaks(logWindow);
            await cpu.ApplyGlobalCpuTweaksAsync();
            token.ThrowIfCancellationRequested();
            if (hw.CpuVendor == CpuVendor.Intel)
            {
                await cpu.ApplyIntelCpuTweaksAsync();
            }
            else if (hw.CpuVendor == CpuVendor.Amd)
            {
                await cpu.ApplyAmdCpuTweaksAsync();
            }

            token.ThrowIfCancellationRequested();

            var gpu = new GpuTweaks(logWindow);
            if (hw.GpuVendor == GpuVendor.Nvidia)
            {
                await gpu.ApplyNvidiaTweaksAsync();
            }
            else if (hw.GpuVendor == GpuVendor.Amd)
            {
                await gpu.ApplyAmdTweaksAsync();
            }

            token.ThrowIfCancellationRequested();

            await ApplyPowerTweaksAsync(logWindow, options.IsLaptop, token);

            logWindow.AddLog("--------------------------------------------------");
            logWindow.AddLog("Easy Mode complete. Restart recommended.");
        }

        private async Task ApplySystemCleanupAsync(LogWindow logWindow, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            logWindow.AddLog("Running system cleanup...");

            var commands = new List<string>
            {
                "del /s /f /q %temp%\\*.* >nul 2>&1",
                "rd /s /q %temp% >nul 2>&1",
                "md %temp% >nul 2>&1",
                "del /s /f /q C:\\Windows\\Temp\\*.* >nul 2>&1",
                "rd /s /q C:\\Windows\\Temp >nul 2>&1",
                "md C:\\Windows\\Temp >nul 2>&1",
                "del /s /f /q C:\\Windows\\Prefetch\\*.* >nul 2>&1",
                "del /s /f /q C:\\Windows\\SoftwareDistribution\\Download\\*.* >nul 2>&1",
                "ipconfig /flushdns >nul 2>&1"
            };

            await ExecuteBatchAsync(commands, logWindow, token);
            logWindow.AddLog("System cleanup completed.");
        }

        private async Task ApplyPowerTweaksAsync(LogWindow logWindow, bool isLaptop, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            logWindow.AddLog("Applying power tweaks...");

            var commands = new List<string>
            {
                "powercfg /hibernate off",
                "reg add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Power\\PowerThrottling\" /v PowerThrottlingOff /t REG_DWORD /d 1 /f",
                
                "powercfg /SETACVALUEINDEX SCHEME_CURRENT SUB_PROCESSOR CPMINCORES 100",
                "powercfg /SETDCVALUEINDEX SCHEME_CURRENT SUB_PROCESSOR CPMINCORES 100",
                "powercfg /SETACVALUEINDEX SCHEME_CURRENT SUB_PROCESSOR CPMAXCORES 100",
                "powercfg /SETDCVALUEINDEX SCHEME_CURRENT SUB_PROCESSOR CPMAXCORES 100",
                "powercfg /SETACTIVE SCHEME_CURRENT"
            };

            if (!isLaptop)
            {
                commands.Insert(1, "powercfg /duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");
                commands.Add("powercfg -x -standby-timeout-ac 0");
                commands.Add("powercfg -x -standby-timeout-dc 0");
            }

            await ExecuteBatchAsync(commands, logWindow, token);
            logWindow.AddLog("Power tweaks applied.");
        }

        private async Task DisableVbsAsync(LogWindow logWindow, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            logWindow.AddLog("Disabling VBS / Memory Integrity...");
            string logFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"frakture_vbs_{Guid.NewGuid()}.log");
            string batchContent = "( \r\n" +
                                  "echo Disabling VBS and Memory Integrity... \r\n" +
                                  "reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\DeviceGuard\" /v \"EnableVirtualizationBasedSecurity\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                  "reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\DeviceGuard\\Scenarios\\HypervisorEnforcedCodeIntegrity\" /v \"Enabled\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                  "reg add \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Control\\Lsa\" /v \"LsaCfgFlags\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                  "reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\DeviceGuard\" /v \"EnableVirtualizationBasedSecurity\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                  "reg add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Policies\\Microsoft\\Windows\\DeviceGuard\" /v \"HVCIMATRequired\" /t \"REG_DWORD\" /d \"0\" /f \r\n" +
                                  $") > \"{logFile}\" 2>&1";

            try
            {
                await TrustedInstaller.RunCommandAsTrustedInstaller(batchContent, logWindow);
                token.ThrowIfCancellationRequested();
                if (System.IO.File.Exists(logFile))
                {
                    string output = System.IO.File.ReadAllText(logFile);
                    if (!string.IsNullOrWhiteSpace(output)) logWindow.AddLog(output.Trim());
                }
                logWindow.AddLog("VBS / Memory Integrity disabled (restart required).");
            }
            finally
            {
                if (System.IO.File.Exists(logFile))
                {
                    try { System.IO.File.Delete(logFile); } catch { }
                }
            }
        }

        private static HardwareInfo DetectHardware(LogWindow log)
        {
            var info = new HardwareInfo();

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject result in searcher.Get())
                    {
                        var kb = Convert.ToUInt64(result["TotalVisibleMemorySize"]);
                        info.RamGb = (int)Math.Max(1, Math.Round(kb / 1024d / 1024d));
                        break;
                    }
                }
            }
            catch { }

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Name FROM Win32_Processor"))
                {
                    foreach (ManagementObject result in searcher.Get())
                    {
                        info.CpuName = (result["Name"]?.ToString() ?? string.Empty).Trim();
                        var manufacturer = (result["Manufacturer"]?.ToString() ?? string.Empty).Trim();
                        if (manufacturer.Contains("Intel", StringComparison.OrdinalIgnoreCase) || manufacturer.Contains("GenuineIntel", StringComparison.OrdinalIgnoreCase))
                        {
                            info.CpuVendor = CpuVendor.Intel;
                        }
                        else if (manufacturer.Contains("AMD", StringComparison.OrdinalIgnoreCase) || manufacturer.Contains("AuthenticAMD", StringComparison.OrdinalIgnoreCase))
                        {
                            info.CpuVendor = CpuVendor.Amd;
                        }
                        break;
                    }
                }
            }
            catch { }

            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController"))
                {
                    foreach (ManagementObject result in searcher.Get())
                    {
                        string name = (result["Name"]?.ToString() ?? string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(name)) continue;
                        if (name.Contains("Microsoft Basic Display", StringComparison.OrdinalIgnoreCase)) continue;

                        info.GpuName = name;
                        if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase)) info.GpuVendor = GpuVendor.Nvidia;
                        else if (name.Contains("AMD", StringComparison.OrdinalIgnoreCase) || name.Contains("Radeon", StringComparison.OrdinalIgnoreCase)) info.GpuVendor = GpuVendor.Amd;
                        else if (name.Contains("Intel", StringComparison.OrdinalIgnoreCase)) info.GpuVendor = GpuVendor.Intel;
                        break;
                    }
                }
            }
            catch { }

            if (info.RamGb == 0) info.RamGb = 8;
            if (info.CpuVendor == CpuVendor.Unknown) log.AddLog("CPU vendor detection failed; skipping vendor-specific CPU tweaks.");
            if (info.GpuVendor == GpuVendor.Unknown) log.AddLog("GPU vendor detection failed; skipping vendor-specific GPU tweaks.");

            return info;
        }

        private async Task ApplyGlobalRamTweaksAsync(LogWindow logWindow, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            logWindow.AddLog("Applying Global RAM Tweaks...");
            List<string> commands = new List<string>();

            try
            {
                ulong totalMemoryKB = 0;
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject result in searcher.Get())
                    {
                        totalMemoryKB = Convert.ToUInt64(result["TotalVisibleMemorySize"]);
                        break;
                    }
                }

                if (totalMemoryKB > 0)
                {
                    ulong svcHostValue = totalMemoryKB + 1024000;
                    commands.Add($"Reg.exe add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\" /v \"SvcHostSplitThresholdInKB\" /t REG_DWORD /d \"{svcHostValue}\" /f");
                    uint lockLimit = (totalMemoryKB < 8000000) ? 65536u : (totalMemoryKB < 16000000) ? 131072u : 262144u;
                    commands.Add($"Reg.exe add \"HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management\" /v \"IoPageLockLimit\" /t REG_DWORD /d \"{lockLimit}\" /f");
                }
            }
            catch { }

            string mmKey = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Memory Management";
            string pfKey = mmKey + "\\PrefetchParameters";

            commands.Add($"Reg.exe add \"{mmKey}\" /v \"LargeSystemCache\" /t REG_DWORD /d \"1\" /f");
            commands.Add($"Reg.exe add \"{pfKey}\" /v \"EnablePrefetcher\" /t REG_DWORD /d \"0\" /f");
            commands.Add($"Reg.exe add \"{pfKey}\" /v \"EnableSuperfetch\" /t REG_DWORD /d \"0\" /f");
            commands.Add($"Reg.exe add \"{mmKey}\" /v \"ClearPageFileAtShutdown\" /t REG_DWORD /d \"0\" /f");
            commands.Add($"Reg.exe add \"{mmKey}\" /v \"DisablePagingExecutive\" /t REG_DWORD /d \"1\" /f");
            commands.Add($"Reg.exe add \"{mmKey}\" /v \"NonPagedPoolQuota\" /t REG_DWORD /d \"0\" /f");
            commands.Add($"Reg.exe add \"{mmKey}\" /v \"NonPagedPoolSize\" /t REG_DWORD /d \"0\" /f");
            commands.Add($"Reg.exe add \"{mmKey}\" /v \"PagedPoolQuota\" /t REG_DWORD /d \"0\" /f");
            commands.Add($"Reg.exe add \"{mmKey}\" /v \"PagedPoolSize\" /t REG_DWORD /d \"192\" /f");
            commands.Add($"Reg.exe add \"{mmKey}\" /v \"SecondLevelDataCache\" /t REG_DWORD /d \"1024\" /f");
            commands.Add($"Reg.exe add \"{mmKey}\" /v \"SessionPoolSize\" /t REG_DWORD /d \"192\" /f");
            commands.Add($"Reg.exe add \"{mmKey}\" /v \"SessionViewSize\" /t REG_DWORD /d \"192\" /f");
            commands.Add($"Reg.exe add \"{mmKey}\" /v \"SystemPages\" /t REG_DWORD /d \"4294967295\" /f");
            commands.Add($"Reg.exe add \"{mmKey}\" /v \"PhysicalAddressExtension\" /t REG_DWORD /d \"1\" /f");

            commands.Add("powershell -Command \"Disable-MMAgent -MemoryCompression\"");
            commands.Add("powershell -Command \"Disable-MMAgent -PageCombining\"");
            commands.Add("powershell -Command \"Disable-MMAgent -ApplicationPreLaunch\"");

            await ExecuteBatchAsync(commands, logWindow, token);
            logWindow.AddLog("Global RAM Tweaks applied.");
        }

        private async Task DisableWindowsUpdateAsync(LogWindow logWindow, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            logWindow.AddLog("Disabling Windows Update (policy + services + tasks)...");

            var commands = new List<string>
            {
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU\" /v NoAutoUpdate /t REG_DWORD /d 1 /f",
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU\" /v AUOptions /t REG_DWORD /d 2 /f",
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\" /v DisableWindowsUpdateAccess /t REG_DWORD /d 1 /f",
                "for %%i in (wuauserv UsoSvc bits dosvc WaaSMedicSvc) do (sc stop %%i >nul 2>&1 & sc config %%i start= disabled >nul 2>&1)",
                "schtasks /Change /TN \"Microsoft\\Windows\\UpdateOrchestrator\\Schedule Scan\" /Disable >nul 2>&1",
                "schtasks /Change /TN \"Microsoft\\Windows\\UpdateOrchestrator\\USO_UxBroker\" /Disable >nul 2>&1",
                "schtasks /Change /TN \"Microsoft\\Windows\\WindowsUpdate\\Automatic App Update\" /Disable >nul 2>&1"
            };

            await ExecuteBatchAsync(commands, logWindow, token);
            logWindow.AddLog("Windows Update disabled (best-effort).");
        }

        private async Task DisableMicrosoftStoreAsync(LogWindow logWindow, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            logWindow.AddLog("Disabling Microsoft Store (policy)...");

            var commands = new List<string>
            {
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsStore\" /v RemoveWindowsStore /t REG_DWORD /d 1 /f",
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsStore\" /v DisableStoreApps /t REG_DWORD /d 1 /f",
                "reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\WindowsStore\" /v AutoDownload /t REG_DWORD /d 2 /f"
            };

            await ExecuteBatchAsync(commands, logWindow, token);
            logWindow.AddLog("Microsoft Store disabled (best-effort).");
        }

        private async Task ApplyDebloatServicesAndTasksAsync(LogWindow logWindow, bool isLaptop, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            logWindow.AddLog("Applying Debloat (services + tasks)...");

            var services = new Dictionary<string, string>
            {
                { "WSearch", "Windows Search" },
                { "SSDPSRV", "SSDP Discovery" },
                { "lfsvc", "Geolocation Service" },
                { "AXInstSV", "ActiveX Installer" },
                { "AJRouter", "AllJoyn Router Service" },
                { "AppReadiness", "App Readiness" },
                { "HomeGroupListener", "HomeGroup Listener" },
                { "HomeGroupProvider", "HomeGroup Provider" },
                { "SharedAccess", "Internet Connection Sharing" },
                { "lltdsvc", "Link-Layer Topology Discovery Mapper" },
                { "diagnosticshub.standardcollector.service", "Microsoft(R) Diagnostics Hub Standard Collector Service" },
                { "wlidsvc", "Microsoft Account Sign-in Assistant" },
                { "SmsRouter", "Microsoft Windows SMS Router Service" },
                { "NcdAutoSetup", "Network Connected Devices Auto-Setup" },
                { "PNRPsvc", "Peer Name Resolution Protocol" },
                { "p2psvc", "Peer Networking Group" },
                { "p2pimsvc", "Peer Networking Identity Manager" },
                { "PNRPAutoReg", "PNRP Machine Name Publication Service" },
                { "WalletService", "WalletService" },
                { "WMPNetworkSvc", "Windows Media Player Network Sharing Service" },
                { "icssvc", "Windows Mobile Hotspot" },
                { "XblAuthManager", "Xbox Live Auth Manager" },
                { "XblGameSave", "Xbox Live Game Save" },
                { "XboxNetApiSvc", "Xbox Live Networking Service" },
                { "DmEnrollmentSvc", "Device Management Enrollment Service" },
                { "RetailDemo", "Retail Demo Service" },
                { "DiagTrack", "Connected User Experiences and Telemetry" },
                { "dmwappushservice", "WAP Push Message Routing Service" },
                { "WerSvc", "Windows Error Reporting Service" },
                { "MapsBroker", "Downloaded Maps Manager" },
                { "Spooler", "Print Spooler" },
                { "SysMain", "SysMain" },
                { "GpuEnergyDrv", "GPU Energy Driver" },
                { "GpuEnergyDr", "GPU Energy Driver (Kernel)" },
                { "bam", "Background Activity Moderator" },
                { "dam", "Desktop Activity Moderator" },
                { "BTAGService", "Bluetooth Audio Gateway Service" },
                { "bthserv", "Bluetooth Support Service" },
                { "BthAvctpSvc", "Bluetooth AVCTP Service" },
                { "BluetoothUserService", "Bluetooth User Support Service" },
                { "XboxGipSvc", "Xbox Accessory Management Service" }
            };

            if (isLaptop)
            {
                services.Remove("BTAGService");
                services.Remove("bthserv");
                services.Remove("BthAvctpSvc");
                services.Remove("BluetoothUserService");
            }

            var scheduledTasks = new List<string>
            {
                "Microsoft\\Windows\\Application Experience\\ProgramDataUpdater",
                "Microsoft\\Windows\\Autochk\\Proxy",
                "Microsoft\\Windows\\Customer Experience Improvement Program\\Consolidator",
                "Microsoft\\Windows\\Customer Experience Improvement Program\\UsbCeip",
                "Microsoft\\Windows\\Defrag\\ScheduledDefrag",
                "Microsoft\\Windows\\DiskFootprint\\Diagnostics",
                "Microsoft\\Windows\\DiskFootprint\\StorageSense",
                "Microsoft\\Windows\\DUSM\\dusmtask",
                "Microsoft\\Windows\\Feedback\\Siuf\\DmClient",
                "Microsoft\\Windows\\Input\\TouchpadSyncDataAvailable",
                "Microsoft\\Windows\\International\\Synchronize Language Settings",
                "Microsoft\\Windows\\Maintenance\\WinSAT",
                "Microsoft\\Windows\\Maps\\MapsToastTask",
                "Microsoft\\Windows\\Maps\\MapsUpdateTask",
                "Microsoft\\Windows\\PI\\Sqm-Tasks",
                "Microsoft\\Windows\\Power Efficiency Diagnostics\\AnalyzeSystem",
                "Microsoft\\Windows\\PushToInstall\\Registration"
            };

            var batch = new List<string>();
            foreach (var service in services)
            {
                batch.Add($"sc config {service.Key} start= disabled >nul 2>&1");
                batch.Add($"sc stop {service.Key} >nul 2>&1");
            }

            foreach (var task in scheduledTasks)
            {
                batch.Add($"schtasks /Change /TN \"{task}\" /Disable >nul 2>&1");
            }

            await ExecuteBatchAsync(batch, logWindow, token);
            logWindow.AddLog("Debloat applied.");
        }

        private async Task ApplyAppDebloatAsync(LogWindow logWindow, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            logWindow.AddLog("Removing Bloatware Apps...");

            string[] bloatware = {
                "Microsoft.ZuneVideo", "Microsoft.ZuneMusic", "Microsoft.SkypeApp", "Microsoft.Messaging",
                "Microsoft.OneConnect", "Microsoft.BingWeather", "Microsoft.BingNews", "Microsoft.BingSports",
                "Microsoft.BingFinance", "Microsoft.GetHelp", "Microsoft.Getstarted", "Microsoft.Office.OneNote",
                "Microsoft.People", "Microsoft.WindowsMaps", "Microsoft.YourPhone", "Microsoft.WindowsFeedbackHub",
                "Microsoft.XboxApp", "Microsoft.XboxGameOverlay", "Microsoft.XboxGamingOverlay",
                "Microsoft.XboxIdentityProvider", "Microsoft.XboxSpeechToTextOverlay",
                "Microsoft.MixedReality.Portal", "Microsoft.ScreenSketch", "Microsoft.WindowsAlarms",
                "Microsoft.WindowsCamera"
            };

            
            string packageList = string.Join("', '", bloatware);
            string psCommand = $"$apps = @('{packageList}'); Get-AppxPackage -AllUsers | Where-Object {{ $_.Name -in $apps }} | ForEach-Object {{ Write-Output \"Removing $($_.Name)...\"; Remove-AppxPackage -Package $_.PackageFullName -ErrorAction SilentlyContinue }}";

            var commands = new List<string> { $"powershell -Command \"{psCommand}\"" };

            await ExecuteBatchAsync(commands, logWindow, token);
            logWindow.AddLog("App debloat finished.");
        }

        private async Task ExecuteBatchAsync(List<string> commands, LogWindow logWindow, CancellationToken token)
        {
            if (commands.Count == 0) return;

            token.ThrowIfCancellationRequested();

            string tempBat = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"frakture_applyall_{Guid.NewGuid()}.bat");
            try
            {
                var lines = new List<string> { "@echo off", "chcp 65001 > nul" };
                lines.AddRange(commands);
                System.IO.File.WriteAllLines(tempBat, lines);

                await Task.Run(() =>
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{tempBat}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        Verb = "runas"
                    };

                    using (var p = new Process { StartInfo = psi })
                    {
                        using var reg = token.Register(() =>
                        {
                            try
                            {
                                if (!p.HasExited) p.Kill(true);
                            }
                            catch { }
                        });

                        p.OutputDataReceived += (_, a) =>
                        {
                            if (!string.IsNullOrWhiteSpace(a.Data)) logWindow.AddLog(a.Data.Trim());
                        };
                        p.ErrorDataReceived += (_, a) =>
                        {
                            if (!string.IsNullOrWhiteSpace(a.Data)) logWindow.AddLog($"ERROR: {a.Data.Trim()}");
                        };
                        p.Start();
                        p.BeginOutputReadLine();
                        p.BeginErrorReadLine();
                        p.WaitForExit();

                        token.ThrowIfCancellationRequested();
                    }
                }, token);
            }
            finally
            {
                if (System.IO.File.Exists(tempBat))
                {
                    try { System.IO.File.Delete(tempBat); } catch { }
                }
            }
        }

        private struct ApplyAllOptions
        {
            public bool IsLaptop;
            public bool CreateRestorePoint;
            public bool DisableMicrosoftStore;
            public bool DisableMitigations;
            public bool DisableWindowsUpdate;
            public bool DisableVbs;
        }

        private struct HardwareInfo
        {
            public int RamGb;
            public CpuVendor CpuVendor;
            public string CpuName;
            public GpuVendor GpuVendor;
            public string GpuName;
        }

        private enum CpuVendor
        {
            Unknown,
            Intel,
            Amd
        }

        private enum GpuVendor
        {
            Unknown,
            Nvidia,
            Amd,
            Intel
        }
    }
}
