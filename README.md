# TarasPavliukOrderApp

Проєкт з дисципліни «Крос-платформне програмування».

## Лабораторна робота 1

### Предметна область

Предметна область: **Замовлення**.

Основні сутності:
- замовлення;
- покупець;
- товар;
- рядок замовлення.

### Збірка і запуск

```bash
dotnet build
dotnet run --project src/Cli
```

Запуск з JSON-виводом:

```bash
dotnet run --project src/Cli -- --json
```

### Середовище

- .NET SDK 10.0
- Windows x64
- RID: win-x64
- Visual Studio Code

### Self-contained публікація

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
```

Розміри:

- win-x64 — 78 MB
- linux-x64 — 80 MB


## Лабораторна робота 2

### Перевірка solution

```bash
dotnet sln list
```

У solution знаходяться два проєкти:

- `src/Cli` — консольний застосунок;
- `src/Core` — бібліотека класів.

### ProjectReference

```bash
cat src/Cli/Cli.csproj
```

`Cli` використовує `Core` через:

```xml
<ProjectReference Include="..\Core\Core.csproj" />
```

### Program.cs

```bash
cat src/Cli/Program.cs
```

`Program.cs` отримує дані з `Core` та виводить їх у консоль.

### Запуск

Без вказаного TFM:

```bash
dotnet run --project src/Cli
```

Оскільки використовується multi-targeting, для запуску можна явно вказати TFM:

```bash
dotnet run --project src/Cli -f net10.0
```

### JSON-вивід

```bash
dotnet run --project src/Cli -f net10.0 -- --json
```

### Запуск з каталогу publish

```bash
cd src/Cli/bin/Release/net10.0/win-x64/publish
./Cli.exe
```

---

### Публікація та порівняння

#### win-x64 self-contained

```bash
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true
du -sh src/Cli/bin/Release/net10.0/win-x64/publish
find src/Cli/bin/Release/net10.0/win-x64/publish -type f | wc -l
```

#### win-x64 framework-dependent

```bash
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained false
du -sh src/Cli/bin/Release/net10.0/win-x64/publish
find src/Cli/bin/Release/net10.0/win-x64/publish -type f | wc -l
```

#### linux-x64 self-contained

```bash
dotnet publish src/Cli -c Release -r linux-x64 -f net10.0 --self-contained true
du -sh src/Cli/bin/Release/net10.0/linux-x64/publish
find src/Cli/bin/Release/net10.0/linux-x64/publish -type f | wc -l
```

| RID | Режим | Розмір publish | Кількість файлів | Потрібен runtime |
|---|---|---:|---:|---|
| win-x64 | self-contained | 78 MB | 194 | ні |
| win-x64 | framework-dependent | 229 KB | 7 | так (.NET 10) |
| linux-x64 | self-contained | 80 MB | 194 | ні |

Self-contained містить .NET Runtime, тому має більший розмір і не потребує окремо встановленого Runtime.

Framework-dependent не містить .NET Runtime, тому має значно менший розмір, але потребує встановленого .NET 10.

---

### Додаткові завдання

#### PublishSingleFile

```bash
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -p:PublishSingleFile=true -o src/Cli/bin/Release/net10.0/win-x64/publish-single
du -sh src/Cli/bin/Release/net10.0/win-x64/publish-single
find src/Cli/bin/Release/net10.0/win-x64/publish-single -type f | wc -l
ls src/Cli/bin/Release/net10.0/win-x64/publish-single
```

#### PublishTrimmed

```bash
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -p:PublishTrimmed=true -o src/Cli/bin/Release/net10.0/win-x64/publish-trimmed
du -sh src/Cli/bin/Release/net10.0/win-x64/publish-trimmed
find src/Cli/bin/Release/net10.0/win-x64/publish-trimmed -type f | wc -l
ls src/Cli/bin/Release/net10.0/win-x64/publish-trimmed
```

| Варіант | Розмір | Кількість файлів |
|---|---:|---:|
| PublishSingleFile | 71 MB | 3 |
| PublishTrimmed | 20 MB | 35 |

`PublishSingleFile` зменшує кількість окремих файлів, об'єднуючи основні компоненти застосунку в один виконуваний файл.

`PublishTrimmed` видаляє невикористаний код і зменшує загальний розмір збірки. При trimmed-публікації було отримано 2 попередження `IL2026`, пов'язані з використанням JSON-серіалізації при trimming.

---

### Multi-targeting

У `Core.csproj` і `Cli.csproj` використовуються:

```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```
Перевірка двох TFM:

```bash
dotnet run --project src/Cli -f net8.0
dotnet run --project src/Cli -f net10.0
```

Для умовної компіляції використовується:

```csharp
#if NET10_0_OR_GREATER
    public const string BuildNote = "збірка під net10.0";
#else
    public const string BuildNote = "збірка під net8.0";
#endif
```




## Лабораторна робота №3

### Основний запуск

```bash
dotnet run --project src/Cli -f net10.0
```

Імпорт даних із `data/sample.csv`. Програма виводить завантажені товари, помилкові рядки та статистику імпорту.

### Основний запуск з JSON-виводом

```bash
dotnet run --project src/Cli -f net10.0 -- --json
```

Імпорт даних із `data/sample.csv` з виведенням результату у форматі JSON.

### Запуск із вказаним CSV-файлом

```bash
dotnet run --project src/Cli -f net10.0 -- data/sample.csv
```

Шлях до CSV-файлу передається через аргумент командного рядка.

### Додаткове завдання 1 — імпорт JSON

```bash
dotnet run --project src/Cli -f net10.0 -- data/sample.json
```

Імпорт товарів із `data/sample.json`. Імпортер автоматично вибирається за розширенням файлу.

```bash
dotnet run --project src/Cli -f net10.0 -- data/sample.json --json
```

Імпорт із `data/sample.json` та виведення результату у форматі JSON.

### Додаткове завдання 2 — різнорідні записи

```bash
dotnet run --project src/Cli -f net10.0 -- --mixed
```

Імпорт із `data/sample-mixed.csv`, де записи з префіксом `P` є товарами, а записи з префіксом `W` — складами.

```bash
dotnet run --project src/Cli -f net10.0 -- --mixed --json
```

Імпорт різнорідних записів із виведенням результату у форматі JSON.

### Додаткове завдання 3 — статистика імпорту

Після звичайного та змішаного імпорту програма виводить статистику:

```text
Статистика: усього 13, прийнято 10, пропущено 3, помилок 23.1%
```