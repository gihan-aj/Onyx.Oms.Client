using System;
using System.Diagnostics;
using System.IO;

namespace Onyx.Oms.Client.Desktop.Shared.Services
{
    public class BackgroundProcessService : IDisposable
    {
        private Process? _idpProcess;
        private Process? _apiProcess;

        public void StartBackendServices()
        {
            var basePath = AppContext.BaseDirectory;

            var idpPath = Path.Combine(basePath, "IdP", "Onyx.IdP.Web.exe");
            var apiPath = Path.Combine(basePath, "API", "Onyx.Oms.Web.exe");

            _idpProcess = LaunchProcess(idpPath);
            _apiProcess = LaunchProcess(apiPath);
        }

        private Process? LaunchProcess(string executablePath)
        {
            if (!File.Exists(executablePath))
                return null;

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = Path.GetDirectoryName(executablePath),
                UseShellExecute = false, // Required to hide the window
                CreateNoWindow = true, // Hide the console window completely
            };

            return Process.Start(startInfo);
        }

        public void Dispose()
        {
            // Endure background processes are killed when the App closes
            if (_idpProcess != null && !_idpProcess.HasExited)
            {
                _idpProcess.Kill();
                _idpProcess.Dispose();
            }

            if (_apiProcess != null && !_apiProcess.HasExited)
            {
                _apiProcess.Kill();
                _apiProcess.Dispose();
            }
        }
    }
}
