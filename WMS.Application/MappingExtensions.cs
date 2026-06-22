using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Application
{
    public static class MappingExtensions
    {
        public static ViewItemDTO ToViewItemDto(this Stock s)
        {
            return new ViewItemDTO(
                s.ComponentId,
                s.Component?.Article ?? string.Empty,
                s.Component?.Name ?? string.Empty,
                s.Component?.Manufacturer ?? string.Empty,
                s.RackId,
                s.Rack?.RackCode ?? string.Empty,
                s.Rack != null ? LocationFormatter.CodeToDisplay(s.Rack.Column, s.Rack.Row) : string.Empty,
                s.CellId,
                s.Cell?.CellCode ?? string.Empty,
                s.Cell != null ? LocationFormatter.CodeToDisplay(s.Cell.Column, s.Cell.Row) : string.Empty,
                s.Quantity,
                0
            );
        }

        public static ViewItemDTO ToViewItemDto(this Component c)
        {
            return new ViewItemDTO(
                c.Id,
                c.Article ?? string.Empty,
                c.Name ?? string.Empty,
                c.Manufacturer ?? string.Empty,
                Guid.Empty,
                string.Empty,
                string.Empty,
                Guid.Empty,
                string.Empty,
                string.Empty,
                0,
                0
            );
        }
    }
}
