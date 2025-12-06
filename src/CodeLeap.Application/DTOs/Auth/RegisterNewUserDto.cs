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
        
        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
        
        /// <summary>
        /// Optional roles to assign to user. Defaults to ["User"] if not provided.
        /// </summary>
        public List<string>? Roles { get; set; }
    }
}
