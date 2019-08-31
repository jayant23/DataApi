using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoaDomAndDataAccess.Dom
{
    public class UploadHistoryDom
    {
        public int FileID { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public string FileStatus { get; set; }
        public DateTime UploadTime { get; set; }
    }
}
