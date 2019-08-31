using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using NoaDomAndDataAccess;
using NoaDomAndDataAccess.Dom;

namespace NoaDomAndDataAccess.DataAccess
{
    public class UserFileAccess: SqlAccess
    {
        public string constr;
        public SqlConnection con;

        public UserFileAccess(string connectionString) : base(connectionString) { constr = connectionString; }

        public UserFileAccess(IDbConnection connection) : base(connection) { }

        public int AddUploadFileForUser(FileDom fileObj, int userID, int studyID)
        {
            int nRetVal = -1;
            IDbTransaction transaction = null;
            try
            {
                OpenConnection(connection);
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                int nFileID = AddFileEntryInDB(fileObj, command);
                AddFileForUserAndStudy(userID, studyID, nFileID, command);
                transaction.Commit();
                nRetVal = 1;
            }
            catch(Exception ex)
            {
                Utils.WriteToLog("Exception While Addind user upload in database: " + ex.Message.ToString());
                transaction.Rollback();
                nRetVal = 0;
            }
            finally
            {
                connection.Close();
            }

            return nRetVal;
        }
        
        internal int AddFileEntryInDB(FileDom fileObj , IDbCommand command)
        {
            int retVal = -1;

            string sCommand = "INSERT INTO _FileUploads(FileName,UploadPath,FileTypeID,FileStatusID,MD5Hash,FileIdentifier,FileSize,UploadTime,IsFileDeletedPostUpload,DeleteTime,ModifiedTime) " +
                            " VALUES(@Filename, @UploadPath, @FiletypeID, @FileStatusID, @Md5Hash, @FileIdentifier, @FileSize, GETDATE(), 0, NULL, GETDATE())" +
                            " SELECT @@IDENTITY";

            command.CommandText = sCommand;
            command.Parameters.Clear();

            command.Parameters.Add(GetNewParameter("@FileName", fileObj.FileName));
            command.Parameters.Add(GetNewParameter("@UploadPath", fileObj.UploadPath));
            command.Parameters.Add(GetNewParameter("@FiletypeID", fileObj.FileType));
            command.Parameters.Add(GetNewParameter("@FileStatusID", fileObj.FileStatus));
            command.Parameters.Add(GetNewParameter("@Md5Hash", fileObj.MD5Hash));
            command.Parameters.Add(GetNewParameter("@FileIdentifier", fileObj.FileIdentifier));
            command.Parameters.Add(GetNewParameter("@FileSize", fileObj.FileSize));

            object identity = command.ExecuteScalar();
            retVal = Convert.ToInt32(identity);

            return retVal;
        }

        internal int AddFileForUserAndStudy(int userID, int studyID, int fileID, IDbCommand command)
        {
            int retVal = -1;

            string sCommand = "INSERT INTO _UserStudyFiles(UserID, StudyID, FileID, OperationDate) VALUES(@UserID, @StudyID, @FileID, GETDATE()) SELECT @@IDENTITY";

            command.CommandText = sCommand;
            command.Parameters.Clear();

            command.Parameters.Add(GetNewParameter("@UserID", userID));
            command.Parameters.Add(GetNewParameter("@StudyID", studyID));
            command.Parameters.Add(GetNewParameter("@FileID", fileID));

            object identity = command.ExecuteScalar();
            retVal = Convert.ToInt32(identity);

            return retVal;
        }

        public List<UserFileDom> GetToBeProcessedFiles()
        {
            List<UserFileDom> userFiles = new List<UserFileDom>();

            string sQuery = "SELECT usf.UserID, u.UserName, usf.StudyID, us.StudyName, ufu.FileID, ufu.FileName, ufu.UploadPath,ufu.FileIdentifier   " +
                "FROM _UserStudyFiles as usf "+
                "INNER JOIN _User as u on usf.UserID = u.UserID "+
                "INNER JOIN _FileUploads ufu on usf.FileID = ufu.FileID " +
                "INNER JOIN _UserStudies as us on usf.StudyID = us.StudyID "+
                "WHERE ufu.FileStatusID = 1 AND ufu.FileTypeID = 1 AND ufu.IsFileDeletedPostUpload = 0";

            try
            {
                OpenConnection(connection);

                command.CommandText = sQuery;
                command.Parameters.Clear();

                IDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    UserFileDom ufd = new UserFileDom();

                    ufd.UserID = Convert.ToInt32(reader["UserID"]);
                    ufd.UserName = reader["UserName"].ToString();
                    ufd.StudyID = Convert.ToInt32(reader["StudyID"]);
                    ufd.StudyName = reader["StudyName"].ToString();
                    ufd.FileObj.FileID = Convert.ToInt32(reader["FileID"]);
                    ufd.FileObj.FileName = reader["FileName"].ToString();
                    ufd.FileObj.UploadPath = reader["UploadPath"].ToString();
                    ufd.FileObj.FileIdentifier = reader["FileIdentifier"].ToString();

                    userFiles.Add(ufd);
                }

            }
            catch(Exception ex)
            {
                Utils.WriteToLog("Exception Occured while fetching user files for process : ");
                Utils.WriteToLog(ex.Message.ToString());
            }
            finally
            {
                connection.Close();
            }

