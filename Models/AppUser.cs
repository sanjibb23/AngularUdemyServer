using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiQuespond.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        [Required]
        public string UserName { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public string KnownAs { get; set; }
        public DateTime? LastActive { get; set; }
        public string Introduction { get; set; }
        public string Interest { get; set; }
        public string Gender { get; set; }
        public string LookingFor { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public List<Photo> UserPhotos { get; set; }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public bool LikebyCurrentUser { get; set; }

        public bool IsOnline { get; set; }

        public List<Message> Messages { get; set; }
    }
}
