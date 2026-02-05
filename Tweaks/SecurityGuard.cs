using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Frakture_Tweaks
{
    public static class SecurityGuard
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);

        public static void StartProtection()
        {
            Thread thread = new Thread(MonitorThreats);
            thread.IsBackground = true;
            thread.Start();
        }

        private static void MonitorThreats()
        {
            while (true)
            {
                
                if (Debugger.IsAttached)
                {
                    KillApp("Debugger detected.");
                }

                
                bool isRemoteDebuggerPresent = false;
                CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isRemoteDebuggerPresent);
                if (isRemoteDebuggerPresent)
                {
                    KillApp("Remote Debugger detected.");
                }

                
                string[] badProcesses = { "dnspy", "ilspy", "wireshark", "fiddler", "httpdebugger", "cheatengine", "processhacker" };
                Process[] processList = Process.GetProcesses();
                foreach (Process p in processList)
                {
                    try
                    {
                        foreach (string bad in badProcesses)
                        {
                            if (p.ProcessName.ToLower().Contains(bad))
                            {
                                KillApp($"Malicious tool detected: {p.ProcessName}");
                            }
                        }
                    }
                    catch { }
                }

                Thread.Sleep(10000); 
            }
        }

        private static void KillApp(string reason)
        {
            MessageBox.Show($"Security Violation: {reason}\nThe application will now terminate.", "Security Alert", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(1);
        }
    }
}
