using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoaDomAndDataAccess.Dom
{
    public class PatientResult
    {
        public int PatientID { get; set; }
        public int TestID { get; set; }
        public int RefFileID { get; set; }
        public OctResult ResultObj = new OctResult();
    }
}
