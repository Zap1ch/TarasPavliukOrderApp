using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var warehouses = new List<WarehouseDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            switch (ParseLine(line))
            {
                case ParseProduct product:
                    products.Add(product.Value);
                    break;

                case ParseWarehouse warehouse:
                    warehouses.Add(warehouse.Value);
                    break;

                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new MixedImportResult(
            products,
            warehouses,
            errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(
            Separator,
            StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 4 }
                => new ParseFailed(
                    $"очікую 4 колонки, отримав {parts.Length}"),

            ["P", _, "", _]
                => new ParseFailed("назва товару порожня"),

            ["P", _, _, var price]
                when !decimal.TryParse(
                    price,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal p) || p < 0
                => new ParseFailed(
                    $"ціна '{price}' не є коректним невід'ємним числом"),

            ["P", var id, var name, var price]
                => new ParseProduct(
                    new ProductDto(
                        id,
                        name,
                        decimal.Parse(
                            price,
                            CultureInfo.InvariantCulture))),

            ["W", _, "", _] or ["W", _, _, ""]
                => new ParseFailed(
                    "назва складу або місто порожні"),

            ["W", var id, var name, var city]
                => new ParseWarehouse(
                    new WarehouseDto(id, name, city)),

            [var type, ..]
                => new ParseFailed(
                    $"невідомий тип запису '{type}'"),

            _ => new ParseFailed("некоректний рядок")
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseProduct(
        ProductDto Value) : ParseOutcome;

    private sealed record ParseWarehouse(
        WarehouseDto Value) : ParseOutcome;

    private sealed record ParseFailed(
        string Reason) : ParseOutcome;
}