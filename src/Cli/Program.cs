using Core.Dto;
using Core.Import;
using System.Text.Encodings.Web;
using System.Text.Json;

bool jsonOutput = args.Contains("--json");

string path = args.Length > 0 && args[0] != "--json"
    ? args[0]
    : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<ProductDto> result = ProductCsvImporter.Load(path);

if (jsonOutput)
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    Console.WriteLine(JsonSerializer.Serialize(result, options));
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
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
    {
        Console.WriteLine($"! {error}");
    }
}

return 0;