using Microsoft.AspNetCore.Identity;

namespace CodeLeap.Core.Entities
{
    public class UserEntity : IdentityUser
    {
        // Custom properties from BaseEntity (Identity doesn't provide these)
        public required string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
    }
}
