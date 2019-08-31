using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace NoaDomAndDataAccess.DataAccess
{
    public class PatientAccess : SqlAccess
    {
        public string constr;
        public SqlConnection con;

        public PatientAccess(string connectionString) : base(connectionString) { constr = connectionString; }

        public PatientAccess(IDbConnection connection) : base(connection) { }

        public void CreatePatient()
        {

        }

        
    }
}
