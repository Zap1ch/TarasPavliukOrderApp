using Core.Dto;
using Core.Import;
using System.Text.Encodings.Web;
using System.Text.Json;

bool jsonOutput = args.Contains("--json");
bool mixedMode = args.Contains("--mixed");

string? pathArgument =
    args.FirstOrDefault(arg => !arg.StartsWith("--"));

string path = pathArgument
    ?? (mixedMode
        ? Path.Combine("data", "sample-mixed.csv")
        : Path.Combine("data", "sample.csv"));

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

if (mixedMode)
{
    MixedImportResult mixedResult =
        MixedCsvImporter.Load(path);

    if (jsonOutput)
    {
        Console.WriteLine(
            JsonSerializer.Serialize(mixedResult, jsonOptions));

        return 0;
    }

    Console.WriteLine(
        $"Завантажено товарів: {mixedResult.Products.Count}");

    foreach (ProductDto product in mixedResult.Products.Take(5))
    {
        Console.WriteLine(
            $"P {product.Id,-7} {product.Name,-25} {product.Price,10:F2}");
    }

    Console.WriteLine();

    Console.WriteLine(
        $"Завантажено складів: {mixedResult.Warehouses.Count}");

    foreach (WarehouseDto warehouse in mixedResult.Warehouses.Take(5))
    {
        Console.WriteLine(
            $"W {warehouse.Id,-7} {warehouse.Name,-25} {warehouse.City}");
    }

    if (mixedResult.Errors.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine(
            $"Пропущено рядків: {mixedResult.Errors.Count}");

        foreach (string error in mixedResult.Errors)
        {
            Console.WriteLine($"! {error}");
        }
    }

    int accepted =
        mixedResult.Products.Count +
        mixedResult.Warehouses.Count;

    int total =
        accepted + mixedResult.Errors.Count;

    double errorPercent =
        total == 0
            ? 0
            : mixedResult.Errors.Count * 100.0 / total;

    Console.WriteLine();
    Console.WriteLine(
        $"Статистика: усього {total}, прийнято {accepted}, " +
        $"пропущено {mixedResult.Errors.Count}, помилок {errorPercent:F1}%");

    return 0;
}

ImportResult<ProductDto> result =
    Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".csv" => ProductCsvImporter.Load(path),
        ".json" => ProductJsonImporter.Load(path),

        _ => new ImportResult<ProductDto>(
            [],
            [$"Непідтримуваний формат файлу: {Path.GetExtension(path)}"])
    };

if (jsonOutput)
{
    Console.WriteLine(
        JsonSerializer.Serialize(result, jsonOptions));

    return 0;
}

Console.WriteLine($"Завантажено записів: {result.Items.Count}");

foreach (ProductDto product in result.Items.Take(5))
{
    Console.WriteLine(
        $"{product.Id,-7} {product.Name,-30} {product.Price,10:F2}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine(
        $"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
    {
        Console.WriteLine($"! {error}");
    }
}

int acceptedCount = result.Items.Count;
int totalCount = acceptedCount + result.Errors.Count;

double errorPercentage =
    totalCount == 0
        ? 0
        : result.Errors.Count * 100.0 / totalCount;

Console.WriteLine();
Console.WriteLine(
    $"Статистика: усього {totalCount}, прийнято {acceptedCount}, " +
    $"пропущено {result.Errors.Count}, помилок {errorPercentage:F1}%");

return 0;