            return userFiles;
        }

        public void ChangeFileStatus(int fileId, int status)
        {
            string sQuery = "UPDATE _FileUploads SET FileStatusID = @FileStatus, ModifiedTime = GETDATE() WHERE FileID = @FileID";

            try
            {
                OpenConnection(connection);
                command.CommandText = sQuery;
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@FileStatus", status));
                command.Parameters.Add(GetNewParameter("@FileID", fileId));

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception while changing file status");
            }
            finally
            {
                connection.Close();
            }
        }

        public void ChangeFileStatus(int[] fileIds, int status)
        {
            string sQuery = "UPDATE _FileUploads SET FileStatusID = @FileStatus, ModifiedTime = GETDATE()  WHERE FileID IN (@FileIDs)";
            string files = string.Join(",", fileIds);
            try
            {
                OpenConnection(connection);
                command.CommandText = sQuery;
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@FileStatus", status));
                command.Parameters.Add(GetNewParameter("@FileIDs", files));

                command.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                Utils.WriteToLog("Exception while changing file status");
            }
            finally
            {
                connection.Close();
            }
        }
       
        public int AddArchieveFileEntryInDB(int refFileID, string archievePath, int isError)
        {
            int retVal = -1;

            string sQuery = "INSERT INTO _ArchievedFiles(ReferingFileID, ArchievePath, ProcessedWithError) "+
                "VALUES(@ReferingFileID, @ArchievePath, @ProcessedWithError) SELECT @@IDENTITY";

            try
            {
                OpenConnection(connection);
                command.CommandText = sQuery;
                command.Parameters.Clear();
                command.Parameters.Add(GetNewParameter("@ReferingFileID", refFileID));
                command.Parameters.Add(GetNewParameter("@ArchievePath", archievePath));
                command.Parameters.Add(GetNewParameter("@ProcessedWithError", isError));


                object identity = command.ExecuteScalar();
                retVal = Convert.ToInt32(identity);

            }
            catch(Exception ex)
            {
                Utils.WriteToLog("Excpetion occurred while adding archieve file path: "+ ex.Message.ToString());
            }
            finally
            {
                connection.Close();
            }

            return retVal;
        }

        public List<FileDom> GetFilesForUserInStudy(int userId, int StudyID, string startDate, string endDate)
        {
            List<FileDom> files = new List<FileDom>();

            string sQuery = "SELECT f.FileName as ParentFile, fd.DownloadFileID, fd.FileName, fd.FileSize, fd.OctFileID, fd.CreatedOn, "+
                "CONVERT(VARCHAR, fd.LastDownloadedOn, 120) as LastDownloadedOn   " +
                "FROM _UserStudyFiles as usf " +
                "INNER JOIN _FileForDownloads as fd on usf.FileID = fd.ParentFileID  " +
                "INNER JOIN _FileUploads as f on usf.FileID = f.FileID  " +
                "WHERE usf.UserID = @UserID AND usf.StudyID = @StudyID "+
                "AND CONVERT(DATE,fd.CreatedOn) BETWEEN CONVERT(DATE, @StartDate) AND CONVERT(DATE, @EndDate) ";
            try
            {
                OpenConnection(connection);
                command.CommandText = sQuery;
                command.Parameters.Clear();
                command.Parameters.Add(GetNewParameter("@UserID", userId));
                command.Parameters.Add(GetNewParameter("@StudyID", StudyID));
                command.Parameters.Add(GetNewParameter("@StartDate", startDate));
                command.Parameters.Add(GetNewParameter("@EndDate", endDate));

                IDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    FileDom file = new FileDom();

                    file.ParentFileName = reader["ParentFile"].ToString();
                    file.FileID = Convert.ToInt32(reader["DownloadFileID"].ToString());
                    file.FileName = reader["FileName"].ToString();
                    file.FileSize = long.Parse(reader["FileSize"].ToString());
                    file.LastDownloaded = reader["LastDownloadedOn"].ToString();
                    file.CreatedOn = DateTime.Parse(reader["CreatedOn"].ToString());
                    file.IsNewFile = String.IsNullOrEmpty(file.LastDownloaded) ? 1 : 0;
                    file.DisplayFileName = GetDisplayNameForFile(file.FileName, file.ParentFileName);

                    files.Add(file);
                }
            }
            catch(Exception ex)
            {
                Utils.WriteToLog("Exception while fetching download list for user: "+ ex.Message.ToString());
            }
            finally
            {
                connection.Close();
            }

