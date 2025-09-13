using System.ComponentModel.DataAnnotations;

namespace CodeLeap.Application.DTOs.Product
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "ProductName is required")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "ProductName must be between 5 and 50 characters")]
        public required string Name { get; set; }

        [Range(0.01, 10000.00, ErrorMessage = "Price must be between 0.01 and 10000.00")]
        public required decimal Price { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public required string Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative integer")]
        public required int Stock { get; set; }

        [Url(ErrorMessage = "ImageUrl must be a valid URL")]
        public required string ImageUrl { get; set; }
    }
}
