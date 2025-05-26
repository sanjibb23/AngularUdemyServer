using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiQuespond.Models
{
    public class PhotoUpload
    {
        public string UserName { get; set; }
        public string FileName { get; set; }
        public string IsMain { get; set; }
        public string FilePath { get; set; }
        public int PhotoId { get; set; }
        public string Status { get; set; }
    }
}
