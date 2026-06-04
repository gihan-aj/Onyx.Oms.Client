using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services
{
    public class DatabaseRestoreService
    {
        private readonly IToastService _toastService;
        private readonly ILogger<DatabaseRestoreService> _logger;

        private const string MasterConnectionString = "Server=(localdb)\\mssqllocaldb;Database=master;Trusted_Connection=True;TrustServerCertificate=True;";

        public DatabaseRestoreService(IToastService toastService, ILogger<DatabaseRestoreService> logger)
        {
            _toastService = toastService;
            _logger = logger;
        }


        public async Task<bool> RestoreSystemAsync(string idpBackupPath, string omsBackupPath)
        {
            if(!File.Exists(idpBackupPath) || !File.Exists(omsBackupPath))
            {
                _toastService.ShowError("Cannot Restore Backup","One or both backup files do not exist.");
                return false;
            }

            try
            {
                using var connection = new SqlConnection(MasterConnectionString);
                await connection.OpenAsync();

                // 1. Restore the Identity Provider
                await RestoreSingleDatabaseAsync(connection, "OnyxIdp", idpBackupPath);

                // 2. Restore the OMS
                await RestoreSingleDatabaseAsync(connection, "OnyxOms", omsBackupPath);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring databases");
                return false;
            }
        }

        private async Task RestoreSingleDatabaseAsync(SqlConnection connection, string databaseName, string backupFilePath)
        {
            // T-SQL to force-kick any lingering connections, restore the file, and unlock the database
            string sql = $@"
                -- Kick everyone out
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                
                -- Perform the restore (REPLACE forces it to overwrite)
                RESTORE DATABASE [{databaseName}] FROM DISK = '{backupFilePath}' WITH REPLACE;
                
                -- Let everyone back in
                ALTER DATABASE [{databaseName}] SET MULTI_USER;
            ";

            using var command = new SqlCommand(sql, connection);
            // CommandTimeout set to 0 (infinite) because massive backups can take several minutes to restore
            command.CommandTimeout = 0;
            await command.ExecuteNonQueryAsync();
        }
    }
}
