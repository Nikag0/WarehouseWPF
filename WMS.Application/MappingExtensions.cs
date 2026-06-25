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
                s.Component.Article,
                s.Component.Name,
                s.Component.Manufacturer,
                s.Id,
                s.RackId,
                $"{s.Rack.Column}-{s.Rack.Row}",
                LocationFormatter.CodeToDisplay(s.Rack.Column, s.Rack.Row),
                s.CellId,
                $"{s.Cell.Column}-{s.Cell.Row}",
                LocationFormatter.CodeToDisplay(s.Cell.Column, s.Cell.Row),
                s.Quantity,
                0
            );
        }

        public static ViewItemDTO ToViewItemDto(this Component c)
        {
            return new ViewItemDTO(
                c.Id,
                c.Article,
                c.Name,
                c.Manufacturer,
                Guid.Empty,
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
