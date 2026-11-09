using Core.Domain;
using Core.Dto;

namespace Core;

// Тестові замовлення для демо і звітів (20 шт., три статуси, кілька дорогих).
public static class SampleData
{
    public static List<Order> Orders()
    {
        var orders = new List<Order>();

        var o1 = Order.Create("O-001", "C-100");
        o1.AddLine("P-1", "Клавіатура", 1200m, 1);
        o1.AddLine("P-2", "Миша", 450m, 2);
        o1.Confirm();
        orders.Add(o1);

        var o2 = Order.Create("O-002", "C-100");
        o2.AddLine("P-3", "Монітор", 5000m, 1);
        o2.Confirm();
        orders.Add(o2);

        var o3 = Order.Create("O-003", "C-101");
        o3.AddLine("P-8", "Ноутбук", 28000m, 1);
        o3.AddLine("P-2", "Миша", 450m, 1);
        o3.Confirm();
        orders.Add(o3);

        var o4 = Order.Create("O-004", "C-101");
        o4.AddLine("P-5", "Кабель HDMI", 250m, 3);
        orders.Add(o4); // чернетка

        var o5 = Order.Create("O-005", "C-102");
        o5.AddLine("P-4", "Навушники", 900m, 2);
        o5.Cancel();
        orders.Add(o5);

        var o6 = Order.Create("O-006", "C-102");
        o6.AddLine("P-6", "Веб-камера", 1500m, 1);
        o6.AddLine("P-7", "Флешка 32 ГБ", 300m, 4);
        o6.Confirm();
        orders.Add(o6);

        var o7 = Order.Create("O-007", "C-103");
        o7.AddLine("P-9", "Принтер", 6500m, 1);
        o7.Confirm();
        orders.Add(o7);

        var o8 = Order.Create("O-008", "C-103");
        o8.AddLine("P-10", "Килимок для миші", 200m, 2);
        o8.AddLine("P-2", "Миша", 450m, 1);
        orders.Add(o8); // чернетка

        var o9 = Order.Create("O-009", "C-104");
        o9.AddLine("P-3", "Монітор", 5000m, 2);
        o9.AddLine("P-5", "Кабель HDMI", 250m, 2);
        o9.Confirm();
        orders.Add(o9);

        var o10 = Order.Create("O-010", "C-104");
        o10.AddLine("P-7", "Флешка 32 ГБ", 300m, 10);
        o10.Confirm();
        o10.Cancel();
        orders.Add(o10);

        var o11 = Order.Create("O-011", "C-105");
        o11.AddLine("P-1", "Клавіатура", 1200m, 1);
        orders.Add(o11); // чернетка

        var o12 = Order.Create("O-012", "C-105");
        o12.AddLine("P-8", "Ноутбук", 28000m, 1);
        o12.AddLine("P-4", "Навушники", 900m, 1);
        o12.Confirm();
        orders.Add(o12);

        var o13 = Order.Create("O-013", "C-100");
        o13.AddLine("P-6", "Веб-камера", 1500m, 2);
        orders.Add(o13); // чернетка

        var o14 = Order.Create("O-014", "C-106");
        o14.AddLine("P-9", "Принтер", 6500m, 1);
        o14.AddLine("P-10", "Килимок для миші", 200m, 1);
        o14.Confirm();
        orders.Add(o14);

        var o15 = Order.Create("O-015", "C-106");
        o15.AddLine("P-5", "Кабель HDMI", 250m, 1);
        o15.Cancel();
        orders.Add(o15);

        var o16 = Order.Create("O-016", "C-107");
        o16.AddLine("P-2", "Миша", 450m, 3);
        o16.AddLine("P-10", "Килимок для миші", 200m, 3);
        o16.Confirm();
        orders.Add(o16);

        // Лаба 7: ще 4 замовлення для звітів.
        var o17 = Order.Create("O-017", "C-101");
        o17.AddLine("P-3", "Монітор", 5000m, 1);
        o17.AddLine("P-2", "Миша", 450m, 1);
        o17.Confirm();
        orders.Add(o17);

        var o18 = Order.Create("O-018", "C-107");
        o18.AddLine("P-9", "Принтер", 6500m, 1);
        orders.Add(o18); // чернетка

        var o19 = Order.Create("O-019", "C-108");
        o19.AddLine("P-8", "Ноутбук", 28000m, 1);
        o19.AddLine("P-4", "Навушники", 900m, 1);
        o19.Confirm();
        orders.Add(o19); // та сама сума, що в O-012 — перевірка ThenBy

        var o20 = Order.Create("O-020", "C-102");
        o20.AddLine("P-6", "Веб-камера", 1500m, 2);
        o20.Cancel();
        orders.Add(o20);

        return orders;
    }

    // Додаткове 1 (лаба 7): клієнти для GroupJoin. C-109 навмисно без замовлень.
    public static List<CustomerDto> Customers()
    {
        return new List<CustomerDto>
        {
            new CustomerDto("C-100", "Тарас Павлюк"),
            new CustomerDto("C-101", "Артур Студинський"),
            new CustomerDto("C-102", "Денис Дубик"),
            new CustomerDto("C-103", "Едуард Боднар"),
            new CustomerDto("C-104", "Тарас Демчишин"),
            new CustomerDto("C-105", "Олександр Срогий"),
            new CustomerDto("C-106", "Артем Федонюк"),
            new CustomerDto("C-107", "Олександр Сорочинський"),
            new CustomerDto("C-108", "Захар Беркута"),
            new CustomerDto("C-109", "Ростислав Потоцький")
        };
    }
}