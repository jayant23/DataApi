using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataApi.DataAccess
{

    public abstract class BaseAccess
    {
        protected IDbConnection connection;
        protected IDbCommand command;

        public BaseAccess(IDbConnection connection)
        {
            this.connection = connection;
            this.command = GetNewCommand(connection, "");
        }

        internal protected void OpenConnection(IDbConnection connection)
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        protected abstract IDbCommand GetNewCommand(IDbConnection connection, string cmdText);

        protected abstract IDataParameter GetNewParameter(string name, object value);

    }
}
