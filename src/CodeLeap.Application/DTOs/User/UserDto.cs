namespace CodeLeap.Application.DTOs.User
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public required string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
