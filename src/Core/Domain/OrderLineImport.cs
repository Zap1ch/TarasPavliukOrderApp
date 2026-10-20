using Core.Dto;

namespace Core.Domain;

// Додаткове 1: перетворює імпортовані DTO на сутності.
// Що пройшло інваріанти -> у список рядків, що ні -> у список помилок.
public static class OrderLineImport
{
    public static ImportResult<OrderLine> ToEntities(ImportResult<OrderLineDto> import)
    {
        List<OrderLine> lines = new List<OrderLine>();

        // Помилки, які були ще під час читання файлу.
        List<string> errors = new List<string>(import.Errors);

        int number = 1;
        foreach (OrderLineDto dto in import.Items)
        {
            try
            {
                OrderLine line = OrderLine.Create(dto.ProductId, dto.ProductName, dto.Price, dto.Quantity);
                lines.Add(line);
            }
            catch (ArgumentException ex)
            {
                // ArgumentOutOfRangeException теж сюди потрапляє (це його нащадок).
                errors.Add($"Запис {number}: {ex.Message}");
            }

            number++;
        }

        return new ImportResult<OrderLine>(lines, errors);
    }
}