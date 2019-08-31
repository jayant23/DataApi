using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NoaDomAndDataAccess.Dom;


namespace NoaDomAndDataAccess.DataAccess
{
    public class FileStreamAccess: SqlAccess
    { 
        public string constr;
        public SqlConnection con;

        public FileStreamAccess(string connectionString) : base(connectionString) { constr = connectionString; }

        public FileStreamAccess(IDbConnection connection) : base(connection) { }

        public int StoreFileInDataBase(string fileName, string filePath, int parentFileID, string parentKey, string key, byte[] md5Data)
        {
            int nRetval = 0;
            string sQuery = "INSERT INTO NoaOCTFiles (FileMD5, FileTitle, FileData, ParentFileID, ParentFileKey, FileKey) " +
                    "VALUES(@FileMD5, @FileTitle, @FileData, @ParentFileID, @ParentFileKey, @FileKey) SELECT @@IDENTITY";

            try
            {
                FileInfo fi = new FileInfo(filePath);
                FileStream fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader br = new BinaryReader(fs);
                byte[] fileData = br.ReadBytes((int)fs.Length);
                br.Close();
                fs.Close();

                OpenConnection(connection);

                command.CommandText = sQuery;
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@FileMD5", md5Data));
                command.Parameters.Add(GetNewParameter("@FileTitle", fileName));
                command.Parameters.Add(GetNewParameter("@FileData", fileData));
                command.Parameters.Add(GetNewParameter("@ParentFileID", parentFileID));
                command.Parameters.Add(GetNewParameter("@ParentFileKey", parentKey));
                command.Parameters.Add(GetNewParameter("@FileKey", key));

                object identity = command.ExecuteScalar();

                nRetval = Convert.ToInt32(identity);
            }
            catch(Exception ex)
            {
                Utils.WriteToLog("Exception while saving file in database: " + ex.Message.ToString());
                nRetval = 0;
            }
            finally
            {
                connection.Close();
            }

            return nRetval;
        }

        
    }
}
