using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ElevatorSimulation.Data
{
  
    public class AccessDatabaseInitializer : IDatabaseInitializer
    {
        private const string CreateTableSql =
            "CREATE TABLE ElevatorLog (" +
            "LogId COUNTER PRIMARY KEY, " +
            "ElevatorId INTEGER NOT NULL, " +
            "CurrentFloor INTEGER NOT NULL, " +
            "DestinationFloor INTEGER, " +
            "EventDescription TEXT(255) NOT NULL, " +
            "EventTime DATETIME NOT NULL)";

        private readonly DatabaseConfig _config;
        private readonly AccessDatabase _database;

        public AccessDatabaseInitializer(DatabaseConfig config, AccessDatabase database)
        {
            _config = config;
            _database = database;
        }

        public void Initialize()
        {
            Exception lastError = null;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_config.FilePath));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                throw new DatabaseException("Cannot create the Data folder: " + ex.Message, ex);
            }

            // Try the newest Access Database Engine first, then the older one.
            foreach (string provider in DatabaseConfig.CandidateProviders)
            {
                _config.Provider = provider;
                try
                {
                    if (!File.Exists(_config.FilePath))
                    {
                        CreateDatabaseFile(provider);
                    }

                    if (!_database.TableExists("ElevatorLog"))
                    {
                        _database.ExecuteNonQuery(CreateTableSql);
                    }
                    return; // success
                }
                catch (Exception ex)
                {
                    lastError = ex;
                }
            }

            throw new DatabaseException(
                "Could not create or open the Access database. Install the 'Microsoft Access Database Engine' " +
                "(same 32/64-bit type as this program) - see README. Details: " +
                (lastError == null ? "unknown" : lastError.Message), lastError);
        }

        /// Creates an empty .accdb file through the late-bound ADOX COM component.
        private void CreateDatabaseFile(string provider)
        {
            Type catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
            if (catalogType == null)
            {
                throw new InvalidOperationException("The ADOX component is not available on this PC.");
            }

            object catalog = null;
            try
            {
                catalog = Activator.CreateInstance(catalogType);
                string connectionString = "Provider=" + provider + ";Data Source=" + _config.FilePath + ";";
                catalogType.InvokeMember("Create", BindingFlags.InvokeMethod, null, catalog,
                    new object[] { connectionString });
            }
            catch (TargetInvocationException ex)
            {
                Exception inner = ex.InnerException ?? ex;
                throw new InvalidOperationException(inner.Message, inner);
            }
            finally
            {
                if (catalog != null)
                {
                    Marshal.ReleaseComObject(catalog);
                }
            }
        }
    }
}
