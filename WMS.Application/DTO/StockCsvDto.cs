using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.DTO
{
    // property: — атрибут вешается на свойство record, а не на параметр конструктора, иначе CsvHelper его не видит
    public record StockCsvDto(
        [property: Name("Артикул")] string Article,
        [property: Name("Наименование")] string Name,
        [property: Name("Производитель")] string Manufacturer,
        [property: Name("Стеллаж")] string Rack,
        [property: Name("Ячейка")] string Cell,
        [property: Name("Количество")] int Quantity
    );
}
