using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CodeLeap.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email or Username is required")]
        public required string EmailOrUsername { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
