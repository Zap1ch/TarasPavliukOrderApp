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
  - `FileOrderStore` (`src/Core/Storage/FileOrderStore.cs`) — JSON-файл `data/orders.json` у папці запуску програми (`src/Cli/bin/Debug/net10.0/data/orders.json`), кеш у пам'яті, читання при першому зверненні, запис після кожної зміни. На диск пишуться `OrderDto` через `ToDto` / `FromDto`.
- Сервіс: `OrderService` (`src/Core/Services/OrderService.cs`) — отримує `IOrderStore` через конструктор; операції `CreateOrder`, `AddLine`, `Confirm`, `All`, `Find`, `Search`; правило «не більше 5 чернеток у покупця».
- Тестові дані: `SampleData.Orders()` — 16 замовлень.

Схема залежностей:

```
Cli (Program.cs) → OrderService → IOrderStore → InMemoryOrderStore | FileOrderStore
```

### Composition root

Основне завдання — вибір сховища тернарним оператором у `Program.cs`:

```csharp
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");

IOrderStore store = useFile
    ? new FileOrderStore(dataPath)
    : new InMemoryOrderStore(SampleData.Orders());

var service = new OrderService(store);
```

Після додаткового завдання 3 цей вибір винесено у фабрику `StoreFactory` (теж у Cli), а в `Program.cs` залишився один рядок:

```csharp
IOrderStore store = StoreFactory.Create(args);
var service = new OrderService(store);
```

Конкретні класи сховищ створюються лише в Cli (`Program.cs` / `StoreFactory.cs`), Core про них у сервісі нічого не знає. `Microsoft.Extensions.DependencyInjection` не підключено.

### Запуск

```bash
dotnet run --project src/Cli -f net10.0 -- --store   # сховище в пам'яті
dotnet run --project src/Cli -f net10.0 -- --file    # файлове сховище
```

Сценарій: створення замовлення, додавання двох рядків, підтвердження, список усіх замовлень, пошук за id, сценарій відмови (дубль id і неіснуюче замовлення) з повідомленням у `try/catch`.

- `--store`: щоразу 17 замовлень (16 тестових + 1 нове), після виходу нове замовлення зникає;
- `--file`: кожен запуск додає до файлу одне замовлення, дані зберігаються між запусками.

### Самоперевірка

- пошук `FileOrderStore` у `src/Core` — лише у файлі реалізації; `Console.` у Core — нічого;
- два запуски з `--file` дають накопичення, а не обнулення файлу;
- з закоментованим `FileOrderStore` проєкт Core компілюється;
- параметр конструктора `OrderService` — інтерфейс `IOrderStore`.

### Додаткові завдання

**1. Декоратор `CachingOrderStore`** (`src/Core/Storage/CachingOrderStore.cs`) — реалізує `IOrderStore` і приймає інший `IOrderStore` у конструкторі. Кешує результат `List()`, після `Add` / `Update` / `Remove` кеш скидається. Працює з обома сховищами; `OrderService` не змінювався.

```bash
dotnet run --project src/Cli -f net10.0 -- --store --cache
```

У кінці виводиться статистика: `Кеш: відповідей з кешу 1, звернень до сховища 2`.

**2. Пошук `OrderService.Search(Func<Order, bool> filter)`** — повертає замовлення, для яких виконується передана умова. У демо три пошуки: чернетки, замовлення покупця C-100, сума понад 10000.

```bash
dotnet run --project src/Cli -f net10.0 -- --store --search
```

**3. Фабрика `StoreFactory.Create(args)`** (`src/Cli/StoreFactory.cs`) — вибір сховища винесено з `Program.cs` у фабрику в Cli: з `--file` повертає `FileOrderStore`, інакше `InMemoryOrderStore`.




## Лабораторна робота 6

### Доменні події, делегати Action/Func, лямбди

`OrderService` повідомляє про зміни замовлень через подію `Changed`. Core лише піднімає подію, а що з нею робити (консоль, файл), вирішують підписники в Cli.

