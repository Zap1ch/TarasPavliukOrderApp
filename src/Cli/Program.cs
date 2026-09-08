using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Encodings.Web;

if (args.Contains("--json"))
{
    var info = new
    {
        Application = "TarasPavliukOrderApp",
        Student = "Павлюк Тарас",
        Group = "ФЕІ-36",
        OSDescription = RuntimeInformation.OSDescription,
        OSVersion = Environment.OSVersion.ToString(),
        ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
        CLRVersion = Environment.Version.ToString(),
        Runtime = RuntimeInformation.FrameworkDescription,
        ApplicationDirectory = AppContext.BaseDirectory,
        CurrentDirectory = Environment.CurrentDirectory,
        Domain = "Замовлення",
        Entities = "замовлення, покупець, товар, рядок замовлення"
    };

    var options = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    Console.WriteLine(JsonSerializer.Serialize(info, options));
}
else
{
    Console.WriteLine("TarasPavliukOrderApp – практикум з крос-платформного програмування");
    Console.WriteLine("Студент: Павлюк Тарас, група ФЕІ-36");
    Console.WriteLine(new string('-', 52));

    Console.WriteLine($"ОС (OSDescription) : {RuntimeInformation.OSDescription}");
    Console.WriteLine($"ОС (Environment) : {Environment.OSVersion}");
    Console.WriteLine($"Архітектура процесу : {RuntimeInformation.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR) : {Environment.Version}");
    Console.WriteLine($"Runtime : {RuntimeInformation.FrameworkDescription}");
    Console.WriteLine($"Каталог застосунку : {AppContext.BaseDirectory}");
    Console.WriteLine($"Поточний каталог : {Environment.CurrentDirectory}");

    Console.WriteLine(new string('-', 52));
    Console.WriteLine("Предметна область: Замовлення (замовлення, покупець, товар, рядок замовлення)");
}