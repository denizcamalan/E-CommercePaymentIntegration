using System.Net;
using ECommercePayment.Domain.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePayment.API.Controllers;

public class BaseController : ControllerBase
    {
        public IActionResult ReturnHttpStatus<T>(BaseResponse<T> response) where T : class
        {
            if (response.ErrorData is null)
                return Ok(response.Data);

            switch (response.ErrorData?.httpCode)
            {
                case HttpStatusCode.Unauthorized:
                    return Unauthorized(response.ErrorData);
                case HttpStatusCode.Forbidden:
                    return StatusCode(403, response.ErrorData);
                case HttpStatusCode.BadRequest:
                    return BadRequest(response.ErrorData);
                case HttpStatusCode.Created:
                    return StatusCode(201, response.Data);
                case HttpStatusCode.NoContent:
                    return NoContent();
                case HttpStatusCode.NotFound:
                    return NotFound(response.ErrorData);
                case HttpStatusCode.InternalServerError:
                    return StatusCode(500, response.ErrorData);
                case HttpStatusCode.GatewayTimeout:
                    return StatusCode(504, response.ErrorData);
                case HttpStatusCode.ServiceUnavailable:
                    return StatusCode(503, response.ErrorData);
                case HttpStatusCode.TooManyRequests:
                    return StatusCode(429, response.ErrorData);
                default:
                    return StatusCode(503, response.ErrorData);
            }
        }

        public IActionResult ReturnHttpStatusCreated<T>(BaseResponse<T> response) where T : class
        {
            if (response.ErrorData is null)
                return StatusCode(201, response.Data);

            switch (response.ErrorData?.httpCode)
            {
                case HttpStatusCode.Unauthorized:
                    return Unauthorized(response.ErrorData);
                case HttpStatusCode.Forbidden:
                    return StatusCode(403, response.ErrorData);
                case HttpStatusCode.BadRequest:
                    return BadRequest(response.ErrorData);
                case HttpStatusCode.NoContent:
                    return NoContent();
                case HttpStatusCode.NotFound:
                    return NotFound(response.ErrorData);
                case HttpStatusCode.InternalServerError:
                    return StatusCode(500, response.ErrorData);
                case HttpStatusCode.GatewayTimeout:
                    return StatusCode(504, response.ErrorData);
                case HttpStatusCode.ServiceUnavailable:
                    return StatusCode(503, response.ErrorData);
                case HttpStatusCode.TooManyRequests:
                    return StatusCode(429, response.ErrorData);
                default:
                    return StatusCode(503, response.ErrorData);
            }
        }
}
