using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.DTO;
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

        public static StockCsvDto StockToCsv(this Stock s)
        {
            return new StockCsvDto(
                s.Id,
                s.Component.Article,
                s.Component.Name,
                s.Component.Manufacturer,
                LocationFormatter.CodeToDisplay(s.Rack.Column, s.Rack.Row),
                LocationFormatter.CodeToDisplay(s.Cell.Column, s.Cell.Row),
                s.Quantity
            );
        }

        public static ViewItemDTO ComponentViewtoItemView(this ComponentViewDto c)
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

        public static ComponentEditDto ToComponentDTO(this Component c)
        {
            return new ComponentEditDto(
                c.Id,
                c.Article,
                c.Name,
                c.Manufacturer,
                c.ExpirationDate,
                c.MinQuantity
            );
        }

        public static OperatorDTO ToOperatorDTO(this Operator o)
        {
            return new OperatorDTO(
                o.Id,
                o.Surname,
                o.Name,
                o.Patronymic,
                o.FullName
            );
        }

        public static HistoryDto ToHistoryDTO(this HistoryItem o)
        {
            return new HistoryDto(
                o.Operation.OccurredAt,
                o.Operation.Operator ?? "[Удаленный компонент]",
                LocationFormatter.OperationToStr(o.Operation.Type),
                o.Component?.Name ?? "[Удаленный компонент]",
                o.QuantityBefore,
                o.QuantityAfter,
                o.Operation.Comment ?? string.Empty
            );
        }
    }
}
