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
    public class OCTResultAccess : SqlAccess
    {
        public string constr;
        public SqlConnection con;

        public OCTResultAccess(string connectionString) : base(connectionString) { constr = connectionString; }

        public OCTResultAccess(IDbConnection connection) : base(connection) { }

        public int InsertResultForPatient(PatientResult pateintResult , string username)
        {
            int retval = -1;
            IDbTransaction transaction = null;

            try
            {
                OpenConnection(connection);
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                int resultID = AddResultInDB(command, pateintResult.ResultObj, username);
                pateintResult.TestID = resultID;
                MapTestForPatient(command, pateintResult, username);

                transaction.Commit();
                retval = 1;
            }
            catch(Exception ex)
            {
                Utils.WriteToLog("Exception While Addind user upload in database: " + ex.Message.ToString());
                transaction.Rollback();
                retval = 0;
            }
            finally
            {
                connection.Close();
            }

            return retval;
        }

        internal int AddResultInDB(IDbCommand command , OctResult resultObj, string username)
        {
            int retval = -1;

            string sQuery = "INSERT INTO _OctResult(Eye, IntrAndSubRetinalFluid, RetinalDisorderFluid, ImageQuality, RetinalVolume, FluidVolume, BScanOrder, ResultDate, Operator) " +
                "VALUES(@Eye, @IntrAndSubRetinalFluid, @RetinalDisorderFluid, @ImageQuality, @RetinalVolume, @FluidVolume, @BScanOrder, GETDATE(), @Operator) SELECT @@IDENTITY";

            command.CommandText = sQuery;
            command.Parameters.Clear();

            command.Parameters.Add(GetNewParameter("@Eye", resultObj.Eye));
            command.Parameters.Add(GetNewParameter("@IntrAndSubRetinalFluid", resultObj.IntraAndSubRetinalFluid));
            command.Parameters.Add(GetNewParameter("@RetinalDisorderFluid", resultObj.IntraAndSubRetinalFluid));
            command.Parameters.Add(GetNewParameter("@ImageQuality", resultObj.ImageQuality));
            command.Parameters.Add(GetNewParameter("@RetinalVolume", resultObj.RetinalVolume));
            command.Parameters.Add(GetNewParameter("@FluidVolume", resultObj.FluidVolume));
            command.Parameters.Add(GetNewParameter("@BScanOrder", resultObj.BScanOrder));
            command.Parameters.Add(GetNewParameter("@Operator", username));

            object identity = command.ExecuteScalar();
            retval = Convert.ToInt32(identity);

            return retval;
        }

        internal int MapTestForPatient(IDbCommand command, PatientResult pateintResult, string username)
        {
            int retval = -1;

            string sQuery = "INSERT INTO _PatientResults(PatientID, ReferenceFileID, TestID) VALUES(@PatientID, @ReferenceFileID, @TestID) SELECT @@IDENTITY";

            command.CommandText = sQuery;
            command.Parameters.Clear();
            
            command.Parameters.Add(GetNewParameter("@PatientID", pateintResult.PatientID));
            command.Parameters.Add(GetNewParameter("@ReferenceFileID", pateintResult.RefFileID));
            command.Parameters.Add(GetNewParameter("@TestID", pateintResult.TestID));

            object identity = command.ExecuteScalar();
            retval = Convert.ToInt32(identity);

            return retval;
        }

        public int InsertPatientResult()
        {
            int retval = -1;
            

            
            return retval;
        }
    }
}
