using ECommercePayment.Application.Services.Abstaract;
using ECommercePayment.Application.Services.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePayment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ECommerceIdentity]
public class ProductsController(IProductService _productService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken = default)
    {
        var response = await _productService.GetProductsAsync(cancellationToken);
        return ReturnHttpStatus(response);
    }
}
