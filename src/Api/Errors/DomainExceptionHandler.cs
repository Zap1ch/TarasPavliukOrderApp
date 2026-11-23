using Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Api.Errors;

// Окремий обробник винятків: DomainException (порушене бізнес-правило) → 409 Conflict
public sealed class DomainExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;

    public DomainExceptionHandler(IProblemDetailsService problemDetails)
    {
        _problemDetails = problemDetails;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        // інші винятки не мої — повертаю false, їх обробить стандартний обробник (500)
        if (exception is not DomainException)
            return false;

        context.Response.StatusCode = StatusCodes.Status409Conflict;

        // відповідь у форматі ProblemDetails (application/problem+json)
        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Порушено бізнес-правило",
                Detail = exception.Message
            }
        });
    }
}