            return files;
        }

        private string GetDisplayNameForFile(string fileName, string parentFileName)
        {
            string displayName = string.Empty;

            if (fileName == parentFileName)
            {
                displayName = "Original_OCT.avi";
            }
            else if(fileName.IndexOf(".pdf") > 0)
            {
                displayName = "Report.pdf";
            }
            else
            {
                displayName = "Marked_OCT.avi";
            }

            return displayName;
        }

        public int InsertFileForDownload(int parentFileID, string fileName, long fileSize, int octFileID)
        {
            int retVal = -1;
            string sQuery = "INSERT INTO _FileForDownloads(ParentFileID,FileName,FileSize, OCTFileID) "+
                "VALUES(@ParentFileID, @FileName, @FileSize, @OCTFileID) SELECT @@IDENTITY";

            try
            {
                OpenConnection(connection);
                command.CommandText = sQuery;
                command.Parameters.Clear();
                command.Parameters.Add(GetNewParameter("@ParentFileID",parentFileID));
                command.Parameters.Add(GetNewParameter("@FileName", fileName));
                command.Parameters.Add(GetNewParameter("@FileSize", fileSize));
                command.Parameters.Add(GetNewParameter("@OCTFileID", octFileID));

                object identity = command.ExecuteScalar();
                retVal = Convert.ToInt32(identity);
            }
            catch(Exception ex)
            {
                Utils.WriteToLog("Exception while adding entry in download table: "+ ex.Message.ToString());
            }
            finally
            {
                connection.Close();
            }

            return retVal;
        }

        public void UpdateDownloadDateForFile(int fileDownloadID)
        {
            IDbTransaction transaction = null;
            try
            {
                OpenConnection(connection);
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                int nFileID = InsertDownloadActivity(fileDownloadID, command);
                UpdateDownloadDate(fileDownloadID, command);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception While Updating download date for user: " + ex.Message.ToString());
                transaction.Rollback();
            }
            finally
            {
                connection.Close();
            }
            
        }

        internal int InsertDownloadActivity(int fileID, IDbCommand command)
        {
            int retval = -1;

            string sQuery = "INSERT INTO FileDownloadHistory(DownloadFileID) VALUES(@DownloadFileID) SELECT @@IDENTITY";
            command.CommandText = sQuery;
            command.Parameters.Clear();

            command.Parameters.Add(GetNewParameter("@DownloadFileID", fileID));

            object identity = command.ExecuteScalar();
            retval = Convert.ToInt32(identity);

            return retval;
        }

        internal void UpdateDownloadDate(int fileID, IDbCommand command)
        {
            string sQuery = "UPDATE _FileForDownloads SET LastDownloadedOn = GETDATE() WHERE DownloadFileID = @DownloadFileID";
            command.CommandText = sQuery;
            command.Parameters.Clear();

            command.Parameters.Add(GetNewParameter("@DownloadFileID", fileID));

            command.ExecuteNonQuery();
        }

        public List<UploadHistoryDom> GetUploadHistoryInStudy(int userId, int StudyID, string startDate, string endDate)
        {
            List<UploadHistoryDom> filesDt = new List<UploadHistoryDom>();

            string sQuery = "SELECT f.FileID, f.FileName, ft.FileType, f.FileSize, fs.FileStatus, " +
                //"CONVERT(VARCHAR, f.UploadTime, 120) as UploadTime " +
                "f.UploadTime "+
                "FROM _UserStudyFiles as usf " +
                "INNER JOIN _FileUploads as f on usf.FileID = f.FileID " +
                "INNER JOIN FileType as ft on f.FileTypeID = ft.ID " +
                "INNER JOIN FileStatusType as fs on f.FileStatusID = fs.ID " +
                "WHERE usf.UserID = @UserID AND usf.StudyID = @StudyID " +
                "AND CONVERT(DATE,f.UploadTime) BETWEEN CONVERT(DATE, @StartDate) AND CONVERT(DATE, @EndDate) " +
                "ORDER BY f.UploadTime DESC";
            try
            {
                OpenConnection(connection);
                command.CommandText = sQuery;
                command.Parameters.Clear();
                command.Parameters.Add(GetNewParameter("@UserID", userId));
                command.Parameters.Add(GetNewParameter("@StudyID", StudyID));
                command.Parameters.Add(GetNewParameter("@StartDate", startDate));
                command.Parameters.Add(GetNewParameter("@EndDate", endDate));

                IDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    UploadHistoryDom file = new UploadHistoryDom();

                    file.FileID = Convert.ToInt32(reader["FileID"].ToString());
                    file.FileName = reader["FileName"].ToString();
                    file.FileType = reader["FileType"].ToString();
                    file.FileSize = long.Parse(reader["FileSize"].ToString());
                    file.FileStatus = reader["FileStatus"].ToString();
                    file.UploadTime = DateTime.Parse(reader["UploadTime"].ToString());

                    filesDt.Add(file);
                }

            }
            catch(Exception ex)
            {
                Utils.WriteToLog("Exception while fetching upload history for user: "+ ex.Message.ToString());
            }
            finally
            {
                connection.Close();
            }

            return filesDt;
            
        }

        public int GetOCTFileIDFromDownloadId(int userID, int downloadFileID)
        {
            int retval = -1;

            try
            {
                OpenConnection(connection);
                string sQuery = "SELECT fd.OCTFileID FROM _UserStudyFiles as usf " +
                        "INNER JOIN _FileUploads as f on usf.FileID = f.FileID " +
                        "INNER JOIN _FileForDownloads as fd on usf.FileID = fd.ParentFileID " +
                        "WHERE fd.DownloadFileID = @DownloadFileID AND usf.UserID = @UserID";

                command.Parameters.Clear();
                command.CommandText = sQuery;
                command.Parameters.Add(GetNewParameter("@DownloadFileID", downloadFileID));
                command.Parameters.Add(GetNewParameter("@UserID", userID));

                object val = command.ExecuteScalar();
                retval = Convert.ToInt32(val);

            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception occured while fetching OCT fileID " + ex.Message.ToString());
            }
            finally
            {
                connection.Close();
            }

            return retval;
        }

        public bool CheckFileInStudy(int studyId, string fileName)
        {
            bool nRetVal = false;

            string sQuery = "SELECT COUNT(1) FROM _UserStudyFiles as usf "+
                    "INNER JOIN _FileUploads as f on usf.FileID = f.FileID "+
                    "WHERE usf.StudyID = @StudyID AND f.FileName = @FileName AND f.IsFileDeletedPostUpload = 0";

            command.CommandText = sQuery;
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@StudyID", studyId));
            command.Parameters.Add(GetNewParameter("@FileName", fileName));

            try
            {
                OpenConnection(connection);
                object retVal = command.ExecuteScalar();
                if (retVal != null)
                {
                    nRetVal = (int)retVal > 0 ? true: false;
                }

            }
            catch (Exception ex)
            {

            }
            finally
            {
                connection.Close();
            }

            return nRetVal;
        }

        public void InsertUserDownloadActivity(int userid, string studyName, string fileName, string filePath, long fileSize)
        {
            string sQuery = "INSERT INTO _UserFileDownloadHistory(UserID, StudyName, FileName, DownloadPath, FileSize, FileType) " +
                "VALUES(@UserID, @StudyName, @FileName, @DownloadPath, @FileSize, @FileType)";
            try
            {
                command.CommandText = sQuery;
                OpenConnection(connection);
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@UserID", userid));
                command.Parameters.Add(GetNewParameter("@StudyName", studyName));
                command.Parameters.Add(GetNewParameter("@FileName", fileName));
                command.Parameters.Add(GetNewParameter("@DownloadPath", filePath));
                command.Parameters.Add(GetNewParameter("@FileSize", fileSize));
                command.Parameters.Add(GetNewParameter("@FileType", GetFileExtByPath(filePath)));

                object identity = command.ExecuteScalar();
                int retVal = Convert.ToInt32(identity);
            }
            finally
            {
                connection.Close();
            }
        }

        public string GetLastDownloadDateForFile(int userid, string studyName, string fileName)
        {
            string retVal = string.Empty;

            string sQuery = "SELECT MAX(CONVERT(char(10), OperationDate,126)) as downloadDate FROM _UserFileDownloadHistory WHERE UserID = @UserID AND StudyName = @StudyName AND FileName = @FileName";
            

            try
            {
                command.CommandText = sQuery;
                OpenConnection(connection);
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@UserID", userid));
                command.Parameters.Add(GetNewParameter("@StudyName", studyName));
                command.Parameters.Add(GetNewParameter("@FileName", fileName));

                using (SqlDataReader reader = (SqlDataReader)command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        retVal = reader["downloadDate"].ToString();
                    }
                }
            }
            finally
            {
                connection.Close();
            }

            return retVal;
        }

        public void InsertUserUploadActivity(int userid, string studyName, string fileName, string filePath , long fileSize, byte[] md5Data)
        {
            string sQuery = "INSERT INTO _UserFileUploads(UserID, StudyName, FileName, UploadPath , FileSize, FileType, MD5Hash) " +
                "VALUES(@UserID, @StudyName, @FileName, @UploadPath, @FileSize, @FileType, @md5)";
            try
            {
                command.CommandText = sQuery;
                OpenConnection(connection);
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@UserID", userid));
                command.Parameters.Add(GetNewParameter("@StudyName", studyName));
                command.Parameters.Add(GetNewParameter("@FileName", fileName));
                command.Parameters.Add(GetNewParameter("@UploadPath", filePath));
                command.Parameters.Add(GetNewParameter("@FileSize", fileSize));
                command.Parameters.Add(GetNewParameter("@FileType", GetFileExtByPath(filePath)));
                command.Parameters.Add(GetNewParameter("@md5", md5Data));

                object identity = command.ExecuteScalar();
                int retVal = Convert.ToInt32(identity);
            }
            finally
            {
                connection.Close();
            }
        }

        public void UpdateFileDeleteAction(int userid, int studyID, string fileName)
        {
            string sQuery = "UPDATE _FileUploads SET IsFileDeletedPostUpload = 1 , DeleteTime = GETDATE() "+
                "WHERE FileID = (  " +
                    "SELECT TOP 1 f.FileID FROM _UserStudyFiles as usf  " +
                    "INNER JOIN _FileUploads as f on usf.FileID = f.FileID " +
                    "WHERE usf.StudyID = @StudyID AND usf.UserID = @UserID AND f.FileName = @FileName " +
                ")";

            try
            {
                command.CommandText = sQuery;
                OpenConnection(connection);
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@UserID", userid));
                command.Parameters.Add(GetNewParameter("@StudyID", studyID));
                command.Parameters.Add(GetNewParameter("@FileName", fileName));

                object identity = command.ExecuteScalar();
            }
            finally
            {
                connection.Close();
            }
        }


        public List<string> GetAllUploadFilesInStudy(int userid, string studyName)
        {
            List<string> allFiles = new List<string>();

            string sQuery = "SELECT uf.FileName FROM _UserStudyFiles as us inner join _UserFileUploads as uf on us.FileID = uf.FileID " +
                            "WHERE us.UserID = @UserID AND us.StudyID = (SELECT StudyID FROM _UserStudies where StudyName = @StudyName)";

            try
            {
                command.CommandText = sQuery;
                OpenConnection(connection);
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@UserID", userid));
                command.Parameters.Add(GetNewParameter("@StudyName", studyName));

                using (SqlDataReader reader = (SqlDataReader)command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        allFiles.Add(reader["FileName"].ToString());
                    }
                }
            }
            finally
            {
                connection.Close();
            }

            return allFiles;
        }

        private string GetFileExtByPath(string filePath)
        {
            string ext = System.IO.Path.GetExtension(filePath);
            ext = ext.Replace(".","");
            return ext;
        }
        
        public DataTable GetUserFilesInStudy(int userid, int studyid)
        {
            DataTable dtFiles = new DataTable();

            return dtFiles;
        }
        
    }
}
