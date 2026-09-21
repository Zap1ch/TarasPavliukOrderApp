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

### Структура solution

У solution знаходяться два проєкти:

- `src/Cli` — консольний застосунок;
- `src/Core` — бібліотека класів.

`Cli` підключає `Core` через `ProjectReference`.

### Збірка і запуск

```bash
dotnet build
dotnet build src/Core/Core.csproj
dotnet run --project src/Cli
```

### Multi-targeting

`Core` збирається для:

```text
net8.0
net10.0
```

У `Core.csproj`:

```xml
<TargetFrameworks>net8.0;net10.0</TargetFrameworks>
```

### Публікація

Self-contained для Windows:

```bash
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true
```

Framework-dependent для Windows:

```bash
dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained false
```

Self-contained для Linux:

```bash
dotnet publish src/Cli -c Release -r linux-x64 -f net10.0 --self-contained true
```

### Порівняння

| RID | Режим | Розмір publish | Потрібен runtime |
|---|---|---:|---|
| win-x64 | self-contained | 78 MB | ні |
| win-x64 | framework-dependent | 229 KB | так (.NET 10) |
| linux-x64 | self-contained | 80 MB | ні |

### Подальша структура Core

У наступних лабораторних планується використання каталогів:

- `Core/Dto`
- `Core/Domain`
- `Core/Storage`