**Нові / змінені файли:**
- `src/Core/Events/OrderChangeKind.cs` — вид зміни: `OrderCreated`, `LineAdded`, `OrderConfirmed`;
- `src/Core/Events/OrderChangedEventArgs.cs` — дані події: `OrderId`, `Kind`, `LinesCount`, `Total`, `At`;
- `src/Core/Services/OrderService.cs` — `event EventHandler<OrderChangedEventArgs>? Changed` і метод `OnChanged`;
- `src/Cli/EventsDemo.cs` — підписники, відписка, сценарій;
- `src/Cli/Program.cs` — режими `--events`, `--sinks`, `--report`.

**Правила:**
- подія піднімається через `Changed?.Invoke(...)` в кінці `CreateOrder`, `AddLine`, `Confirm` — лише після успішної зміни; якщо інваріант кинув виняток, події немає;
- у Core немає `Console` і запису в лог-файл.

**Підписники (Cli):**
1. локальний метод `OnOrderChanged` — рядок у консоль і в `logs/app.log` (шлях через `Path.Combine(AppContext.BaseDirectory, "logs", "app.log")`, каталог створюється кодом);
2. лямбда `(_, _) => changes++` — лічильник подій (замикання).

Після `service.Changed -= OnOrderChanged;` операції виконуються, але рядків у консолі й лозі немає, а лічильник рахує далі.

### Запуск

```bash
dotnet run --project src/Cli -f net10.0 -- --events
```
Сховище в пам'яті. Створення замовлення, 2 рядки, підтвердження → 4 рядки подій у консолі й `app.log`; сценарій відмови (від'ємна кількість, `O-999`) — помилка є, події немає; відписка `-=` → нове замовлення без рядка в лозі; пошук через `Func`: `Search(o => o.Total < 1000m)`.

```bash
dotnet run --project src/Cli -f net10.0 -- --events --file
```
Те саме з файловим сховищем.

Лог: `src/Cli/bin/Debug/net10.0/logs/app.log`
```
23:57:22 OrderCreated   89f0e233 рядків: 0 → сума 0,00
23:57:22 LineAdded      89f0e233 рядків: 1 → сума 500,00
23:57:22 LineAdded      89f0e233 рядків: 2 → сума 5500,00
23:57:22 OrderConfirmed 89f0e233 рядків: 2 → сума 5500,00
```

### Додаткові завдання

```bash
dotnet run --project src/Cli -f net10.0 -- --sinks
```
Сповіщення через інтерфейс `INotificationSink` (Core) з реалізаціями `ConsoleSink` і `FileSink` (Cli, `logs/sinks.log`), які передаються в `OrderService` через конструктор. На відміну від події, одержувачі видно в конструкторі, але змінити їх після створення сервісу не можна.

```bash
dotnet run --project src/Cli -f net10.0 -- --report
```
`ProductCsvImporter.Load(path, Action<string>? onSkipped)`: кожен пропущений рядок `data/sample.csv` одразу виводиться як `[пропущено] ...`. Без `--report` імпорт працює як у лабі 3.





## Лабораторна робота 7

### Звіти через LINQ: фільтри, проєкції, групування, топ-N, агрегати

Усі запити лежать у Core, у статичному класі `ReportService` (`src/Core/Reports/ReportService.cs`). Кожен звіт приймає `IEnumerable<Order>`, а не сховище, і повертає готовий результат (`IReadOnlyList` DTO або кортеж), а не сутності `Order`. Cli (`src/Cli/ReportsDemo.cs`) лише вибирає звіт за командою і друкує результат; у `Program.cs` немає жодного `Where` чи `GroupBy`.

**Нові / змінені файли:**
- `src/Core/Reports/OrderReportLine.cs` — DTO рядка про замовлення: `OrderId`, `CustomerId`, `Status`, `LinesCount`, `Total`;
- `src/Core/Reports/GroupSummary.cs` — DTO групи: `Key`, `Count`, `TotalSum`;
- `src/Core/Reports/ReportService.cs` — усі звіти;
- `src/Core/SampleData.cs` — розширено до 20 замовлень;
- `src/Cli/ReportsDemo.cs` — команди звітів і вивід;
- `src/Cli/Program.cs` — режим для команд, що починаються з `report-`.

