using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoaDomAndDataAccess.Dom
{
    public class UserFileDom: User
    {
        public int StudyID { get; set; }
        public string StudyName { get; set; }
        public FileDom FileObj = new FileDom();
    }
}



