using Serilog;
using System;
using System.Diagnostics;
using System.IO;

namespace Onyx.Oms.Client.Desktop.Shared.Services
{
    public class BackgroundProcessService
    {
        private Process? _idpProcess;
        private Process? _apiProcess;

        public void StartBackendServices()
        {
            try
            {
                Log.Information("Starting background services from MSIX payload...");

                // In a packaged app, BaseDirectory is the root of the installed package
                string basePath = AppDomain.CurrentDomain.BaseDirectory;

                // Build the paths to the executables inside your BackendServices folder
                string idpPath = Path.Combine(basePath, "BackendServices", "IdP", "Onyx.IdP.Web.exe");
                string apiPath = Path.Combine(basePath, "BackendServices", "API", "Onyx.Oms.Web.exe");

                if (!File.Exists(idpPath)) Log.Error($"IdP executable not found at: {idpPath}");
                if (!File.Exists(apiPath)) Log.Error($"API executable not found at: {apiPath}");

                _idpProcess = LaunchHiddenProcess(idpPath);
                _apiProcess = LaunchHiddenProcess(apiPath);

                Log.Information("Background services started successfully.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to start background services.");
            }
        }

        private Process? LaunchHiddenProcess(string executablePath)
        {
            if (!File.Exists(executablePath)) return null;

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = false,   // Must be false to hide the window
                CreateNoWindow = true,     // Do not create a console window
                WindowStyle = ProcessWindowStyle.Hidden
            };

            return Process.Start(startInfo);
        }

        public void StopBackendServices()
        {
            Log.Information("Shutting down background services...");

            SafelyKillProcess(_idpProcess, "IdP");
            SafelyKillProcess(_apiProcess, "API");
        }

        private void SafelyKillProcess(Process? process, string name)
        {
            if (process != null && !process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true); // Ensures child processes die too
                    process.Dispose();
                    Log.Information($"{name} process terminated successfully.");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error terminating {name} process.");
                }
            }
        }
    }
}