**Звіти основного завдання:**
- `ExpensiveOrders` — замовлення дорожчі за N: `Where`, `OrderByDescending`, `ThenBy`, `Select`;
- `TopCustomers` — топ-5 клієнтів за сумою: `GroupBy`, `Select`, `OrderByDescending`, `ThenBy`, `Take`;
- `ByStatus` — кількість і сума замовлень за статусом: `GroupBy`, `Select`, `OrderByDescending`, `ThenBy`;
- `Totals` — кількість, загальна сума, середній чек, максимальне замовлення: `Count`, `Sum`, `Average`, `Max`; для порожнього джерела повертає нулі.

У топ-N після основного ключа стоїть `ThenBy` за кодом, тому порядок однаковий за кожного запуску.

### Запуск

```bash
dotnet run --project src/Cli -f net10.0 -- report-top          # топ-5 клієнтів за сумою
dotnet run --project src/Cli -f net10.0 -- report-expensive    # замовлення дорожчі за 10000
dotnet run --project src/Cli -f net10.0 -- report-groups       # групування за статусом
dotnet run --project src/Cli -f net10.0 -- report-sum          # підсумки
dotnet run --project src/Cli -f net10.0 -- report-deferred     # демо відкладеного виконання
```

Додатково: `--file` бере дані з файлового сховища, `--empty` передає звітам порожній список (перевірка, що нічого не падає).

```
Топ-5 клієнтів за сумою замовлень:
  C-101      замовлень:   3   сума:   34650,00
  C-105      замовлень:   2   сума:   30100,00
  C-108      замовлень:   1   сума:   28900,00
  C-104      замовлень:   2   сума:   13500,00
  C-100      замовлень:   3   сума:   10100,00
Замовлень: 20, загальна сума: 147500,00, середній чек: 7375,00, максимальне: 28900,00
```

### Відкладене виконання

Запит `source.Where(o => o.Total > 10000m)` у моєму коді в момент оголошення не виконується: програма лише запам'ятовує умову. Виконується він під час кожного виклику `Count()` або `ToList()`, і щоразу заново перебирає джерело. Тому перший `Count()` дав 4, а після додавання в джерело ще одного замовлення на 28000 той самий запит дав 5. Після `ToList()` результат зафіксовано в окремому списку: коли джерело очистили, запит дав 0, а знімок і далі містить 5 замовлень.

### Додаткові завдання

```bash
dotnet run --project src/Cli -f net10.0 -- report-customers      # 1. GroupJoin клієнтів із замовленнями
dotnet run --project src/Cli -f net10.0 -- report-top-orders     # 2. топ-5 замовлень через TopBy
dotnet run --project src/Cli -f net10.0 -- report-top-named      # 2. топ-5 клієнтів з іменами через TopBy
dotnet run --project src/Cli -f net10.0 -- report-groups-query   # 3. групування в синтаксисі запитів
dotnet run --project src/Cli -f net10.0 -c Release -- report-bench   # 4. Stopwatch на 100 000 замовлень
```

1. **GroupJoin.** `CustomersWithOrders` з'єднує клієнтів (`SampleData.Customers()`, `CustomerDto`) із замовленнями за кодом клієнта і будує DTO `CustomerReport`. На відміну від `Join`, клієнт без замовлень (C-109) теж потрапляє у звіт із нулями.
2. **Метод-розширення.** `TopBy<T>(this IEnumerable<T>, Func<T, decimal>, Func<T, string>, int)` у `EnumerableExtensions.cs` — топ-N з другим ключем; використано у звітах `TopOrders` і `TopNamedCustomers`.
3. **Синтаксис запитів.** `ByStatusQuery` записано через `from ... group ... by ... into ... orderby ... select` і дає той самий результат, що `ByStatus`. Синтаксис запитів читається як речення і наочніший для групування; методний синтаксис коротший для простих ланцюжків і дозволяє вільно використовувати `Take`, `ToList` чи `TopBy`, тому в основних звітах залишено його.
4. **Stopwatch.** На 100 000 згенерованих замовлень (`OrderGenerator`) варіант без `ToList` у середині ланцюжка виконувався в середньому за ~19 мс, а з `ToList` у середині (усі замовлення спершу перетворюються на DTO) — за 31–49 мс. Фільтр варто ставити якомога раніше, а `ToList` — у кінці.