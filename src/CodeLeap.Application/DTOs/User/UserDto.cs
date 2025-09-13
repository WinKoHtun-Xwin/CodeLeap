namespace CodeLeap.Application.DTOs.User
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Username { get; set; }
        public required string CreatedBy { get; set; }
        public required string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
