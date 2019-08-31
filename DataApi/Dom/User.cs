using NoaDomAndDataAccess.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoaDomAndDataAccess.Dom
{
    public class User
    {
        public DateTime RegistrationDate { get; set; }
        public int UserID { get; set; }
        public int ClinicID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string UserAccessToken { get; set; }
        public string UserEmail { get; set; }
        public string ClinicName { get; set; }
        public string MobileNumber { get; set; }

        public User()
        {

        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
                
    }
}
