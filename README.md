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



## Лабораторна робота №4

### Доменна модель та інваріанти

Доменні сутності **Order** і **OrderLine** розташовані в `src/Core/Domain`. Замовлення містить колекцію рядків.

- Конструктори приватні, об'єкти створюються лише через фабричні методи `Order.Create` і `OrderLine.Create`.
- Стан замовлення змінюється лише через методи `AddLine`, `Confirm` і `Cancel`.
- Колекція рядків назовні доступна лише для читання (`IReadOnlyList` через `AsReadOnly()`), а `Status` має приватний сеттер.
- Усі перевірки виконуються до створення об'єкта або зміни стану, тому стан не псується при помилці.

### Зв'язок сутностей

`Order` (1) → (0..*) `OrderLine`. Рядок не існує окремо від замовлення: додати його можна лише через `Order.AddLine`, який сам викликає `OrderLine.Create`.

### Інваріанти

| Правило | Виняток | Метод |
|---|---|---|
| Номер замовлення не порожній | ArgumentException | Order.Create |
| Ідентифікатор покупця не порожній | ArgumentException | Order.Create |
| ID і назва товару в рядку не порожні | ArgumentException | OrderLine.Create |
| Ціна ≥ 0 | ArgumentOutOfRangeException | OrderLine.Create |
| Кількість > 0 | ArgumentOutOfRangeException | OrderLine.Create |
| Рядки додаються лише до чернетки (Draft) | InvalidOperationException | Order.AddLine |
| Не підтверджувати порожнє замовлення | InvalidOperationException | Order.Confirm |
| Лише допустимі переходи статусу (повторне підтвердження заборонене) | InvalidOperationException | Order.Confirm, Order.Cancel |
| Статус у DTO має бути відомим | ArgumentException | Order.FromDto |
| Не більше 5 непідтверджених замовлень у покупця | InvalidOperationException | OrderService.CreateOrder |

Перелік інваріантів списком:

- Номер замовлення не може бути порожнім (`Order.Create`, `ArgumentException`).
- Ідентифікатор покупця не може бути порожнім (`Order.Create`, `ArgumentException`).
- ID товару в рядку не може бути порожнім (`OrderLine.Create`, `ArgumentException`).
- Назва товару в рядку не може бути порожньою (`OrderLine.Create`, `ArgumentException`).
- Ціна не може бути від'ємною (`OrderLine.Create`, `ArgumentOutOfRangeException`).
- Кількість у рядку має бути більшою за нуль (`OrderLine.Create`, `ArgumentOutOfRangeException`).
- Рядки можна додавати лише до чернетки, тобто не до підтвердженого чи скасованого замовлення (`Order.AddLine`, `InvalidOperationException`).
- Порожнє замовлення не можна підтвердити (`Order.Confirm`, `InvalidOperationException`).
- Замовлення не можна підтвердити повторно, а скасоване не можна змінити (`Order.Confirm`, `Order.Cancel`, `InvalidOperationException`).
- Статус у DTO має бути одним із `Draft`, `Confirmed`, `Cancelled` (`Order.FromDto`, `ArgumentException`).
- Покупець може мати не більше 5 непідтверджених замовлень (`OrderService.CreateOrder`, `InvalidOperationException`).

Останні три правила з'явилися в додаткових завданнях 2 і 3.

### DTO і сутності

`OrderDto` та `OrderLineDto` (`src/Core/Dto`) — позиційні record, як `ProductDto` з лаби 3. Вони лише переносять дані й не гарантують їх коректності. Сутність захищає правила, тому створити некоректний об'єкт неможливо. Records лаби 3 залишились без змін.

`Order.ToDto` перетворює сутність у DTO. `Order.FromDto` відновлює замовлення тільки через `Create`, `AddLine`, `Confirm` і `Cancel` (статус відновлюється в кінці), а не через приватний конструктор, тому пошкоджені дані відхиляються тими самими інваріантами.

### Запуск

```bash
dotnet run --project src/Cli -f net10.0 -- --domain
```

Сценарій 1: створення замовлення, два рядки, підтвердження.

Сценарій 2: 10 порушень, кожен інваріант спрацьовує хоча б раз. Кожне порушення перехоплюється в try/catch, виводиться тип винятку і `ex.Message` (без stack trace). Наприкінці виводиться стан після відмов, який не змінився.

Сценарій 3: `order.ToDto()` серіалізується в JSON (як у режимі `--json` лаби 3), потім JSON → `OrderDto` → `Order.FromDto` відновлює таке саме замовлення. Той самий JSON із кількістю 0 у рядку відхиляється тим самим інваріантом, що й `AddLine`.

Далі у виводі йдуть розділи трьох додаткових завдань (пункти 12–16).

### Самоперевірка

- `order.Status = ...;` і `order.IsConfirmed = true;` поза класом не компілюються (приватний сеттер / лише обчислення).
- `order.Id = "...";` не компілюється (властивість лише для читання).
- `new Order(...)` і `new OrderLine(...)` поза класом не компілюються (приватні конструктори).
- `order.Lines.Add(...)` не компілюється (`IReadOnlyList` не має `Add`).
- У `src/Core/Domain` немає викликів `Console.` і `File.`, а Core не посилається на Cli.

### Додаткове завдання 1 — ImportResult → сутності

