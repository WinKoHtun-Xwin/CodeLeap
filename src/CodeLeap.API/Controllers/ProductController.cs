using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeLeap.Application.Interfaces;
using CodeLeap.Application.DTOs.Product;
using CodeLeap.Application.Common;
using Swashbuckle.AspNetCore.Annotations;

namespace CodeLeap.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [SwaggerTag("Product management endpoints for creating, reading, updating, and deleting products")]
    public class ProductController(IProductService productService) : BaseController
    {
        private readonly IProductService _productService = productService;

        [HttpGet("pagination")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get products with pagination", Description = "Retrieves products with pagination support. No authentication required.")]
        [SwaggerResponse(200, "Products retrieved successfully", typeof(BaseResponseModelPagination<IEnumerable<ProductDto>>))]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> GetProductsByPagination([FromQuery] PaginationRequestDto paginationRequestDto)
        {
            var result = await _productService.GetProductsByPaginationAsync(paginationRequestDto);
            return ConvertToHttpResponse(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get all products", Description = "Retrieves all products. No authentication required.")]
        [SwaggerResponse(200, "Products retrieved successfully", typeof(BaseResponseModel<IEnumerable<ProductDto>>))]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> GetAllProducts()
        {
            var result = await _productService.GetAllProductsAsync();
            return ConvertToHttpResponse(result);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get product by ID", Description = "Retrieves a specific product by its ID. Requires authentication.")]
        [SwaggerResponse(200, "Product retrieved successfully", typeof(BaseResponseModel<ProductDto>))]
        [SwaggerResponse(400, "Invalid ID parameter")]
        [SwaggerResponse(404, "Product not found")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> GetProductById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(BaseResponseModel<ProductDto>.Failure(
                    HttpStatusCodes.BadRequest,
                    "Product ID is required",
                    "Invalid ID parameter"
                ));
            }

            var result = await _productService.GetProductByIdAsync(id);
            return ConvertToHttpResponse(result);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new product", Description = "Creates a new product. Requires authentication.")]
        [SwaggerResponse(201, "Product created successfully", typeof(BaseResponseModel<ProductDto>))]
        [SwaggerResponse(409, "Product already exists")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            var result = await _productService.CreateProductAsync(createProductDto);
            return ConvertToHttpResponse(result);
        }

        [HttpGet("my-products")]
        [SwaggerOperation(Summary = "Get my products", Description = "Retrieves all products created by the authenticated user. Requires authentication.")]
        [SwaggerResponse(200, "Products retrieved successfully", typeof(BaseResponseModel<IEnumerable<ProductDto>>))]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> GetMyProducts()
        {
            var result = await _productService.GetAllProductsAsync();
            return ConvertToHttpResponse(result);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update product", Description = "Updates an existing product. Requires authentication.")]
        [SwaggerResponse(200, "Product updated successfully", typeof(BaseResponseModel<ProductDto>))]
        [SwaggerResponse(400, "Invalid ID parameter")]
        [SwaggerResponse(404, "Product not found")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> UpdateProduct(string id, [FromBody] CreateProductDto updateProductDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(BaseResponseModel<ProductDto>.Failure(
                    HttpStatusCodes.BadRequest,
                    "Product ID is required",
                    "Invalid ID parameter"
                ));
            }

            var result = await _productService.UpdateProductAsync(id, updateProductDto);
            return ConvertToHttpResponse(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Delete product", Description = "Deletes a product. Requires Admin role.")]
        [SwaggerResponse(200, "Product deleted successfully", typeof(BaseResponseModel<bool>))]
        [SwaggerResponse(400, "Invalid ID parameter")]
        [SwaggerResponse(403, "Forbidden - Admin role required")]
        [SwaggerResponse(404, "Product not found")]
        [SwaggerResponse(500, "Internal server error")]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(BaseResponseModel<bool>.Failure(
                    HttpStatusCodes.BadRequest,
                    "Product ID is required",
                    "Invalid ID parameter"
                ));
            }

            var result = await _productService.DeleteProductAsync(id);
            return ConvertToHttpResponse(result);
        }
    }
}
