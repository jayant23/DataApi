using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;


namespace NoaDomAndDataAccess.DataAccess
{
    public class SecurityUserAccess
    {
        public const string RoleAdministrator = "Administrator";
        public const string RolePatient = "Patient";
        public const string RoleNoaUser = "NOA_User";

        public SecurityUserAccess()
        {

        }

        private string GeneratePassword()
        {
            //PasswordGenerator pg = new PasswordGenerator();
            return "";
            //pg.Generate();
        }

        public MembershipUser GetSecurityUser(string username)
        {
            return Membership.GetUser(username);
        }
        private bool IsUserNameTaken(string userName)
        {
            MembershipUser membershipUser = Membership.GetUser(userName);

            return (membershipUser == null) ? false : true;
        }
        public List<string> GetUserRoles(string username)
        {

            List<string> userRoles = new List<string>();
            if (!String.IsNullOrEmpty(username))
            {
                string[] currentRoles = Roles.GetRolesForUser(username);
                userRoles = currentRoles.ToList();

            }
            return userRoles;
        }
        public string CreateUserRole(string userRole)
        {
            string sMsg = string.Empty;
            try
            {
                if (Roles.RoleExists(userRole))
                {
                    sMsg = "Role Already Exists";
                }
                else
                {
                    Roles.CreateRole(userRole);
                    sMsg = "Role SuccessFully Created";
                }
            }
            catch(Exception e)
            {
                sMsg = e.Message.ToString();
            }
            
            return sMsg;
        }

        internal void SendEmailToCreatedDoctor(string doctorsEmail, string userName, string password)
        {
            try
            {
                
                System.Configuration.Configuration configurationFile = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~/web.config");
                System.Net.Configuration.MailSettingsSectionGroup mailSettings = configurationFile.GetSectionGroup("system.net/mailSettings") as System.Net.Configuration.MailSettingsSectionGroup;
                if (mailSettings != null)
                {
                    int port = mailSettings.Smtp.Network.Port;
                    string host = mailSettings.Smtp.Network.Host;
                    string mailPassword = mailSettings.Smtp.Network.Password;
                    string username = mailSettings.Smtp.Network.UserName;
                }

                System.Net.Mail.MailMessage Message = new System.Net.Mail.MailMessage("Administrator@ForseeHome", doctorsEmail);
                PopulateEmailMessage(Message, userName, password);

                System.Net.Mail.SmtpClient SmtpMail = new System.Net.Mail.SmtpClient(mailSettings.Smtp.Network.Host);
                SmtpMail.Send(Message);
            }
            catch (Exception ex)
            {
                throw new SendingMailFailureException(doctorsEmail, "doctor", ex);
            }
        }

        private void PopulateEmailMessage(System.Net.Mail.MailMessage message, string userName, string password)
        {
            message.Subject = "Welcome to ForseeHome telemedicine site";
            message.Body = "Your UserName is: " + userName + Environment.NewLine +
                            "Your password is: " + password + Environment.NewLine +
                            "Enjoy!";// + Environment.NewLine +
                                     //                            "<HTML><BODY><FONT FACE= Arial SIZE = 2 ><A HREF = " + Page. + ">Link</A></FONT></BODY></HTML>"; ;
        }
    }

    //class CreateUserFailureException : System.Web.Services.Protocols.SoapException
    //{
    //    public CreateUserFailureException(string message)
    //        : base(message, new System.Xml.XmlQualifiedName(ExceptionCode.CreateUserFailure.ToString()))
    //    {
    //    }
    //}
    
    class SendingMailFailureException : Exception
    {
        public SendingMailFailureException(string emailAddress, string emailOwner, Exception innerException) :
            base("Sending mail failure exception to " + emailAddress + " of " + emailOwner, innerException)
        {
        }
    }
}
