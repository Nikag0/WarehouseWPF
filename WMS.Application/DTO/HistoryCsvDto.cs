using CsvHelper.Configuration.Attributes;
using WMS.Domain;

namespace WMS.Application.DTO
{
    // property: — атрибут вешается на свойство record, а не на параметр конструктора, иначе CsvHelper его не видит
    public record HistoryCsvDto(
        [property: Name("Дата"), Format("dd.MM.yyyy HH:mm")] DateTime OccurredAt,
        [property: Name("Операция")] string Type,
        [property: Name("Компонент")] string ComponentName,
        [property: Name("Стеллаж")] string Rack,
        [property: Name("Ячейка")] string Cell,
        [property: Name("Было"), Format(QuantityRules.DisplayFormat)] decimal PreviousQuantity,
        [property: Name("Изменение"), Format(QuantityRules.DisplayFormat)] decimal QuantityChanged,
        [property: Name("Стало"), Format(QuantityRules.DisplayFormat)] decimal NewQuantity,
        [property: Name("Оператор")] string Operator,
        [property: Name("Комментарий")] string Comment
    );
}
