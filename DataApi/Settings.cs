using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoaDomAndDataAccess
{
    public static class Settings
    {
        public static string ConnectionStringName = "DBConnectionString";
        public static string SecurityDBConnString = "SecurityConnectionString";
        public static string OCTFilesDBConnString = "NoaOctFilesDBString";
        
        public static int ListLimit = 2;

        public const int Week = 7;
        public const int Month = 30;
        public const int Month3 = 90;

        public const string TxtRecoveredNonCompliant = "Recovered non-compliant";

        public const string UrlParam_Reset = "res";
        public const string UrlParam_Type = "typ";

        public static class Params
        {
            public static string Feedback = "Feedback";
            public static string Activity = "Activity";
            public static string DeviceAssignment = "DeviceAssignment";
            public static string DoctorPosition = "DoctorPosition";
            public static string DoctorType = "DoctorType";

        }
        public static class SessionKeys
        {
            public static string UserCountry = "UserCountry";

        }
        public static class ApplicationKeys
        {
            public static string ValidationService = "ValidationService";
        }
        
    }
}
