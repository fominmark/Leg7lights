using System.Diagnostics;
using System.IO;

namespace Leg7lights
{
    public class LegionLightingController
    {
        private readonly string _scriptPath;

        public LegionLightingController()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _scriptPath = Path.Combine(currentDirectory, "legion7-rgb-ps-main", "legion7-rgb.ps1");
            
            if (!File.Exists(_scriptPath))
            {
                throw new FileNotFoundException($"PowerShell script not found at: {_scriptPath}");
            }
        }

        public async Task SetColor(string group, string hexColor)
        {
            try
            {
                string arguments = $"-executionPolicy bypass -file \"{_scriptPath}\" -{group} {hexColor}";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    await process.WaitForExitAsync();
                    if (process.ExitCode != 0)
                    {
                        throw new Exception("PowerShell script execution failed");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute PowerShell script: {ex.Message}");
            }
        }
    }
} 