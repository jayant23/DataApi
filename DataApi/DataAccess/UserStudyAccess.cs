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
    public class UserStudyAccess : SqlAccess
    {
        public string constr;
        public SqlConnection con;

        public UserStudyAccess(string connectionString) : base(connectionString) { constr = connectionString; }

        public UserStudyAccess(IDbConnection connection) : base(connection) { }

        public string AddStudyForUser(int userId, string studyName)
        {
            string sMsg = string.Empty;
            bool isAlreadyExists = IsStudyAlreadyExistsForUser(userId, studyName);

            string sQuery = "INSERT INTO _UserStudies(UserID, StudyName, OperationDate, OperationType) VALUES(@UserID, @StudyName, GETDATE(), 'INSERT') SELECT @@IDENTITY";

            if(!isAlreadyExists)
            {
                try
                {
                    command.CommandText = sQuery;
                    OpenConnection(connection);
                    command.Parameters.Clear();
                    command.Parameters.Add(GetNewParameter("@UserID", userId));
                    command.Parameters.Add(GetNewParameter("@StudyName", studyName));

                    object identity = command.ExecuteScalar();
                    int retVal = Convert.ToInt32(identity);

                    if (retVal > 0)
                    {
                        sMsg = "Study for user created successfully!";
                    }
                }
                catch(Exception ex)
                {
                    sMsg = ex.Message.ToString();
                }
                finally
                {
                    connection.Close();
                }
            }
            else
            {
                sMsg = "Study with same name already exists.";
            }

            return sMsg;
        }

        private bool IsStudyAlreadyExistsForUser(int userId, string studyName)
        {
            bool isExists = false;

            string sQuery = "SELECT COUNT(1) FROM _UserStudies WHERE UserID = @UserID AND StudyName = @StudyName";

            command.CommandText = sQuery;
            OpenConnection(connection);
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@UserID", userId));
            command.Parameters.Add(GetNewParameter("@StudyName", studyName));

            try
            {
                int count = (int)command.ExecuteScalar();
                if(count > 0)
                {
                    isExists = true;
                }
                
            }
            finally
            {
                connection.Close();
            }

            return isExists;
        }

        public List<UserStudy> GetAllStudiesForUser(int userID)
        {
            List<UserStudy> allStudies = new List<UserStudy>();

            string sQuery = "SELECT StudyID, StudyName FROM _UserStudies WHERE UserID = @UserID ORDER BY OperationDate ASC";

            command.CommandText = sQuery;
            OpenConnection(connection);
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@UserID", userID));

            try
            {
                using (SqlDataReader reader = (SqlDataReader)command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserStudy study = new UserStudy();

                        study.UserID = userID;
                        study.StudyID = Convert.ToInt32(reader["StudyID"].ToString());
                        study.StudyName = reader["StudyName"].ToString();

                        allStudies.Add(study);
                    }
                }
            }
            finally
            {
                connection.Close();
            }

            return allStudies;
        }
        public UserStudy GetStudybyStudyID(int studyID)
        {
            UserStudy study = new UserStudy();


            string sQuery = @"SELECT distinct  S.StudyID,S.StudyName,C.Name FROM _Study S
                                INNER JOIN _ClinicStudy CS ON CS.StudyID=S.StudyID
                                INNER JOIN _Clinic C on CS.ClinicID=C.ClinicID
                            where S.StudyID=@studyID ORDER BY S.StudyID ASC";

            command.CommandText = sQuery;
            OpenConnection(connection);
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@studyID", studyID));

            try
            {
                using (SqlDataReader reader = (SqlDataReader)command.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        study.StudyID = studyID;
                        study.StudyID = Convert.ToInt32(reader["StudyID"].ToString());
                        study.StudyName = reader["StudyName"].ToString();
                        study.Name = reader["Name"].ToString();//ClinicName                    }
                    }
                }
            }
            finally
            {
                connection.Close();
            }

            return study;
        }

        public List<UserStudy> GetDefaultStudyForUser(int userID)
        {
            List<UserStudy> allStudies = new List<UserStudy>();

            string sQuery = @"SELECT  s.StudyID, s.StudyName FROM _Study S
							INNER JOIN _ClinicStudy CS on CS.StudyID=S.StudyID 
                            INNER JOIN _User CU ON CU.ClinicID = CS.ClinicID
                            where CU.UserID=@UserID ORDER BY S.OperationDate ASC";

            command.CommandText = sQuery;
            OpenConnection(connection);
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@UserID", userID));

            try
            {
                using (SqlDataReader reader = (SqlDataReader)command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserStudy study = new UserStudy();

                        study.UserID = userID;
                        study.StudyID = Convert.ToInt32(reader["StudyID"].ToString());
                        study.StudyName = reader["StudyName"].ToString();

                        allStudies.Add(study);
                    }
                }
            }
            finally
            {
                connection.Close();
            }

            return allStudies;
        }

        public List<UserFileDom> GetUserList()
        {
            List<UserFileDom> allUsers = new List<UserFileDom>();

            string sQuery = @"SELECT distinct UserID,FirstName,MiddleName,LastName, UserName,EmailAddress,U.ClinicID, C.Name,S.StudyID ,S.StudyName  FROM _User U
							INNER JOIN _clinic C on U.ClinicID=U.ClinicID
							INNER JOIN _ClinicStudy CS ON C.ClinicID=CS.ClinicID
							INNER JOIN _Study S ON S.StudyID=CS.StudyID ORDER BY UserID ASC";

            command.CommandText = sQuery;
            OpenConnection(connection);
            command.Parameters.Clear();
            try
            {
                using (SqlDataReader reader = (SqlDataReader)command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserFileDom _userObj = new UserFileDom();

                        _userObj.UserID = Convert.ToInt32(reader["UserID"].ToString());
                        _userObj.FirstName =reader["FirstName"].ToString();
                        _userObj.MiddleName = reader["MiddleName"].ToString();
                        _userObj.LastName = reader["LastName"].ToString();
                        _userObj.UserEmail = reader["EmailAddress"].ToString();
                        _userObj.UserName = reader["UserName"].ToString();
                        _userObj.ClinicID = Convert.ToInt32(reader["ClinicID"].ToString());
                        _userObj.StudyID = Convert.ToInt32(reader["StudyID"].ToString());
                        _userObj.StudyName = reader["StudyName"].ToString();
                        _userObj.ClinicName = reader["Name"].ToString();

                        allUsers.Add(_userObj);
                    }
                }
            }
            finally
            {
                connection.Close();
            }

            return allUsers;
        }

        public int EditUser(User  userinfo)
        {
            int nRetVal = -1;
            IDbTransaction transaction = null;
            try
            {
                OpenConnection(connection);
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                string sQuery = "UPDATE _User SET FirstName=@FirstName,MiddleName=@MiddleName,LastName=@LastName WHERE UserID=@UserID";
                command.CommandText = sQuery;
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@FirstName", userinfo.FirstName));
                command.Parameters.Add(GetNewParameter("@MiddleName", userinfo.MiddleName));
                command.Parameters.Add(GetNewParameter("@LastName", userinfo.LastName));
                //command.Parameters.Add(GetNewParameter("@EmailAddress", userinfo.EmailAddress));

                command.ExecuteNonQuery();
                transaction.Commit();
                nRetVal = 1;
            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception While Updating user: " + ex.Message.ToString());

                transaction.Rollback();
                nRetVal = 0;
            }
            finally
            {
                connection.Close();
            }
            return nRetVal;
        }

        public int EditStudy(UserStudy studyinfo)
        {
            int nRetVal = -1;
            IDbTransaction transaction = null;
            try
            {
                OpenConnection(connection);
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                string sQuery = "UPDATE _Study SET StudyName=@StudyName WHERE StudyID=@StudyID";
                command.CommandText = sQuery;
                command.Parameters.Clear();
                command.Parameters.Add(GetNewParameter("@StudyName", studyinfo.StudyName));
                command.Parameters.Add(GetNewParameter("@StudyID", studyinfo.StudyID));
                command.ExecuteNonQuery();
                transaction.Commit();
                nRetVal = 1;
            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception While Updating user: " + ex.Message.ToString());

                transaction.Rollback();
                nRetVal = 0;
            }
            finally
            {
                connection.Close();
            }
            return nRetVal;
        }
        public int DeleteUserbyuserID(int userID)
        {
            int nRetVal = -1;
            IDbTransaction transaction = null;
            try
            {
                OpenConnection(connection);
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                string sQuery = "delete _user where userID=@userID";
                command.CommandText = sQuery;
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@userID", userID));

                command.ExecuteNonQuery();
                transaction.Commit();
                nRetVal = 1;
            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception Deleting user : " + ex.Message.ToString());

                transaction.Rollback();
                nRetVal = 0;
            }
            finally
            {
                connection.Close();
            }
            return nRetVal;
        }
        public int DeleteStudybystudyID(int studyID)
        {
            int nRetVal = -1;
            IDbTransaction transaction = null;
            try
            {
                OpenConnection(connection);
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                string sQuery = "delete _Study where studyID=@studyID";
                command.CommandText = sQuery;
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@studyID", studyID));

                command.ExecuteNonQuery();
                transaction.Commit();
                nRetVal = 1;
            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception Deleting Study : " + ex.Message.ToString());

                transaction.Rollback();
                nRetVal = 0;
            }
            finally
            {
                connection.Close();
            }
            return nRetVal;
        }

        public List<UserStudy> GetStudyList()
        {
            List<UserStudy> allUsers = new List<UserStudy>();

            string sQuery = @"SELECT distinct  S.StudyID,S.StudyName,C.Name FROM _Study S
                                INNER JOIN _ClinicStudy CS ON CS.StudyID=S.StudyID
                                INNER JOIN _Clinic C on CS.ClinicID=C.ClinicID
                                ORDER BY S.StudyID ASC";

            command.CommandText = sQuery;
            OpenConnection(connection);
            command.Parameters.Clear();
            try
            {
                using (SqlDataReader reader = (SqlDataReader)command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserStudy _userObj = new UserStudy();

                        _userObj.StudyID = Convert.ToInt32(reader["StudyID"].ToString());

                        _userObj.StudyName = reader["StudyName"].ToString();
                        _userObj.Name = reader["Name"].ToString();

                        allUsers.Add(_userObj);
                    }
                }
            }
            finally
            {
                connection.Close();
            }

            return allUsers;
        }
        public List<Clinic> GetClinicList()
        {
            List<Clinic> allClinic = new List<Clinic>();

            string sQuery = "SELECT UserId, UserName FROM _user";

            command.CommandText = sQuery;
            OpenConnection(connection);
            command.Parameters.Clear();

            try
            {
                using (SqlDataReader reader = (SqlDataReader)command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Clinic _clinicObj = new Clinic();

                        _clinicObj.ClinicID = Convert.ToInt32(reader["ClinicID"].ToString());
                        _clinicObj.Name = reader["Name"].ToString();

                        allClinic.Add(_clinicObj);
                    }
                }
            }
            finally
            {
                connection.Close();
            }

            return allClinic;
        }
        public int GetTotalStudyForUser(int userID)
        {
            int nRetVal = 0;

            string sQuery = "SELECT COUNT(1) as StudyCount "+
                "FROM _User as u INNER JOIN _UserStudies as us on u.UserID = us.UserID " +
                "WHERE u.UserID = @UserID";

            command.CommandText = sQuery;
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@UserID", userID));

            try
            {
                OpenConnection(connection);
                object retVal = command.ExecuteScalar();
                if (retVal != null)
                {
                    nRetVal = (int)retVal;
                }
                
            }
            catch(Exception ex)
            {

            }
            finally
            {
                connection.Close();
            }

            return nRetVal;
        }
    }
}
