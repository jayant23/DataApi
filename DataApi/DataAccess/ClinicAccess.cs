using NoaDomAndDataAccess.Dom;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace NoaDomAndDataAccess.DataAccess
{
    public class ClinicAccess: SqlAccess
    {
        public string constr;
        public SqlConnection con;

        public ClinicAccess(string connectionString) : base(connectionString) { constr = connectionString; }

        public ClinicAccess(IDbConnection connection) : base(connection) { }
        public List<Clinic> GetClinicList()
        {
            List<Clinic> allClinic = new List<Clinic>();

            string sQuery = "select ClinicID, OfficialID, Name,CRMID, Status = case when status =1 then 'Activate' else 'Deactivate' end  from _clinic order by ClinicID asc";

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
                        _clinicObj.OfficialID = reader["OfficialID"].ToString();
                        _clinicObj.CRMID = reader["CRMID"].ToString();
                        _clinicObj.Name =reader["Name"].ToString();
                        _clinicObj.Status = reader["Status"].ToString();

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
        public DataSet GetClinicDropDownList()
        {
            DataSet clinicData = new DataSet();

            string sQuery = "select ClinicID, Name  from _clinic order by Name ASC";

            try
            {
                OpenConnection(connection);
                using (SqlDataAdapter dAdapter = new SqlDataAdapter(sQuery, (SqlConnection)connection))
                {
                    dAdapter.Fill(clinicData);
                }
            }
            finally
            {
                connection.Close();
            }
            return clinicData;
        }

        public DataTable SearchClinic(string txtSearch)
        {
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    string sql = "select ClinicID, OfficialID, Name,CRMID from _clinic";
                    if (!string.IsNullOrEmpty(txtSearch.Trim()))
                    {
                        sql += " WHERE Name LIKE @Name + '%'";
                        cmd.Parameters.AddWithValue("@Name", txtSearch.Trim());
                    }
                    cmd.CommandText = sql;
                    cmd.Connection = con;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        return dt;
                    }
                }
            }

        }

        public bool isStudyAvailable(int clinicID)
        {
            bool sResponse = false;
            using (SqlConnection con = new SqlConnection(constr))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    string sql = @"select _clinicStudy.StudyID from _clinicStudy INNER JOIN _Study ON _Study.StudyID = _ClinicStudy.StudyID   where _ClinicStudy.clinicID = " + clinicID+"" ;
                    if (clinicID > 0)
                    {
                        //sql += " WHERE C.clinicID ";
                        //cmd.Parameters.AddWithValue("@clinicID", clinicID);
                    
                    cmd.CommandText = sql;
                    cmd.Connection = con;
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                            try
                            {
                                if ((dt.Rows.Count)>0)
                                    sResponse = true;
                                else
                                {
                                    sResponse = false;

                                }
                            }
                            catch
                            {

                            }
                    }
                    }
                }

            }
            
            return sResponse;
        }
        
        public int  ActivateDeActivate(int clinicID)
        {
            int nRetVal = -1;
            IDbTransaction transaction = null;
            try
            {
                OpenConnection(connection);
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                string sQuery = "UPDATE _clinic SET status = 1 WHERE ClinicID = @clinicID";
                command.CommandText = sQuery;
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@clinicID", clinicID));

                command.ExecuteNonQuery();
                transaction.Commit();
                nRetVal = 1;
            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception While Updating status of Clinic: " + ex.Message.ToString());

                transaction.Rollback();
                nRetVal = 0;
            }
            finally
            {
                connection.Close();
            }
            return nRetVal;


        }
        public int DeActivateClinic(int clinicID)
        {
            int nRetVal = -1;
            IDbTransaction transaction = null;
            try
            {
                OpenConnection(connection);
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                string sQuery = "UPDATE _clinic SET status = 0 WHERE ClinicID = @clinicID";
                command.CommandText = sQuery;
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@clinicID", clinicID));

                command.ExecuteNonQuery();
                transaction.Commit();
                nRetVal = 1;
            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception While Updating status of Clinic: " + ex.Message.ToString());

                transaction.Rollback();
                nRetVal = 0;
            }
            finally
            {
                connection.Close();
            }
            return nRetVal;


        }
        public int createStudy(string StudyName, int clinicID)
        {
            int retVal = -1;
            IDbTransaction transaction = null;

            string sQuery = "INSERT INTO _Study(StudyName) OUTPUT Inserted.StudyID " +
                            "VALUES(@StudyName)";
            string s_clinicstudy = "INSERT INTO _clinicstudy(StudyID, ClinicID) VALUES (@studyID,@clinicID)";
            if (!string.IsNullOrEmpty(StudyName))
            {
                try
                {
                    command.CommandText = sQuery;
                    OpenConnection(connection);
                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    command.Parameters.Clear();

                    command.Parameters.Add(GetNewParameter("@StudyName", StudyName));
                    object studyIDidentity = command.ExecuteScalar();
                    command.CommandText = s_clinicstudy;
                    command.Parameters.Add(GetNewParameter("@studyID", Convert.ToInt32(studyIDidentity)));
                    command.Parameters.Add(GetNewParameter("@clinicID", clinicID));
                    object identity = command.ExecuteScalar();
                    transaction.Commit();

                    if (identity != null)
                    {
                        retVal = (int)identity;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
                finally
                {
                    connection.Close();
                }
            }

            return retVal;

        }
        public int createClinic(Clinic clinic,  string operatorName)
        {
            int nRetVal = -1;
            IDbTransaction transaction = null;

            string sQuery = "INSERT INTO _Clinic (OfficialID,Name, RegistrationDate,Operator,OperationType, ContactEmail,OperationDate,CRMID,Status) values (@OfficialID,@Name,getdate(),@Operator,@OperationType,@ContactEmail,getdate(),@CRMID,@Status)";
            if (clinic !=null)
            {
                try
                {
                    OpenConnection(connection);
                    transaction = connection.BeginTransaction();
                    command.Transaction = transaction;
                    command.CommandText = sQuery;
                    command.Parameters.Clear();

                    command.Parameters.Add(GetNewParameter("@OfficialID", clinic.OfficialID));
                    command.Parameters.Add(GetNewParameter("@Name", clinic.Name));
                    command.Parameters.Add(GetNewParameter("@CRMID",clinic.CRMID));
                    command.Parameters.Add(GetNewParameter("@ContactEmail", clinic.ContactEmail)); 
                    command.Parameters.Add(GetNewParameter("@Operator", operatorName.Trim()));
                    command.Parameters.Add(GetNewParameter("@OperationType", "INSERT"));
                    command.Parameters.Add(GetNewParameter("@Status", 0));// status 0 for newly created clinic

                    object identity = command.ExecuteScalar();
                    transaction.Commit();
                    nRetVal = 1;
                    if (identity != null)
                    {
                        nRetVal = (int)identity;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                }
                finally
                {
                    connection.Close();
                }
            }

            return nRetVal;

        }
        public int DeleteClinicbyclinicID(int clinicID)
        {
            int nRetVal = -1;
            IDbTransaction transaction = null;
            try
            {
                OpenConnection(connection);
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                string sQuery = "delete _Clinic where clinicID=@clinicID";
                command.CommandText = sQuery;
                command.Parameters.Clear();

                command.Parameters.Add(GetNewParameter("@clinicID", clinicID));

                command.ExecuteNonQuery();
                transaction.Commit();
                nRetVal = 1;
            }
            catch (Exception ex)
            {
                Utils.WriteToLog("Exception Deleting clinic : " + ex.Message.ToString());

                transaction.Rollback();
                nRetVal = 0;
            }
            finally
            {
                connection.Close();
            }
            return nRetVal;
        }
        public int EditClinic(Clinic clinicinfo,  string operatorName)
        {
            int nRetVal = -1;
            IDbTransaction transaction = null;
            try
            {
                OpenConnection(connection);
                transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                string sQuery = @"UPDATE _clinic SET OfficialID=@OfficialID, Name=@Name, ContactEmail=@ContactEmail,Operator=@Operator,
                 OperationType=@OperationType, OperationDate=getdate(),CRMID=@CRMID  WHERE ClinicID=@ClinicID";
                command.CommandText = sQuery;
                command.Parameters.Clear();
                command.Parameters.Add(GetNewParameter("@OfficialID", clinicinfo.OfficialID));
                command.Parameters.Add(GetNewParameter("@Name", clinicinfo.Name));
                command.Parameters.Add(GetNewParameter("@ContactEmail", clinicinfo.ContactEmail));
                command.Parameters.Add(GetNewParameter("@ClinicID", clinicinfo.ClinicID));
                command.Parameters.Add(GetNewParameter("@Operator", operatorName));
                command.Parameters.Add(GetNewParameter("@OperationType", "UPDATE"));
                command.Parameters.Add(GetNewParameter("@CRMID", clinicinfo.CRMID));
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
        public Clinic GetClinicbyClinicID(int clinicID)
        {
            Clinic clinic = new Clinic();


            string sQuery = @"select OfficialID,Name,CRMID,	ContactEmail from _clinic 
                            where clinicID=@clinicID ORDER BY clinicID ASC";

            command.CommandText = sQuery;
            OpenConnection(connection);
            command.Parameters.Clear();
            command.Parameters.Add(GetNewParameter("@clinicID", clinicID));

            try
            {
                using (SqlDataReader reader = (SqlDataReader)command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clinic.ClinicID = clinicID;
                        clinic.OfficialID =reader["OfficialID"].ToString();
                        clinic.Name = reader["Name"].ToString();
                        clinic.CRMID = reader["CRMID"].ToString();
                        clinic.ContactEmail = reader["ContactEmail"].ToString();
                    }
                }
            }
            catch(Exception ex)
            {

            }
            
            finally
            {
                connection.Close();
            }

            return clinic;
        }
    }

    }
