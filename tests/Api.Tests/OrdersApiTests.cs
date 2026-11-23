using System.Net;
using System.Net.Http.Json;
using Api.Contracts;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;

// Інтеграційні тести: весь API піднімається в пам'яті, запити — справжні HTTP
public class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    // xUnit сам створює фабрику один раз на весь клас і передає сюди
    public OrdersApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]   // POST дає 201, GET за отриманим Location — 200
    public async Task Post_Returns201_AndGetByLocation_Returns200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new SaveOrderRequest("C-900",
            [new SaveOrderLineRequest("P-1", "Клавіатура", 1200m, 1)]);

        // Act 1: створюю замовлення
        var postResponse = await client.PostAsJsonAsync("/api/orders", request);

        // Assert 1: 201 і є адреса нового замовлення
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        Assert.NotNull(postResponse.Headers.Location);

        // Act 2: GET саме за адресою з Location
        var getResponse = await client.GetAsync(postResponse.Headers.Location);

        // Assert 2: 200 і це те саме замовлення
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var created = await postResponse.Content.ReadFromJsonAsync<OrderResponse>();
        var loaded = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(created);
        Assert.NotNull(loaded);
        Assert.Equal(created.Id, loaded.Id);
        Assert.Equal("C-900", loaded.Customer);
        Assert.Equal(1200m, loaded.Total);
    }

    [Fact]   // некоректне тіло — 400 від фільтра валідації, замовлення не створюється
    public async Task Post_InvalidBody_Returns400()
    {
        var client = _factory.CreateClient();
        var request = new SaveOrderRequest("", []);

        var response = await client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}