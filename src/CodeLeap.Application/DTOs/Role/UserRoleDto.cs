namespace CodeLeap.Application.DTOs.Role
{
    public class UserRoleDto
    {
        public required string UserId { get; set; }
        public required List<string> Roles { get; set; }
    }
}
