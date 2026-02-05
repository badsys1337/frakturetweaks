using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Frakture_Tweaks
{
    public partial class PowerView : UserControl
    {
        public PowerView()
        {
            InitializeComponent();
        }

        private async void ImportPowerPlanBtn_Click(object sender, RoutedEventArgs e)
        {
            string? tempPath = null;
            try
            {
                StatusText.Text = "Downloading Frakture Powerplan...";
                
                string url = "https://frakturetweaks.ru/Frakture%20Powerplan.pow";
                tempPath = Path.Combine(Path.GetTempPath(), "Frakture Powerplan.pow");

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    using (var fs = new FileStream(tempPath, FileMode.Create))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                }

                StatusText.Text = "Importing Frakture Powerplan...";

                if (!File.Exists(tempPath))
                {
                    StatusText.Text = $"Error: Downloaded file not found at {tempPath}";
                    return;
                }

                
                string importOutput = await Task.Run(() => RunPowerCfgCommand($"-import \"{tempPath}\""));
                
                
                
                string guidPattern = @"[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}";
                Match match = Regex.Match(importOutput, guidPattern);

                if (match.Success)
                {
                    string guid = match.Value;
                    
                    
                    await Task.Run(() => RunPowerCfgCommand($"-setactive {guid}"));
                    
                    StatusText.Text = "Successfully downloaded, imported and activated the power plan!";
                    StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
                }
                else
                {
                    StatusText.Text = "Failed to parse GUID from import output: " + importOutput;
                    StatusText.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error: " + ex.Message;
                StatusText.Foreground = System.Windows.Media.Brushes.Red;
            }
            finally
            {
                if (tempPath != null && File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
            }
        }

        private string RunPowerCfgCommand(string arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = "powercfg.exe";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }

        private void DisableHibernateBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RunPowerCfgCommand("/hibernate off");
                StatusText.Text = "Hibernate disabled successfully.";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            catch (Exception ex) { StatusText.Text = "Error: " + ex.Message; }
        }

        private void DisableSleepBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RunPowerCfgCommand("-x -standby-timeout-ac 0");
                RunPowerCfgCommand("-x -standby-timeout-dc 0");
                StatusText.Text = "Sleep timeout set to Never.";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            catch (Exception ex) { StatusText.Text = "Error: " + ex.Message; }
        }

        private void EnableUltimatePerfBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RunPowerCfgCommand("/duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");
                StatusText.Text = "Ultimate Performance scheme enabled.";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            catch (Exception ex) { StatusText.Text = "Error: " + ex.Message; }
        }

        private void OptimizePowerThrottlingBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling"))
                {
                    key.SetValue("PowerThrottlingOff", 1, RegistryValueKind.DWord);
                }
                StatusText.Text = "Power Throttling disabled in registry.";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            catch (Exception ex) { StatusText.Text = "Error: " + ex.Message; }
        }

        private void DisableCStatesBtn_Click(object sender, RoutedEventArgs e)
        {
            
            var result = MessageBox.Show(
                "⚠️ WARNING: Disabling C-States can cause boot failures on some motherboards (especially ASUS, MSI, Gigabyte).\n\n" +
                "This setting conflicts with UEFI power management (AI Overclocking, MultiCore Enhancement).\n\n" +
                "Only proceed if you understand the risks.\n\nContinue?",
                "Dangerous Operation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            
            if (result != MessageBoxResult.Yes) return;
            
            try
            {
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(@"SYSTEM\CurrentControlSet\Control\Processor"))
                {
                    key.SetValue("Cstates", 0, RegistryValueKind.DWord);
                }
                StatusText.Text = "C-States disabled in registry. Restart required.";
                StatusText.Foreground = System.Windows.Media.Brushes.Orange;
            }
            catch (Exception ex) { StatusText.Text = "Error: " + ex.Message; }
        }

        private void UnparkCoresBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RunPowerCfgCommand("/SETACVALUEINDEX SCHEME_CURRENT SUB_PROCESSOR CPMINCORES 100");
                RunPowerCfgCommand("/SETDCVALUEINDEX SCHEME_CURRENT SUB_PROCESSOR CPMINCORES 100");
                RunPowerCfgCommand("/SETACVALUEINDEX SCHEME_CURRENT SUB_PROCESSOR CPMAXCORES 100");
                RunPowerCfgCommand("/SETDCVALUEINDEX SCHEME_CURRENT SUB_PROCESSOR CPMAXCORES 100");
                RunPowerCfgCommand("/SETACTIVE SCHEME_CURRENT");
                
                StatusText.Text = "All CPU cores unparked.";
                StatusText.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            catch (Exception ex) { StatusText.Text = "Error: " + ex.Message; }
        }
    }
}

