using Microsoft.AspNetCore.Mvc;
using POS.Model;
using POS.Services;

namespace POS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    public async Task<ActionResult> AddProduct([FromBody] CreateProductRequestDto request)
    {
        var product = await _productService.AddProductAsync(request.ProductName, request.Barcode, request.MRP);
        if (product == null)
            return BadRequest("Product creation failed");
        return Ok( new CreateProductResponseDto(product.Id));
    }
}