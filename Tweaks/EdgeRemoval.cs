using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Linq;

namespace Frakture_Tweaks
{
    public class EdgeRemoval
    {
        private LogWindow _logger;
        private bool _is64Bit;

        public EdgeRemoval(LogWindow logger)
        {
            _logger = logger;
            _is64Bit = Environment.Is64BitOperatingSystem;
        }

        public async Task PerformEdgeRemoval()
        {
            await Task.Run(() =>
            {
                try
                {
                    _logger.AddLog("Starting Microsoft Edge Removal (Enhanced)...");
                    _logger.AddLog("Note: This process may take a few minutes.");

                    
                    KillProcesses();

                    
                    ClearWin32UninstallBlocks();

                    
                    PrepareEdge();

                    
                    RemoveAppxPackages();

                    
                    RunSetupUninstall();

                    
                    CleanupMsiInstallers();

                    
                    CleanupEdgeUpdate();

                    
                    BlockEdgeUpdates();

                    
                    CleanupShortcuts();

                    
                    ForceDeleteFiles();

                    
                    RestartExplorer();

                    _logger.AddLog("Microsoft Edge Removal Completed!");
                }
                catch (Exception ex)
                {
                    _logger.AddLog($"Error during Edge removal: {ex.Message}");
                }
            });
        }

        private void KillProcesses()
        {
            _logger.AddLog("Killing Edge processes...");
            string[] processes = { "explorer", "Widgets", "widgetservice", "MicrosoftEdge", "chredge", "msedge", "edge", "msteams", "msfamily", "Clipchamp" };
            
            foreach (var procName in processes)
            {
                try
                {
                    foreach (var proc in Process.GetProcessesByName(procName))
                    {
                        proc.Kill();
                    }
                }
                catch { }
            }
        }

