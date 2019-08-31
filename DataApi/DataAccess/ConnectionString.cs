using DataApi.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace DataApi.DataAccess
{
    public class ConnectionString
    {
        public ConnectionString()
        {
        }

        public static string Get()
        {
            string connectionString = WebConfigurationManager.ConnectionStrings[Settings.ConnectionStringName].ConnectionString;
            return connectionString;
        }

        public static string GetSecurityConnection()
        {
            string connectionString = WebConfigurationManager.ConnectionStrings[Settings.SecurityDBConnString].ConnectionString;
            return connectionString;
        }

        public static string GetFileStreamConnection()
        {
            string connectionString = WebConfigurationManager.ConnectionStrings[Settings.OCTFilesDBConnString].ConnectionString;
            return connectionString;
        }
    }
}
