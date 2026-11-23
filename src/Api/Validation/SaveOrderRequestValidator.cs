using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

// Правила для тіла POST/PUT: ті самі, що були в TryValidate, але в окремому класі
public sealed class SaveOrderRequestValidator : AbstractValidator<SaveOrderRequest>
{
    public SaveOrderRequestValidator()
    {
        RuleFor(x => x.Customer)
            .NotEmpty().WithMessage("Покупець обов'язковий, до 50 символів.")
            .MaximumLength(50).WithMessage("Покупець обов'язковий, до 50 символів.");

        // Варіант «Замовлення»: lines не може бути порожнім.
        RuleFor(x => x.Lines)
            .NotEmpty().WithMessage("Замовлення має містити хоча б один рядок.");

        // кожен рядок перевіряє окремий валідатор нижче; ключі помилок — Lines[0].ProductId тощо
        RuleForEach(x => x.Lines)
            .NotNull().WithMessage("Рядок замовлення не може бути порожнім.")
            .SetValidator(new SaveOrderLineRequestValidator());
    }
}

// Правила для одного рядка замовлення
public sealed class SaveOrderLineRequestValidator : AbstractValidator<SaveOrderLineRequest>
{
    public SaveOrderLineRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Код товару обов'язковий, до 20 символів.")
            .MaximumLength(20).WithMessage("Код товару обов'язковий, до 20 символів.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Назва товару обов'язкова, до 200 символів.")
            .MaximumLength(200).WithMessage("Назва товару обов'язкова, до 200 символів.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Ціна не може бути від'ємною.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Кількість має бути більшою за нуль.");
    }
}