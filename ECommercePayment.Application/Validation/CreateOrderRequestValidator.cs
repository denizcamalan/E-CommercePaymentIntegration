using ECommercePayment.Domain.DTOs.Request;
using FluentValidation;

namespace ECommercePayment.Application.Validation;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {

    }
}

