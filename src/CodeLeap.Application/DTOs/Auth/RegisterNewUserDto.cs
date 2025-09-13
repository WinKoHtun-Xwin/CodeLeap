using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeLeap.Application.DTOs.Auth
{
    public class RegisterNewUserDto
    {
        [Required(ErrorMessage = "UserName is required")]
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
