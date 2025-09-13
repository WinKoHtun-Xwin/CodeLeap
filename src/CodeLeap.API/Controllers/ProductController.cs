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
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        public ProductController(IProductService ProductService)
        {
            _productService = ProductService;
        }

        [HttpGet("pagination")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Get products with pagination", Description = "Retrieves products with pagination support. No authentication required.")]
        [SwaggerResponse(200, "Products retrieved successfully", typeof(BaseResponseModelPagination<IEnumerable<ProductDto>>))]
        public async Task<ActionResult<BaseResponseModelPagination<IEnumerable<ProductDto>>>> GetProductsByPagination([FromQuery] PaginationRequestDto paginationRequestDto)
        {
            var result = await _productService.GetProductsByPaginationAsync(paginationRequestDto);
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<BaseResponseModel<IEnumerable<ProductDto>>>> GetAllProducts()
        {
            var result = await _productService.GetAllProductsAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BaseResponseModel<ProductDto>>> GetProductById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(BaseResponseModel<ProductDto>.Failure(
                    "Product ID is required",
                    "Invalid ID parameter"
                ));
            }

            var result = await _productService.GetProductByIdAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        [HttpPost]
        public async Task<ActionResult<BaseResponseModel<ProductDto>>> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            return await _productService.CreateProductAsync(createProductDto);

        }

        [HttpGet("my-products")]
        public async Task<ActionResult<BaseResponseModel<IEnumerable<ProductDto>>>> GetMyProducts()
        {
            var result = await _productService.GetAllProductsAsync();
            
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BaseResponseModel<ProductDto>>> UpdateProduct(string id, [FromBody] CreateProductDto updateProductDto)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(BaseResponseModel<ProductDto>.Failure(
                    "Product ID is required",
                    "Invalid ID parameter"
                ));
            }

            var result = await _productService.UpdateProductAsync(id, updateProductDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        [SwaggerOperation(Summary = "Delete product", Description = "Deletes a product. Requires Admin role.")]
        [SwaggerResponse(200, "Product deleted successfully", typeof(BaseResponseModel<bool>))]
        [SwaggerResponse(400, "Invalid ID or deletion failed")]
        [SwaggerResponse(403, "Forbidden - Admin role required")]
        public async Task<ActionResult<BaseResponseModel<bool>>> DeleteProduct(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(BaseResponseModel<bool>.Failure(
                    "Product ID is required",
                    "Invalid ID parameter"
                ));
            }

            var result = await _productService.DeleteProductAsync(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
