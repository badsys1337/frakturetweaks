using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace Frakture_Tweaks.Services
{
    public class LocalizationManager : INotifyPropertyChanged
    {
        private static LocalizationManager? _instance;
        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? LanguageChanged;

        private string _currentLanguage = "en-US";
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    SwitchLanguage(value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentLanguage)));
                    LanguageChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private LocalizationManager() { }

        public void Initialize()
        {
            
            SettingsManager.Load();
            string savedLang = SettingsManager.Settings.Language;
            if (string.IsNullOrEmpty(savedLang)) savedLang = "en-US";
            
            SwitchLanguage(savedLang);
        }

        public void SwitchLanguage(string cultureCode)
        {
            _currentLanguage = cultureCode;
            
            
            SettingsManager.Settings.Language = cultureCode;
            SettingsManager.Save();

            
            string dictPath = $"Resources/Languages/{cultureCode}.xaml";
            
            try
            {
                
                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri(dictPath, UriKind.Relative);

                
                
                ResourceDictionary? oldDict = null;
                
                
                foreach (ResourceDictionary d in Application.Current.Resources.MergedDictionaries)
                {
                    if (d.Source != null && d.Source.OriginalString.Contains("Resources/Languages/"))
                    {
                        oldDict = d;
                        break;
                    }
                }

                if (oldDict != null)
                {
                    int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error switching language: {ex.Message}");
            }
        }
        public string GetString(string key)
        {
            try
            {
                if (Application.Current.Resources.Contains(key))
                {
                    return Application.Current.Resources[key] as string ?? key;
                }
                return key;
            }
            catch
            {
                return key;
            }
        }
    }
}
