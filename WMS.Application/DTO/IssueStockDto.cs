using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.DTO
{
    public record IssueStockDto
    (
        Guid StockId,
        int QuantityChanged,
         Guid RackId,
        string RackName,
        Guid CellId,
        string CellName,
        Guid ComponentId,
        string ComponentName
    );
}
