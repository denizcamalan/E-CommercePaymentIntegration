using ECommercePayment.Domain.DTOs.Request;
using FluentValidation;

namespace ECommercePayment.Application.Validation;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(1000);

        RuleFor(x => x.Currency)
            .NotEmpty();
    }
}

