using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;

namespace Core.Tests;

public class OrderServiceTests
{
    [Fact]   // 1) інваріант: відмова і незмінний стан
    public void AddLine_WithZeroQuantity_Throws_AndKeepsState()
    {
        // Arrange
        var store = new InMemoryOrderStore();
        var service = new OrderService(store);
        var order = service.CreateOrder("C-1");
        service.AddLine(order.Id, "P-1", "Клавіатура", 1200m, 1);

        // Act + Assert: точний тип, бо Assert.Throws не приймає нащадків
        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.AddLine(order.Id, "P-2", "Миша", 450m, 0));

        // Стан не змінився
        var saved = store.GetById(order.Id)!;
        Assert.Single(saved.Lines);
        Assert.Equal(1200m, saved.Total);
    }

    [Fact]   // 1б) бізнес-правило: рядок у підтверджене замовлення
    public void AddLine_ToConfirmedOrder_ThrowsDomainException_AndKeepsState()
    {
        var store = new InMemoryOrderStore();
        var service = new OrderService(store);
        var order = service.CreateOrder("C-1");
        service.AddLine(order.Id, "P-1", "Клавіатура", 1200m, 1);
        service.Confirm(order.Id);

        Assert.Throws<DomainException>(
            () => service.AddLine(order.Id, "P-3", "Монітор", 5000m, 1));

        Assert.Single(store.GetById(order.Id)!.Lines);
    }

    [Fact]   // варіант «Замовлення»: замовлення вже підтверджено
    public void Confirm_Twice_ThrowsDomainException()
    {
        var service = new OrderService(new InMemoryOrderStore());
        var order = service.CreateOrder("C-1");
        service.AddLine(order.Id, "P-1", "Клавіатура", 1200m, 1);
        service.Confirm(order.Id);

        Assert.Throws<DomainException>(() => service.Confirm(order.Id));
        Assert.Equal(OrderStatus.Confirmed, service.Find(order.Id)!.Status);
    }

    [Fact]   // 2) подія з лаби 6 як спосіб перевірки
    public void CreateOrder_RaisesChangedExactlyOnce()
    {
        var service = new OrderService(new InMemoryOrderStore());
        var events = new List<OrderChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        var order = service.CreateOrder("C-1");

        Assert.Single(events);
        Assert.Equal(OrderChangeKind.OrderCreated, events[0].Kind);
        Assert.Equal(order.Id, events[0].OrderId);
    }

    [Theory]   // варіант «Замовлення»: сума після додавання рядків
    [InlineData(1200, 1, 450, 2, 2100)]
    [InlineData(100, 3, 50, 1, 350)]
    [InlineData(10, 5, 0, 1, 50)]   // безкоштовний товар не змінює суму
    public void Total_AfterAddingLines_IsSumOfLines(int price1, int qty1, int price2, int qty2, int expected)
    {
        var service = new OrderService(new InMemoryOrderStore());
        var order = service.CreateOrder("C-1");

        service.AddLine(order.Id, "P-1", "Товар 1", price1, qty1);
        service.AddLine(order.Id, "P-2", "Товар 2", price2, qty2);

        Assert.Equal((decimal)expected, service.Find(order.Id)!.Total);
    }
}