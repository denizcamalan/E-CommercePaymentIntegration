using ECommercePayment.Application.Services.Abstaract;
using ECommercePayment.Application.Services.Identity;
using ECommercePayment.Domain.DTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePayment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ECommerceIdentity] 
public class OrdersController(IOrderService _orderService) : BaseController
{
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var response = await _orderService.CreateOrderAsync(request);
        return ReturnHttpStatusCreated(response);
    }

    [HttpPost]
    [Route("{id}/complete")]
    public async Task<IActionResult> CompleteOrder(string id, CompleteOrderRequest request)
    {
        var response = await _orderService.CompleteOrderAsync(id, request);
        return ReturnHttpStatus(response);
    }
}
