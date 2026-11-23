using System.Text.Encodings.Web;
using Api.Endpoints;
using Core;
using Core.Abstractions;
using Core.Storage;

var builder = WebApplication.CreateBuilder(args);

// Сховище в DI: одне на весь застосунок, стартові дані — 20 замовлень із SampleData.
builder.Services.AddSingleton<IOrderStore>(_ => new InMemoryOrderStore(SampleData.Orders()));

// Кирилиця у відповідях як є, а не \u041A...
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapOrders();

app.Run();