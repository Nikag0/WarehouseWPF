using CsvHelper.Configuration.Attributes;

namespace WMS.Application.DTO
{
    // property: — атрибут вешается на свойство record, а не на параметр конструктора, иначе CsvHelper его не видит
    public record HistoryCsvDto(
        [property: Name("Дата"), Format("dd.MM.yyyy HH:mm")] DateTime OccurredAt,
        [property: Name("Операция")] string Type,
        [property: Name("Компонент")] string ComponentName,
        [property: Name("Стеллаж")] string Rack,
        [property: Name("Ячейка")] string Cell,
        [property: Name("Было")] int PreviousQuantity,
        [property: Name("Изменение")] int QuantityChanged,
        [property: Name("Стало")] int NewQuantity,
        [property: Name("Оператор")] string Operator,
        [property: Name("Комментарий")] string Comment
    );
}
