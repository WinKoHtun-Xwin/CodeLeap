namespace CodeLeap.Application.DTOs.Product
{
    public class ProductDto
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required decimal Price { get; set; }
        public required string Description { get; set; }
        public required int Stock { get; set; }
        public required string ImageUrl { get; set; }
        public required string CreatedBy { get; set; }

    }
}
