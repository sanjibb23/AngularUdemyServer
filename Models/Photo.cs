using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiQuespond.Models
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; }
        public string IsMain { get; set; }

         public string FilePath { get; set; }

    }
}
