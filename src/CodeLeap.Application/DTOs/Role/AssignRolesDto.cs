using System.ComponentModel.DataAnnotations;

namespace CodeLeap.Application.DTOs.Role
{
    public class AssignRolesDto
    {
        [Required(ErrorMessage = "UserId is required")]
        public required string UserId { get; set; }
        
        [Required(ErrorMessage = "At least one role is required")]
        [MinLength(1, ErrorMessage = "At least one role must be specified")]
        public required List<string> Roles { get; set; }
    }
}
