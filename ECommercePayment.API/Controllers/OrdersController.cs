using Microsoft.AspNetCore.Mvc;

namespace ECommercePayment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController() : ControllerBase
{
    [HttpPost]
    [Route("create")]
    public IActionResult CreateOrder()
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    [Route("{id}/complete")]
    public IActionResult CompleteOrder(int id)
    {
        throw new NotImplementedException();
    }
}
