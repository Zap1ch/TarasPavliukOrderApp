using FluentValidation;

namespace Api.Validation;

// Фільтр маршруту: перевіряє тіло запиту ДО обробника
public sealed class ValidationFilter<T> : IEndpointFilter
{
    private readonly IValidator<T> _validator;

    // валідатор приходить з DI
    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // шукаю серед аргументів маршруту об'єкт типу T (тіло запиту)
        var request = context.Arguments.OfType<T>().FirstOrDefault();
        if (request is null)
            return await next(context);

        var result = await _validator.ValidateAsync(request, context.HttpContext.RequestAborted);

        // є помилки — 400 ProblemDetails, обробник не викликається
        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        // все добре — пускаю далі до обробника
        return await next(context);
    }
}