        private void ClearWin32UninstallBlocks()
        {
            _logger.AddLog("Clearing Win32 uninstall blocks...");
            string[] names = { "Microsoft Edge", "Microsoft Edge Update", "Microsoft EdgeWebView" };
            string[] hives = {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };

            foreach (var name in names)
            {
                foreach (var hive in hives)
                {
                    string keyPath = $@"{hive}\{name}";
                    try
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath, true))
                        {
                            if (key != null)
                            {
                                string[] valuesToRemove = { "NoRemove", "NoModify", "NoRepair" };
                                foreach (var val in valuesToRemove)
                                {
                                    try { key.DeleteValue(val); } catch { }
                                }

                                key.SetValue("ForceRemove", 1, RegistryValueKind.DWord);
                                key.SetValue("Delete", 1, RegistryValueKind.DWord);
                            }
                        }
                    }
                    catch { }
                }
            }
        }

        private void PrepareEdge()
        {
            _logger.AddLog("Preparing Edge for removal...");
            
            
            
            
            string[] commands = {
                @"reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\msedge.exe"" /f",
                @"reg delete ""HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\ie_to_edge_stub.exe"" /f",
                @"reg delete ""HKLM\SOFTWARE\Classes\microsoft-edge"" /f",
                @"reg delete ""HKLM\SOFTWARE\Classes\MSEdgeHTM"" /f"
            };

            foreach (var cmd in commands)
            {
                RunCommand(cmd);
            }
        }

        private void RemoveAppxPackages()
        {
            _logger.AddLog("Removing Appx packages (Enhanced)...");
            
            
            string script = @"
$remove_appx = @('Microsoft.MicrosoftEdge', 'Microsoft.MicrosoftEdge.Stable', 'Microsoft.MicrosoftEdge.Beta', 'Microsoft.MicrosoftEdge.Dev', 'Microsoft.MicrosoftEdge.Canary', 'Microsoft.Edge', 'Microsoft.Win32WebViewHost', 'Microsoft.WebExperience', 'Microsoft.GamingServices');
$provisioned = Get-AppxProvisionedPackage -Online;
$packages = Get-AppxPackage -AllUsers;

foreach ($appName in $remove_appx) {
    # Remove Provisioned Packages
    $pPackages = $provisioned | Where-Object { $_.PackageName -like ""*$appName*"" };
    foreach ($p in $pPackages) {
        Write-Output ""Removing provisioned: $($p.PackageName)""
        Remove-AppxProvisionedPackage -Online -PackageName $p.PackageName -ErrorAction SilentlyContinue
    }

    # Remove Installed Packages for All Users
    $iPackages = $packages | Where-Object { $_.Name -like ""*$appName*"" };
    foreach ($p in $iPackages) {
        Write-Output ""Removing installed: $($p.Name)""
        Remove-AppxPackage -Package $p.PackageFullName -AllUsers -ErrorAction SilentlyContinue
    }
}

# Force removal by package family name (fallback)
$families = @('Microsoft.MicrosoftEdge_8wekyb3d8bbwe', 'Microsoft.MicrosoftEdge.Stable_8wekyb3d8bbwe', 'Microsoft.Win32WebViewHost_cw5n1h2txyewy');
foreach ($family in $families) {
     Get-AppxPackage -PackageTypeFilter Main,Bundle,Resource,Optional,Framework | Where-Object {$_.PackageFamilyName -eq $family} | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue
}
";
            RunPowerShell(script);
        }

        private void RunSetupUninstall()
        {
            _logger.AddLog("Running Edge Setup Uninstall (Forceful)...");
            
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            
            List<string> paths = new List<string>();
            paths.Add(programFiles);
            if (_is64Bit) paths.Add(programFilesX86);

            foreach (var folder in paths)
            {
                try
                {
                    string edgeFolder = Path.Combine(folder, "Microsoft", "Edge");
                    if (Directory.Exists(edgeFolder))
                    {
                        var setups = Directory.GetFiles(edgeFolder, "setup.exe", SearchOption.AllDirectories);
                        foreach (var setup in setups)
                        {
                            
                            string[] argsList = {
                                "--uninstall --msedge --system-level --verbose-logging --force-uninstall",
                                "--uninstall --msedge --user-level --verbose-logging --force-uninstall",
                                "--uninstall --system-level --force-uninstall", 
                                "--uninstall --force-uninstall" 
                            };

                            foreach (var args in argsList)
                            {
                                string currentArgs = args;
                                if (setup.Contains("EdgeWebView"))
                                {
                                    currentArgs = currentArgs.Replace("--msedge", "--msedgewebview");
                                }
                                
                                _logger.AddLog($"Executing: {setup} {currentArgs}");
                                RunProcess(setup, currentArgs);
                            }
                        }
                    }
                }
                catch { }
            }
        }

        private void ForceDeleteFiles()
        {
             _logger.AddLog("Force deleting Edge files...");
             
             var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
             var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
             
             List<string> paths = new List<string> { 
                 Path.Combine(programFiles, "Microsoft", "Edge"),
                 Path.Combine(programFiles, "Microsoft", "EdgeUpdate"),
                 Path.Combine(programFiles, "Microsoft", "EdgeCore"),
                 Path.Combine(programFiles, "Microsoft", "EdgeWebView")
             };

             if (_is64Bit)
             {
                 paths.Add(Path.Combine(programFilesX86, "Microsoft", "Edge"));
                 paths.Add(Path.Combine(programFilesX86, "Microsoft", "EdgeUpdate"));
                 paths.Add(Path.Combine(programFilesX86, "Microsoft", "EdgeCore"));
             }

             
             string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
             paths.Add(Path.Combine(localAppData, "Microsoft", "Edge"));

             foreach (var path in paths)
             {
                 if (Directory.Exists(path))
                 {
                     _logger.AddLog($"Deleting folder: {path}");
                     
                     
                     RunCommand($"takeown /f \"{path}\" /r /d y");
                     RunCommand($"icacls \"{path}\" /grant administrators:F /t");
                     
                     try 
                     { 
                         Directory.Delete(path, true); 
                     } 
                     catch (Exception ex) 
                     {
                         _logger.AddLog($"Failed to delete {path}: {ex.Message}. Trying forced removal via cmd.");
                         RunCommand($"rd /s /q \"{path}\"");
                     }
                 }
             }
        }

        private void CleanupMsiInstallers()
        {
            _logger.AddLog("Cleaning up MSI Installers...");
            
            
            
            string script = @"
get-itemproperty 'HKLM:\SOFTWARE\Classes\Installer\Products\*' 'ProductName' -ea 0 | where {$_.ProductName -like '*Microsoft Edge*'} | foreach { 
  $prod = ($_.PSChildName -split '(.{8})(.{4})(.{4})(.{4})' -join '-').trim('-');
  start-process msiexec.exe -argumentlist ""/X$prod /qn"" -wait -windowstyle hidden;
}";
            RunPowerShell(script);
        }

        private void CleanupEdgeUpdate()
        {
            _logger.AddLog("Cleaning up Edge Update...");
            
            
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            List<string> folders = new List<string> { programFiles };
            if (_is64Bit) folders.Add(programFilesX86);

            foreach (var folder in folders)
            {
                string updateFolder = Path.Combine(folder, "Microsoft", "EdgeUpdate");
                if (Directory.Exists(updateFolder))
                {
                    try
                    {
                        var updates = Directory.GetFiles(updateFolder, "MicrosoftEdgeUpdate.exe", SearchOption.AllDirectories);
                        foreach (var exe in updates)
                        {
                            RunProcess(exe, "/unregsvc");
                            RunProcess(exe, "/uninstall");
                        }
                    }
                    catch { }
                }
            }

            string[] keys = {
                @"HKLM\SOFTWARE\Microsoft\EdgeUpdate",
                @"HKLM\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate"
            };

            foreach (var key in keys)
            {
                RunCommand($"reg delete \"{key}\" /f");
            }
            
            RunCommand("schtasks /delete /tn MicrosoftEdgeUpdate* /f");
            
            
            try { Directory.Delete(Path.Combine(programFiles, "Microsoft", "Temp"), true); } catch { }
        }

        private void BlockEdgeUpdates()
        {
            _logger.AddLog("Blocking Edge Updates...");
            
            string[] keys = {
                @"HKLM\SOFTWARE\Microsoft\EdgeUpdate",
                @"HKLM\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate",
                @"HKLM\SOFTWARE\Policies\Microsoft\EdgeUpdate"
            };

            foreach (var key in keys)
            {
                RunCommand($"reg add \"{key}\" /v DoNotUpdateToEdgeWithChromium /t REG_DWORD /d 1 /f");
                RunCommand($"reg add \"{key}\" /v InstallDefault /t REG_DWORD /d 0 /f");
            }
        }

        private void CleanupShortcuts()
        {
            _logger.AddLog("Cleaning up shortcuts...");
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string[] shortcuts = {
                Path.Combine(appData, @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar\Tombstones\Microsoft Edge.lnk"),
                Path.Combine(appData, @"Microsoft\Internet Explorer\Quick Launch\Microsoft Edge.lnk"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory), "Microsoft Edge.lnk")
            };

            foreach (var s in shortcuts)
            {
                if (File.Exists(s))
                {
                    try { File.Delete(s); } catch { }
                }
            }
        }

        private void RestartExplorer()
        {
            _logger.AddLog("Restarting Explorer...");
            try
            {
                Process.Start("explorer.exe");
            }
            catch { }
        }

        private void RunCommand(string command)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/c " + command);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi)?.WaitForExit();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error running command {command}: {ex.Message}");
            }
        }

        private void RunProcess(string fileName, string args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo(fileName, args);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi)?.WaitForExit();
            }
            catch { }
        }

        private void RunPowerShell(string script)
        {
            try
            {
                
                string encodedScript = Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(script));
                ProcessStartInfo psi = new ProcessStartInfo("powershell.exe", $"-NoProfile -EncodedCommand {encodedScript}");
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process.Start(psi)?.WaitForExit();
            }
            catch (Exception ex)
            {
                _logger.AddLog($"PowerShell Error: {ex.Message}");
            }
        }
    }
}

