using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CodeLeap.Application.Interfaces;
using CodeLeap.Application.DTOs.Product;
using CodeLeap.Application.Common;
using CodeLeap.API.Extensions;

namespace CodeLeap.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;
        public ProductController(IProductService ProductService)
        {
            _productService = ProductService;
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
