using Microsoft.AspNetCore.Mvc;

namespace ECommercePayment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
    {
       throw new NotImplementedException();
    }
}
