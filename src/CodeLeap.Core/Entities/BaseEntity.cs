namespace CodeLeap.Core.Entities
{
    public abstract class BaseEntity
    { 
        public required string Id { get; set; }
        public required string CreatedBy { get; set; }  
        public  string? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }   
}
