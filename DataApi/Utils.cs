using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;

namespace NoaDomAndDataAccess
{
    public class Utils
    {
        private const string source = "NoaApplication";
        public const string log = "Application";

        public static void WriteToLog(string str)
        {
            if (!System.Diagnostics.EventLog.SourceExists(source))
            {
                System.Diagnostics.EventLog.CreateEventSource(source, log);
            }
            System.Diagnostics.EventLog.WriteEntry(source, str, System.Diagnostics.EventLogEntryType.Information);
        }

        public static byte[] GetMD5(string filePath)
        {
            byte[] md5Hash = null;
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        md5Hash =  md5.ComputeHash(stream);
                    }
                }
            }
            catch(Exception ex)
            {
                WriteToLog("Exception Occured while computing file MD5 hash :  "+ ex.Message.ToString());
            }

            return md5Hash;

        }

        public static string GetIdentifierForFile(byte[] bytes, string fileName, string studyName)
        {
            bool upperCase = false;
            string sFileIdentifier = String.Empty;

            string sFileName = String.Concat("_", studyName, "_", fileName); 
            sFileName = Regex.Replace(sFileName, @"\s+", "");

            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
            {
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            }

            result.Append(sFileName);

            return result.ToString();
        }
    }
}
