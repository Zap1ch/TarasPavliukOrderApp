# TarasPavliukOrderApp

Наскрізний проєкт з крос-платформного програмування.

## Предметна область

Замовлення.

Сутності: замовлення, покупець, товар, рядок замовлення.

Призначення: оформлення замовлень і підрахунок їх загальної вартості.

## Запуск

Після клонування або завантаження репозиторію відкрити термінал у корені проєкту.

### Git Bash

```bash
dotnet build
dotnet run --project src/Cli
```

### PowerShell

```powershell
dotnet build
dotnet run --project src/Cli
```

## Середовище

- .NET SDK 10.0
- Windows x64
- RID: win-x64
- Редактор: Visual Studio Code

### Вивід у форматі JSON

Для запуску програми з виведенням інформації одним JSON-рядком:

```bash
dotnet run --project src/Cli -- --json
```

## Self-contained публікація

Застосунок було опубліковано у self-contained режимі для двох RID:

- win-x64 — 78 MB
- linux-x64 — 80 MB

Linux-версія вийшла приблизно на 2 MB більшою за Windows-версію.
Обидві версії містять необхідний .NET Runtime і можуть запускатися без окремого встановлення .NET Runtime на відповідній платформі.