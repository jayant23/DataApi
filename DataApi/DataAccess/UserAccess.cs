using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using NoaDomAndDataAccess.Dom;

namespace NoaDomAndDataAccess.DataAccess
{
    public class UserAccess : SqlAccess
    {
        public string constr;
        public SqlConnection con;

        public UserAccess(string connectionString) : base(connectionString) { constr = connectionString; }

        public UserAccess(IDbConnection connection) : base(connection) { }

        public int CreateUser(User user, string operatorName)
        {
            if (IsUserExists(user.UserName))
            {
                user.UserID = GetUserIDByUsername(user.UserName);
                return UpdateUser(user, this.command, operatorName);
            }
            else
            {
                return InsertUser(user, this.command, operatorName);
            }
            
        }

        internal int InsertUser(User user, IDbCommand command, string operatorName)
        {
            int retval = 0;
            string commandText = "INSERT INTO _User (FirstName, LastName, Username, ClinicID, Mobile, EmailAddress, RegistrationDate, Operator, OperationType) " +
                " VALUES(@FirstName, @LastName, @UserName, @ClinicID, @Mobile, @EmailAddress, GETDATE(),@Operator,@OperationType) SELECT @@IDENTITY";
            OpenConnection(connection);
            command.CommandText = commandText;
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@FirstName", user.FirstName));
            command.Parameters.Add(GetNewParameter("@LastName", user.LastName));
            command.Parameters.Add(GetNewParameter("@UserName", user.UserName));
            command.Parameters.Add(GetNewParameter("@Operator", operatorName));
            command.Parameters.Add(GetNewParameter("@OperationType", "INSERT"));
            command.Parameters.Add(GetNewParameter("@ClinicID", user.ClinicID));
            command.Parameters.Add(GetNewParameter("@Mobile", user.MobileNumber));
            command.Parameters.Add(GetNewParameter("@EmailAddress", user.UserEmail));

            object identity = command.ExecuteScalar();
            retval = Convert.ToInt32(identity);
            return retval;
        }

        internal int UpdateUser(User user, IDbCommand command, string operatorName)
        {
            int retval = 0;

            string sQuery = "UPDATE _User SET FirstName=@FirstName, LastName=@LastName, ClinicID=@ClinicID, Mobile=@Mobile, "+
                "EmailAddress=@EmailAddress, Operator=@Operator , OperationType=@OperationType where UserID = @UserID";

            OpenConnection(connection);
            command.CommandText = sQuery;
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@FirstName", user.FirstName));
            command.Parameters.Add(GetNewParameter("@LastName", user.LastName));
            command.Parameters.Add(GetNewParameter("@Operator", operatorName));
            command.Parameters.Add(GetNewParameter("@OperationType", "UPDATE"));
            command.Parameters.Add(GetNewParameter("@ClinicID", user.ClinicID));
            command.Parameters.Add(GetNewParameter("@Mobile", user.MobileNumber));
            command.Parameters.Add(GetNewParameter("@UserID", user.UserID));
            command.Parameters.Add(GetNewParameter("@EmailAddress", user.UserEmail));

            object identity = command.ExecuteNonQuery();
            retval = Convert.ToInt32(identity);

            return retval;
        }

        internal bool IsUserExists(string userName)
        {
            bool exists = false;
            int userid = GetUserIDByUsername(userName);

            exists = userid > 0;
            return exists;
        }

        public int GetUserIDByUsername(string userName)
        {
            command.CommandText = "select UserID from _User where UserName = @userName";
            OpenConnection(connection);
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@userName", userName));

            try
            {
                object retVal = command.ExecuteScalar();
                if (retVal != null)
                {
                    return (int)retVal;
                }

                return -1;
            }
            finally
            {
                connection.Close();
            }
        }

        public string GetUserNameByID(int userID)
        {
            command.CommandText = "select UserName from _User where UserID = @userID";
            OpenConnection(connection);
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@userID", userID));

            try
            {
                object retVal = command.ExecuteScalar();
                if (retVal != null)
                {
                    return (string)retVal;
                }

                return null;
            }
            finally
            {
                connection.Close();
            }
        }

        public string GetFullNameByUserID(int userID)
        {
            string sName = string.Empty;

            string sQuery = "SELECT  " +
                "(CASE " +
                    "WHEN MiddleName IS NULL THEN FirstName + ' ' + LastName " +
                    "ELSE FirstName + ' ' + MiddleName + ' ' + LastName " +
                "END) as FullName " +
                "FROM _User  " + 
                "WHERE UserID = @UserID";

            command.CommandText = sQuery;
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@UserID", userID));

            try
            {
                OpenConnection(connection);
                object retVal = command.ExecuteScalar();
                if (retVal != null)
                {
                    sName = (string)retVal;
                }
            }
            catch(Exception ex)
            {
                sName = null;
                Utils.WriteToLog("Exception While getting fullname for userID = " + userID);
            }
            finally
            {
                connection.Close();
            }

            return sName;
        }

