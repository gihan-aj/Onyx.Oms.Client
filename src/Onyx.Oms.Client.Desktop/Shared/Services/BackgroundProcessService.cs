using CommunityToolkit.WinUI.Animations;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml.Documents;
using Onyx.Oms.Client.Desktop.Shared.Models.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services
{
    public class BackgroundProcessService
    {
        private Process? _idpProcess;
        private Process? _apiProcess;

        private readonly OnyxOmsApiOptions _omsApiOptions;
        private readonly AuthenticationOptions _authenticationOptions;

        public BackgroundProcessService(IOptions<OnyxOmsApiOptions> omsApiOptions, IOptions<AuthenticationOptions> authenticationOptions)
        {
            _omsApiOptions = omsApiOptions.Value;
            _authenticationOptions = authenticationOptions.Value;
        }

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

                _idpProcess = LaunchHiddenProcess(idpPath, "--urls \"http://localhost:54320\"");
                _apiProcess = LaunchHiddenProcess(apiPath, "--urls \"http://localhost:54321\"");

                Log.Information("Background services started successfully.");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed to start background services.");
            }
        }

        private Process? LaunchHiddenProcess(string executablePath, string args)
        {
            if (!File.Exists(executablePath)) return null;

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = args,
                WorkingDirectory = Path.GetDirectoryName(executablePath),
                UseShellExecute = true,   // Must be false to hide the window
                CreateNoWindow = false,     // Do not create a console window
                WindowStyle = ProcessWindowStyle.Normal
            };

            return Process.Start(startInfo);
        }

        public bool IsReady { get; private set; }

        public async Task WaitForApiToWakeUpAsync()
        {
            if (IsReady) return;

            Log.Information("Waiting for background APIs to become responsive...");

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(2);

            int maxRetries = 15;

            for(int i = 0;  i < maxRetries; i++)
            {
                try
                {
                    var apiPing = client.GetAsync(_omsApiOptions.BaseUrl + "/api/v1/subscription-plans");
                    var idpPing = client.GetAsync(_authenticationOptions.Authority + "/.well-known/openid-configuration");

                    var response = await Task.WhenAll(apiPing, idpPing);

                    IsReady = true;

                    Log.Information("Background APIs are online and responding!");
                    return;
                }
                catch (HttpRequestException)
                {
                    Log.Debug($"Services actively refused connection. Retrying... ({i + 1}/{maxRetries})");
                }
                catch(TaskCanceledException)
                {
                    Log.Debug($"Service ping timed out. Retrying... ({i + 1}/{maxRetries})");
                }
                catch(Exception ex)
                {
                    Log.Error(ex, $"Service ping failed. Retrying... ({i + 1}/{maxRetries})");
                }
                await Task.Delay(2000);
            }

            Log.Fatal("Background services failed to respond after 30 seconds.");
            throw new Exception("The background services took too long to start.");
        }

        public async Task StopBackendServicesAsync()
        {
            Log.Information("Politely asking background services to shut down and run backups...");

            var shutdownTasks = new List<Task>
            {
                SendShutdownSignalAsync(_omsApiOptions.BaseUrl), 
                SendShutdownSignalAsync(_authenticationOptions.Authority),
            };

            var timeoutTask = Task.Delay(2000);
            //Task.WaitAll(shutdownTasks.ToArray(), 2000);
            await Task.WhenAny(Task.WhenAll(shutdownTasks), timeoutTask); // Using Task.WhenAny prevents blocking if an API is unresponsive

            var processWaitTasks = new List<Task>();
            if (_idpProcess != null && !_idpProcess.HasExited)
                processWaitTasks.Add(_idpProcess.WaitForExitAsync());
            if(_apiProcess != null && !_apiProcess.HasExited)
                processWaitTasks.Add(_apiProcess.WaitForExitAsync());

            if (processWaitTasks.Any())
            {
                Log.Information("Waiting for background services to finish their tasks (like backups) and exit...");

                // Give them up to 60 seconds to finish massive backups
                var failSafeTimeout = Task.Delay(TimeSpan.FromSeconds(60));
                var completedTask = await Task.WhenAny(Task.WhenAll(processWaitTasks), failSafeTimeout);

                if (completedTask == failSafeTimeout)
                {
                    Log.Warning("Services took longer than 60 seconds to exit. Forcefully terminating.");
                    SafelyKillProcess(_idpProcess, "IdP");
                    SafelyKillProcess(_apiProcess, "API");
                }
                else
                {
                    Log.Information("Background services exited gracefully.");
                }
            }
        }

        private async Task SendShutdownSignalAsync(string baseUrl)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                await client.PostAsync($"{baseUrl}/api/system/shutdown", null);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, $"Failed to send graceful shutdown signal to {baseUrl}");
            }
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
