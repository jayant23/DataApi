using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using NoaDomAndDataAccess.Dom;

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
            PasswordGenerator pg = new PasswordGenerator();
            return pg.Generate();
        }

        public MembershipUser GetSecurityUser(string username)
        {
            return Membership.GetUser(username);
        }
        
        internal void CreateSecurityUser(string userName, string password, string userRole)
        {
            if (!Roles.RoleExists(userRole))
            {
                throw new RoleNameNotExistsException(userRole);
            }

            try
            {
                //WriteToEventLog("SecurityUserAccess::CreateSecurityUser - username accepted is " + userName);
                // Create new user.
                MembershipUser newUser = Membership.CreateUser(userName, "1234567" /*password*/);
                //WriteToEventLog("SecurityUserAccess::CreateSecurityUser - user created");
                ////Make it change password on next login
                // newUser.Comment = "MustChangePassword";
                Membership.UpdateUser(newUser);

                //Add user to role.
                Roles.AddUserToRole(newUser.UserName, userRole.ToString());
                //WriteToEventLog("SecurityUserAccess::CreateSecurityUser - user: " + newUser.UserName + " added to role: " + userRole.ToString());
            }
            catch (MembershipCreateUserException ex)
            {
                //WriteToEventLog("SecurityUserAccess::CreateSecurityUser - user NOT created!! " + System.Environment.NewLine + ex.ToString());
                switch (ex.StatusCode)
                {
                    case MembershipCreateStatus.DuplicateUserName:
                        throw new UserAlresdyExistsException(userName);

                    case MembershipCreateStatus.DuplicateEmail:
                        throw new CreateUserFailureException("A username for that e-mail address already exists. Please enter a different e-mail address.");

                    case MembershipCreateStatus.InvalidPassword:
                        throw new InvalidPasswordException();

                    case MembershipCreateStatus.InvalidEmail:
                        throw new CreateUserFailureException("The e-mail address provided is invalid. Please check the value and try again.");

                    case MembershipCreateStatus.InvalidAnswer:
                        throw new CreateUserFailureException("The password retrieval answer provided is invalid. Please check the value and try again.");

                    case MembershipCreateStatus.InvalidQuestion:
                        throw new CreateUserFailureException("The password retrieval question provided is invalid. Please check the value and try again.");

                    case MembershipCreateStatus.InvalidUserName:
                        throw new CreateUserFailureException("The user name provided is invalid. Please check the value and try again.");

                    case MembershipCreateStatus.ProviderError:
                        throw new CreateUserFailureException("The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.");

                    case MembershipCreateStatus.UserRejected:
                        throw new CreateUserFailureException("The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.");

                    default:
                        throw new CreateUserFailureException("An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.");
                }
            }
            catch (Exception ex)
            {
                //WriteToEventLog("SecurityUserAccess::CreateSecurityUser - user NOT created!! " + System.Environment.NewLine + ex.ToString());
                throw ex;
            }
        }

        internal void CreateSecurityUserForExistingPatients(string userName, string password, string userRole)
        {
            if (!Roles.RoleExists(userRole))
            {
                throw new RoleNameNotExistsException(userRole);
            }

            try
            {
                MembershipUser newUser = Membership.CreateUser(userName, password);
                Membership.UpdateUser(newUser);
                Roles.AddUserToRole(newUser.UserName, userRole.ToString());
            }
            catch (MembershipCreateUserException ex)
            {
                //WriteToEventLog("SecurityUserAccess::CreateSecurityUser - user NOT created!! " + System.Environment.NewLine + ex.ToString());
                switch (ex.StatusCode)
                {
                    case MembershipCreateStatus.DuplicateUserName:
                        throw new UserAlresdyExistsException(userName);

                    case MembershipCreateStatus.DuplicateEmail:
                        throw new CreateUserFailureException("A username for that e-mail address already exists. Please enter a different e-mail address.");

                    case MembershipCreateStatus.InvalidPassword:
                        throw new InvalidPasswordException();

                    case MembershipCreateStatus.InvalidEmail:
                        throw new CreateUserFailureException("The e-mail address provided is invalid. Please check the value and try again.");

                    case MembershipCreateStatus.InvalidAnswer:
                        throw new CreateUserFailureException("The password retrieval answer provided is invalid. Please check the value and try again.");

                    case MembershipCreateStatus.InvalidQuestion:
                        throw new CreateUserFailureException("The password retrieval question provided is invalid. Please check the value and try again.");

                    case MembershipCreateStatus.InvalidUserName:
                        throw new CreateUserFailureException("The user name provided is invalid. Please check the value and try again.");

                    case MembershipCreateStatus.ProviderError:
                        throw new CreateUserFailureException("The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.");

                    case MembershipCreateStatus.UserRejected:
                        throw new CreateUserFailureException("The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.");

                    default:
                        throw new CreateUserFailureException("An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.");
                }
            }
            catch (Exception ex)
            {
                //WriteToEventLog("SecurityUserAccess::CreateSecurityUser - user NOT created!! " + System.Environment.NewLine + ex.ToString());
                throw ex;
            }
        }

        private bool IsUserNameTaken(string userName)
        {
            MembershipUser membershipUser = Membership.GetUser(userName);

            return (membershipUser == null) ? false : true;
        }

        public void ChangePassword(string userName, string newPassword)
        {
            MembershipUser membershipUser = Membership.GetUser(userName);
            string randomResetPassword = membershipUser.ResetPassword();

            try
            {
                membershipUser.ChangePassword(randomResetPassword, newPassword);
            }
            catch (Exception)
            {
                throw new InvalidPasswordException();
            }
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


        public void UpdateUserRoles(string username, List<string> userRoles)
        {

            string[] currentRoles = Roles.GetRolesForUser(username);
            foreach (string currentRole in currentRoles)
            {
                if (!userRoles.Contains(currentRole))
                {
                    try
                    {
                        Roles.RemoveUserFromRole(username, currentRole);
                    }
                    catch (Exception ex)
                    {
                        //WriteToEventLog("SecurityUserAccess::CreateSecurityUser - user NOT created!! " + System.Environment.NewLine + ex.ToString());
                        throw ex;
                    }

                }
            }

            foreach (string userRole in userRoles)
            {

                if (!Roles.RoleExists(userRole))
                {
                    throw new RoleNameNotExistsException(userRole);
                }

                try
                {
                    //WriteToEventLog("before AddUserToRole");
                    if (!Roles.IsUserInRole(username, userRole))
                    {
                        Roles.AddUserToRole(username, userRole);
                    }
                }
                catch (MembershipCreateUserException ex)
                {
                    switch (ex.StatusCode)
                    {
                        case MembershipCreateStatus.InvalidUserName:
                            throw new CreateUserFailureException("The user name provided is invalid. Please check the value and try again.");
                        default:
                            throw new CreateUserFailureException("An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.");
                    }
                }
                catch (Exception ex)
                {
                    //WriteToEventLog("SecurityUserAccess::CreateSecurityUser - user NOT created!! " + System.Environment.NewLine + ex.ToString());
                    throw ex;
                }
            }
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

        public void UpdateUserRole(string username, string userRole, string olduserRole)
        {

            if (!Roles.RoleExists(userRole))
            {
                throw new RoleNameNotExistsException(userRole);
            }
            if (!Roles.RoleExists(olduserRole))
            {
                throw new RoleNameNotExistsException(olduserRole);
            }

            try
            {
                MembershipUser user = GetSecurityUser(username);
                //WriteToEventLog("before RemoveUserFromRole: is user in role " + olduserRole + ": " + Roles.IsUserInRole(olduserRole).ToString());
                if (Roles.IsUserInRole(user.UserName, olduserRole))
                {
                    Roles.RemoveUserFromRole(user.UserName, olduserRole);
                }
                //WriteToEventLog("before AddUserToRole");
                if (!Roles.IsUserInRole(user.UserName, userRole))
                {
                    Roles.AddUserToRole(user.UserName, userRole);
                }
            }
            catch (MembershipCreateUserException ex)
            {
                switch (ex.StatusCode)
                {
                    case MembershipCreateStatus.InvalidUserName:
                        throw new CreateUserFailureException("The user name provided is invalid. Please check the value and try again.");
                    default:
                        throw new CreateUserFailureException("An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateUser(string oldUserName, string newUserName, string password, string userRole)
        {
            //if new user name is null, it means the old user will just be deleted 
            //and no new user will be created.
            if (newUserName != null)
            {
                if (IsUserNameTaken(newUserName))
                {
                    throw new UserAlresdyExistsException(newUserName);
                }

                CreateSecurityUser(newUserName, password, userRole);
            }

            try
            {
                Membership.DeleteUser(oldUserName);
            }
            catch (Exception)
            {
                //Security user is not a must, so it can be first added on update existing user.
            }
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

    class CreateUserFailureException : System.Web.Services.Protocols.SoapException
    {
        public CreateUserFailureException(string message)
            : base(message, new System.Xml.XmlQualifiedName(ExceptionCode.CreateUserFailure.ToString()))
        {
        }
    }

    class UserAlresdyExistsException : CreateUserFailureException
    {
        public UserAlresdyExistsException(string username)
            : base("The user " + username + " alredy exists")
        {
        }
    }

    class UserEmailAreadyExistsException : CreateUserFailureException
    {
        public UserEmailAreadyExistsException(string username, string email)
            : base("The user " + username + " email " + email + " alredy exists")
        {
        }
    }

    class RoleNameNotExistsException : CreateUserFailureException
    {
        public RoleNameNotExistsException(string userRole)
            : base("The role " + userRole + " does not exist")
        {
        }
    }

    class InvalidPasswordException : CreateUserFailureException
    {
        public InvalidPasswordException()
            : base("The password provided is invalid. Password must be at least 7 characters long.")
        {
        }
    }

    class SendingMailFailureException : Exception
    {
        public SendingMailFailureException(string emailAddress, string emailOwner, Exception innerException) :
            base("Sending mail failure exception to " + emailAddress + " of " + emailOwner, innerException)
        {
        }
    }
}