        public string GetFullNameByUsername(string username)
        {
            string sName = string.Empty;

            string sQuery = "SELECT  " +
                "(CASE " +
                    "WHEN MiddleName IS NULL THEN FirstName + ' ' + LastName " +
                    "ELSE FirstName + ' ' + MiddleName + ' ' + LastName " +
                "END) as FullName " +
                "FROM _User  " +
                "WHERE UserName = @UserName";

            command.CommandText = sQuery;
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@UserName", username));

            try
            {
                OpenConnection(connection);
                object retVal = command.ExecuteScalar();
                if (retVal != null)
                {
                    sName = (string)retVal;
                }
            }
            catch (Exception ex)
            {
                sName = null;
                Utils.WriteToLog("Exception While getting fullname for username = " + username);
            }
            finally
            {
                connection.Close();
            }

            return sName;
        }

        public User GetUserByUserName(string username)
        {
            User user = new User();
            if (Roles.IsUserInRole(username, SecurityUserAccess.RoleAdministrator))
            {
                command.CommandText = @"select TOP 1 *, 'Admin Clinic' as Name from _User U where UserName = @Username";
            }
            {
                command.CommandText = @"select TOP 1 * from _User U
							    INNER JOIN _clinic C ON C.ClinicID=U.ClinicID  where UserName = @Username";
            }
           
            OpenConnection(connection);
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@Username", username));

            try
            {
                SqlDataReader reader = (SqlDataReader)command.ExecuteReader();
                while (reader.Read())
                {
                    user.UserID = int.Parse(reader["UserID"].ToString());
                    user.FirstName = reader["FirstName"].ToString();
                    user.LastName = reader["LastName"].ToString();
                    user.UserAccessToken = reader["UserAccessToken"].ToString();
                    user.UserName = reader["UserName"].ToString();
                    user.UserEmail = reader["EmailAddress"].ToString();
                    user.MobileNumber = reader["Mobile"].ToString();
                    user.FullName = user.FirstName + " " + user.LastName;
                    user.ClinicName = reader["Name"].ToString();
                    user.ClinicID = int.Parse(reader["ClinicID"].ToString());
                }

            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception Occurred at GetUserByAccessToken():  " + ex.Message.ToString());
            }
            finally
            {
                connection.Close();
            }

            return user;
        }
        public Clinic GetClinicUserByUserName(string username)
        {
            Clinic clinic = new Clinic();
            if (Roles.IsUserInRole(username, SecurityUserAccess.RoleAdministrator))
            {
                command.CommandText = @"select TOP 1 *, 'Admin Clinic' as Name from _User U where UserName = @Username";
            }
            {
                command.CommandText = @"select TOP 1 * from _User U
							    INNER JOIN _clinic C ON C.ClinicID=U.ClinicID  where UserName = @Username";
            }

            OpenConnection(connection);
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@Username", username));

            try
            {
                SqlDataReader reader = (SqlDataReader)command.ExecuteReader();
                while (reader.Read())
                {
                    
                    clinic.ContactEmail = reader["ContactEmail"].ToString();
                }

            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception Occurred at GetUserByAccessToken():  " + ex.Message.ToString());
            }
            finally
            {
                connection.Close();
            }

            return clinic;
        }
        public User GetUserByAccessToken(string accessToken)
        {
            User u = new User();

            command.CommandText = "select * from _User where UserAccessToken = @UserAccessToken";
            OpenConnection(connection);
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@UserAccessToken", accessToken));

            try
            {
                SqlDataReader reader = (SqlDataReader)command.ExecuteReader();
                while (reader.Read())
                {
                    u.FirstName = reader["FirstName"].ToString();
                    u.LastName = reader["LastName"].ToString();
                    u.UserAccessToken = reader["UserAccessToken"].ToString();
                    u.UserName = reader["UserName"].ToString();
                }

            }
            catch(Exception ex)
            {
                Utils.WriteToLog("Exception Occurred at GetUserByAccessToken():  " + ex.Message.ToString());
            }
            finally
            {
                connection.Close();
            }

            return u;
        }

        private void CreateSecurityUser(string userName, string password, string userRole)
        {
            SecurityUserAccess securityUserAccess = new SecurityUserAccess();
            securityUserAccess.CreateSecurityUser(userName, password, userRole);
        }

        public void CreateSecurityUserForPatient(string userName, string password)
        {
            SecurityUserAccess securityUserAccess = new SecurityUserAccess();
            securityUserAccess.CreateSecurityUserForExistingPatients(userName, password, SecurityUserAccess.RolePatient);
        }

        private void ChangeSecurityUserPassword(string userName, string newPassword)
        {
            SecurityUserAccess securityUserAccess = new SecurityUserAccess();
            securityUserAccess.ChangePassword(userName, newPassword);
        }

        private void UpdateSecurityUser(string oldUserName, string newUserName, string password, string userRole)
        {
            SecurityUserAccess securityUserAccess = new SecurityUserAccess();
            securityUserAccess.UpdateUser(oldUserName, newUserName, password, userRole);
        }

        public DataTable FillUserDatabyUserID(int userID)
        {
            string sName = string.Empty;
            DataTable dtable = new DataTable();

            string sQuery = "SELECT  UserName, FirstName, LastName from _user where userID="+userID+"";
            command.CommandText = sQuery;
            command.Parameters.Clear();

            try
            {
                OpenConnection(connection);
                using (SqlDataAdapter dAdapter = new SqlDataAdapter(sQuery, (SqlConnection)connection))
                {
                    //command.Parameters.Add(GetNewParameter("@userID", userID));

                    dAdapter.Fill(dtable);
                }
            }
            finally
            {
                connection.Close();
            }


            return dtable;
        }
    }
}
