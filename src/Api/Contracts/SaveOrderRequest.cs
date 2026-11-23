namespace Api.Contracts;

// Вхідний DTO для POST і PUT: клієнт передає лише покупця і рядки, номер і статус задає сервер.
public sealed record SaveOrderLineRequest(string ProductId, string ProductName, decimal Price, int Quantity);

public sealed record SaveOrderRequest(string Customer, List<SaveOrderLineRequest> Lines)
{
    // Перевіряє запит і збирає помилки по полях: ключ — назва поля, значення — повідомлення.
    public bool TryValidate(out Dictionary<string, string[]> errors)
    {
        var found = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(Customer) || Customer.Length > 50)
            found[nameof(Customer)] = ["Покупець обов'язковий, до 50 символів."];

        // Варіант «Замовлення»: lines не може бути порожнім.
        if (Lines is null || Lines.Count == 0)
        {
            found[nameof(Lines)] = ["Замовлення має містити хоча б один рядок."];
        }
        else
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                var line = Lines[i];
                string prefix = $"Lines[{i}]";   // щоб було видно, в якому рядку помилка

                if (line is null)
                {
                    found[prefix] = ["Рядок замовлення не може бути порожнім."];
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line.ProductId) || line.ProductId.Length > 20)
                    found[$"{prefix}.ProductId"] = ["Код товару обов'язковий, до 20 символів."];

                if (string.IsNullOrWhiteSpace(line.ProductName) || line.ProductName.Length > 200)
                    found[$"{prefix}.ProductName"] = ["Назва товару обов'язкова, до 200 символів."];

                if (line.Price < 0)
                    found[$"{prefix}.Price"] = ["Ціна не може бути від'ємною."];

                if (line.Quantity <= 0)
                    found[$"{prefix}.Quantity"] = ["Кількість має бути більшою за нуль."];
            }
        }

        errors = found;
        return found.Count == 0;
    }
}