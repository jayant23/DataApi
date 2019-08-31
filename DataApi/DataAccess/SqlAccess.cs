using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace NoaDomAndDataAccess.DataAccess
{
    public abstract class SqlAccess : BaseAccess
    {
        public SqlAccess(string connectionString) :this(new SqlConnection(connectionString))
        {
        }

        public SqlAccess(IDbConnection connection) :base(connection)
        {
        }

        protected override IDbCommand GetNewCommand(IDbConnection connection, string cmdText)
        {
            return new SqlCommand(cmdText, (SqlConnection)connection);
        }

        protected override IDataParameter GetNewParameter(string name, object value)
        {
            return new SqlParameter(name, value);
        }

        public bool HasRows(IDataReader reader)
        {
            return ((SqlDataReader)reader).HasRows;
        }
    }
}
