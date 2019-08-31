using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoaDomAndDataAccess.Dom;

namespace NoaDomAndDataAccess.DataAccess
{
    public class EmailNotificationAccess: SqlAccess
    {
        public string constr;
        public SqlConnection con;

        public EmailNotificationAccess(string connectionString) : base(connectionString) { constr = connectionString; }

        public EmailNotificationAccess(IDbConnection connection) : base(connection) { }

        public List<EmailNotification> GetDataForEmailNotification()
        {
            List<EmailNotification> emailsData = new List<EmailNotification>();

            string sQuery = "SELECT e.NotificationID, e.FileID, u.FirstName + ' ' + u.LastName as UserFullName, u.EmailAddress "+
                "FROM _NoaEmailNotifications as e " +
                "INNER JOIN _UserStudyFiles as usf on e.FileID = usf.FileID " +
                "INNER JOIN _User as u on usf.UserID = u.UserID " +
                "WHERE isEmailSent = 0";

            try
            {
                OpenConnection(connection);

                command.CommandText = sQuery;
                command.Parameters.Clear();

                IDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    EmailNotification notification = new EmailNotification();

                    notification.NotificationID = Convert.ToInt32(reader["NotificationID"]);
                    notification.FileID = Convert.ToInt32(reader["FileID"].ToString());
                    notification.UserFullName = reader["UserFullName"].ToString();
                    notification.EmailAddress = reader["EmailAddress"].ToString();

                    emailsData.Add(notification);
                }

            }
            catch(Exception ex)
            {

            }
            finally
            {
                connection.Close();
            }

            return emailsData;
        }

        public int InsertEmailNotification(int fileID)
        {
            int retVal = -1;

            string sQuery = "INSERT INTO _NoaEmailNotifications(FileID, IsEmailSent) "+
                "VALUES(@FileID, @IsEmailSent) SELECT @@IDENTITY";
            try
            {
                command.CommandText = sQuery;
                OpenConnection(connection);
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@FileID", fileID));
                command.Parameters.Add(GetNewParameter("@IsEmailSent", 0));

                object identity = command.ExecuteScalar();
                if (identity != null)
                {
                    retVal = (int)identity;
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                connection.Close();
            }

            return retVal;
        }

        public int UpdateNotificationStatus(int notificationID)
        {
            int retVal = 0;

            string sQuery = "UPDATE _NoaEmailNotifications SET IsEmailSent=1, EmailSentOn = GETDATE() "+
                "WHERE NotificationID = @NotificationID";
            try
            {
                command.CommandText = sQuery;
                OpenConnection(connection);
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@NotificationID", notificationID));

                object identity = command.ExecuteScalar();
                if (identity != null)
                {
                    retVal = (int)identity;
                }
            }
            catch(Exception ex)
            {

            }
            finally
            {
                connection.Close();
            }

            return retVal;
        }
    }
}
