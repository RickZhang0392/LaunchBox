using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchBox.Utils
{
    public class ProcessUtil
    {
        public static void StartProcess(string workingdirectory, string processName, string extension, bool noWindows = false, string parameters="")
        {
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = workingdirectory,
                FileName = processName,
                CreateNoWindow = noWindows,
                UseShellExecute = extension.ToLower().Equals(".exe")?false: true,
                Arguments = parameters
            };
            Process.Start(startInfo);
        }

        public static void KillProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                process.Kill();
            }
        }

        public static bool isProcessAlive(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes.Any())
            {
                return true;
            }
            return false;
        }
    }
}
