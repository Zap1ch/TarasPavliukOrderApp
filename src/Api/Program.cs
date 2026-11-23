using System.Text.Encodings.Web;
using Api.Endpoints;
using Core;
using Core.Abstractions;
using Core.Storage;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Генерація документа OpenAPI (опис усіх маршрутів).
builder.Services.AddOpenApi();

// Формат помилок RFC 9457: усі помилки — коротке JSON ProblemDetails.
builder.Services.AddProblemDetails();

// Сховище в DI: одне на весь застосунок, стартові дані — 20 замовлень із SampleData.
builder.Services.AddSingleton<IOrderStore>(_ => new InMemoryOrderStore(SampleData.Orders()));

// Кирилиця у відповідях як є, а не \u041A...
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping);

var app = builder.Build();

// Будь-який виняток -> ProblemDetails без stack trace.
// Тіло, яке не розібралося (BadHttpRequestException), лишається 400, решта — 500.
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = ex => ex is BadHttpRequestException bad
        ? bad.StatusCode
        : StatusCodes.Status500InternalServerError
});

// Порожні 404/405 теж отримують тіло ProblemDetails.
app.UseStatusCodePages();

// Документація, Scalar і тестова аварія — лише в Development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // /openapi/v1.json
    app.MapScalarApiReference();    // /scalar/v1

    // Лише для перевірки: навмисний виняток, щоб побачити 500 без stack trace.
    app.MapGet("/debug/throw", () => { throw new InvalidOperationException("Тестова аварія"); });
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapOrders();

app.Run();