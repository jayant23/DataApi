using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoaDomAndDataAccess.Dom
{
    public class EmailNotification
    {
        public int NotificationID { get; set; }
        public int FileID { get; set; }
        public int IsEmailSent { get; set; }
        public DateTime EmailSentOn { get; set; }
        public string UserFullName { get; set; }
        public string EmailAddress { get; set; }
        public string FileName { get; set; }

        public EmailNotification()
        {

        }

        public EmailNotification(int fileID, int isEmailSent)
        {
            this.FileID = fileID;
            this.IsEmailSent = isEmailSent;
        }
    }
}