`OrderLineImport.ToEntities` (`src/Core/Domain/OrderLineImport.cs`) бере `ImportResult<OrderLineDto>` (формат лаби 3) і пробує перетворити кожен DTO на `OrderLine` через `OrderLine.Create`. Рядки, що пройшли перевірки, потрапляють у список сутностей, а ті, що порушують інваріанти, у список помилок з номером запису. Помилки читання файлу зберігаються. Результат — `ImportResult<OrderLine>`, тобто та сама ідея «дані + помилки». Domain лише збирає тексти помилок, виводить їх Cli.

У демо з 4 записів прийнято 2, відхилено 2 (від'ємна ціна, нульова кількість).

### Додаткове завдання 2 — правило на дві сутності

Правило: покупець може мати не більше 5 непідтверджених замовлень (чернеток). Його перевіряє `OrderService.CreateOrder` (`src/Core/Services/OrderService.cs`): спочатку `Order.Create` перевіряє аргументи, потім рахуються чернетки цього покупця, і якщо їх уже 5, виникає `InvalidOperationException`.

Чому в сервісі, а не в `Order`: одне замовлення знає лише свої рядки і свій стан, а для перевірки потрібні всі замовлення покупця. Якби правило було в сутності, вона мала б зберігати чужі замовлення й залежати від того, де вони зберігаються. Сервіс отримує список ззовні (на тижні 5 його дасть сховище), а сутності відповідають лише за власні інваріанти.

У демо: 5 чернеток створено, шосте відхилено (пункт 12), після підтвердження одного замовлення нове створюється, інших покупців правило не зачіпає.

### Додаткове завдання 3 — статус замовлення

`bool IsConfirmed` замінено на `enum OrderStatus { Draft, Confirmed, Cancelled }` (`src/Core/Domain/OrderStatus.cs`). `Status` має приватний сеттер і змінюється лише методами `Confirm()` і `Cancel()`. `IsConfirmed` залишено як обчислювану властивість (`Status == Confirmed`).

Допустимі переходи описані через switch expression у `Order.CanTransition`:

| Звідки | Куди | Дозволено |
|---|---|---|
| Draft | Confirmed | так (якщо є хоча б один рядок) |
| Draft | Cancelled | так |
| Confirmed | Cancelled | так |
| Confirmed | Draft | ні |
| Cancelled | будь-який | ні (кінцевий стан) |
| будь-який | той самий | ні |

`OrderDto` зберігає статус рядком. `Order.FromDto` розбирає його через switch expression: невідомий статус дає `ArgumentException`, а потрібний стан відновлюється викликами `Confirm()` / `Cancel()`. `OrderService` рахує лише чернетки, тому скасовані замовлення не займають ліміт.

У демо: таблиця переходів, скасування чернетки й підтвердженого замовлення, 4 заборонені дії (пункти 13–16) і відновлення скасованого замовлення через `ToDto` → `FromDto`.







## Лабораторна робота 5

### Сервісний шар: інтерфейс, два сховища, ручний DI

- Інтерфейс сховища: `IOrderStore` (`src/Core/Abstractions/IOrderStore.cs`) — методи `List`, `GetById`, `Add`, `Update`, `Remove`.
- Реалізації:
  - `InMemoryOrderStore` (`src/Core/Storage/InMemoryOrderStore.cs`) — `Dictionary` у пам'яті, початкові дані з `SampleData`, після виходу дані зникають;
  - `FileOrderStore` (`src/Core/Storage/FileOrderStore.cs`) — JSON-файл `data/orders.json` у папці запуску програми, кеш у пам'яті, читання при першому зверненні, запис після кожної зміни. На диск пишуться `OrderDto` через `ToDto` / `FromDto`.
- Сервіс: `OrderService` (`src/Core/Services/OrderService.cs`) — отримує `IOrderStore` через конструктор; операції `CreateOrder`, `AddLine`, `Confirm`, `All`, `Find`; правило «не більше 5 чернеток у покупця».
- Тестові дані: `SampleData.Orders()` — 16 замовлень.

Схема залежностей:

```
Cli (Program.cs) → OrderService → IOrderStore → InMemoryOrderStore | FileOrderStore
```

### Composition root (Program.cs)

```csharp
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");

IOrderStore store = useFile
    ? new FileOrderStore(dataPath)
    : new InMemoryOrderStore(SampleData.Orders());

var service = new OrderService(store);
```

Конкретні класи сховищ створюються лише тут. `Microsoft.Extensions.DependencyInjection` не підключено.

### Запуск

```bash
dotnet run --project src/Cli -f net10.0 -- --store   # сховище в пам'яті
dotnet run --project src/Cli -f net10.0 -- --file    # файлове сховище
```

Сценарій: створення замовлення, додавання двох рядків, підтвердження, список усіх замовлень, пошук за id, сценарій відмови (дубль id і неіснуюче замовлення) з повідомленням у `try/catch`.

### Самоперевірка

- пошук `FileOrderStore` у `src/Core` — лише у файлі реалізації; `Console.` у Core — нічого;
- два запуски з `--file` дають 1, потім 2 замовлення (накопичення);
- з закоментованим `FileOrderStore` проєкт Core компілюється;
- параметр конструктора `OrderService` — інтерфейс `IOrderStore`.