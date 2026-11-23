using System.Text.Encodings.Web;
using Api.Endpoints;
using Core;
using Core.Abstractions;
using Core.Storage;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Генерація документа OpenAPI (опис усіх маршрутів).
builder.Services.AddOpenApi();

// Сховище в DI: одне на весь застосунок, стартові дані — 20 замовлень із SampleData.
builder.Services.AddSingleton<IOrderStore>(_ => new InMemoryOrderStore(SampleData.Orders()));

// Кирилиця у відповідях як є, а не \u041A...
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping);

var app = builder.Build();

// Документація і Scalar лише в Development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // /openapi/v1.json
    app.MapScalarApiReference();    // /scalar/v1
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapOrders();

app.Run();