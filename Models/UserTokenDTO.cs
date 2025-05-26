using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiQuespond.Models
{
    public class UserTokenDTO
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Token { get; set; }

        public int Id { get; set; }
    }
}
