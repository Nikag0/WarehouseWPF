using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.DTO
{
    public record StockCsvDto(
        Guid Id,
        string Article,
        string Name,
        string Manufacturer,
        string Rack,
        string Cell,
        int Quantity);
}
