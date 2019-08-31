using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoaDomAndDataAccess.Dom
{
    public class OctResult
    {
        public int TestID { get; set; }
        public string Eye { get; set; }
        public int IntraAndSubRetinalFluid { get; set; }
        public int RetinalDisorderFluid { get; set; }
        public string ImageQuality { get; set; }
        public float RetinalVolume { get; set; }
        public float FluidVolume { get; set; }
        public string BScanOrder { get; set; }
        public DateTime ResultDate { get; set; }
    }
}
