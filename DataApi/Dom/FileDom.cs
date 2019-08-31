using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoaDomAndDataAccess.Dom
{
    public class FileDom
    {
        public int FileID { get; set; }
        public string FileName { get; set; }
        public string DisplayFileName { get; set; }
        public string UploadPath { get; set; }
        public int FileType { get; set; }
        public int FileStatus { get; set; }
        public byte[] MD5Hash { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadTime { get; set; }
        public DateTime CreatedOn { get; set; }
        public int IsDeleted { get; set; }
        public DateTime DeleteTime { get; set; }
        public DateTime ModifiedTime { get; set; }
        public string FileIdentifier { get; set; }

        // extra config for download list
        public int RefFileID { get; set; }
        public string LastDownloaded { get; set; }
        public string ParentFileName { get; set; }
        public int IsNewFile { get; set; }
    }
}
