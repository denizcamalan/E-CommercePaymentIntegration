using ECommercePayment.Domain.DTOs.Request;
using FluentValidation;

namespace ECommercePayment.Application.Validation;

public class CompleteOrderRequestValidator : AbstractValidator<CompleteOrderRequest>
{
    public CompleteOrderRequestValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty();

        RuleFor(x => x.PreOrderId)
            .NotEmpty();

        RuleFor(x => x.PaymentConfirmation)
            .Equal(true)
            .WithMessage("Payment must be confirmed to complete the order.");
    }
}

