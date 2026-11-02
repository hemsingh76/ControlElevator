using System;
using System.IO;

namespace ElevatorSimulation.Data
{
   
    public class DatabaseConfig
    {
        /// Access Database Engine providers to try, newest first.
        public static readonly string[] CandidateProviders =
        {
            "Microsoft.ACE.OLEDB.16.0",
            "Microsoft.ACE.OLEDB.12.0"
        };

        public string FilePath { get; }

        /// Provider currently in use (chosen by the initializer)
        public string Provider { get; set; }

        public DatabaseConfig()
        {
            FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "ElevatorLog.accdb");
            Provider = CandidateProviders[0];
        }

        public string ConnectionString
        {
            get { return "Provider=" + Provider + ";Data Source=" + FilePath + ";Persist Security Info=False;"; }
        }
    }
}
