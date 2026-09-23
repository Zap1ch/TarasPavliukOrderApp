using Core;
using System.Text.Json;
using System.Text.Encodings.Web;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Contains("--json"))
{
    var info = new
    {
        OS = report.OsDescription,
        Runtime = report.FrameworkDescription,
        Build = EnvironmentInfo.BuildNote,
        Architecture = report.ProcessArchitecture,
        DetectedRid = report.DetectedRid,
        ReportedRid = report.ReportedRid,
        Directory = report.BaseDirectory
    };

    var options = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    Console.WriteLine(JsonSerializer.Serialize(info, options));
}
else
{
    Console.WriteLine("TarasPavliukOrderApp – інформація про середовище");
    Console.WriteLine(new string('-', 52));

    Console.WriteLine($"ОС : {report.OsDescription}");
    Console.WriteLine($"Runtime : {report.FrameworkDescription}");
    Console.WriteLine($"Build : {EnvironmentInfo.BuildNote}");
    Console.WriteLine($"Архітектура : {report.ProcessArchitecture}");
    Console.WriteLine($"RID (визначено) : {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
    Console.WriteLine($"Каталог : {report.BaseDirectory}");
}