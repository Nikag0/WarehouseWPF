using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.DTO
{
    public record IssueItemDto
    (
        Guid StockId,
        int Quantity
    );
}
