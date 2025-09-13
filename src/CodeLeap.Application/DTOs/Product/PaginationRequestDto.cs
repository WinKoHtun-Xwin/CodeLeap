using System.ComponentModel.DataAnnotations;

namespace CodeLeap.Application.DTOs.Product
{
    public class PaginationRequestDto
    {
        [Range(1, 100, ErrorMessage = "PageNumber must be between 1 and 100")]
        [Required(ErrorMessage = "PageNumber is required")]
        public int PageNumber { get; set; } = 1;
        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        [Required(ErrorMessage = "PageSize is required")]
        public int PageSize { get; set; } = 10;
        [StringLength(50, ErrorMessage = "Search must be less than 50 characters")]
        public string? Search { get; set; } = string.Empty;
    }
}

