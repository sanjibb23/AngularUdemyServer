using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiQuespond.Models
{
    public class MsgUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public bool IsOnline { get; set; }

    }
}
