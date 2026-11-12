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
        public static StockViewDTO StockToView(this Stock s)
        {
            return new StockViewDTO(
                s.ComponentId,
                s.Component.Article,
                s.Component.Name,
                s.Component.Manufacturer,
                s.Id,
                s.RackId,
                $"{s.Rack.Column}-{s.Rack.Row}",
                CodeToDisplay(s.Rack.Column, s.Rack.Row),
                s.CellId,
                $"{s.Cell.Column}-{s.Cell.Row}",
                CodeToDisplay(s.Cell.Column, s.Cell.Row),
                s.Quantity,
                0
            );
        }

        public static StockCsvDto StockToCsv(this Stock s)
        {
            return new StockCsvDto(
                s.Component.Article,
                s.Component.Name,
                s.Component.Manufacturer,
                CodeToDisplay(s.Rack.Column, s.Rack.Row),
                CodeToDisplay(s.Cell.Column, s.Cell.Row),
                s.Quantity
            );
        }

        public static HistoryCsvDto HistoryToCsv(this OperationHistory o)
        {
            return new HistoryCsvDto(
                // В БД время хранится в UTC, в файл выгружаем местное
                o.OccurredAt.ToLocalTime(),
                OperationToStr(o.Type),
                o.ComponentName,
                o.RackName,
                o.CellName,
                o.PreviousQuantity,
                o.QuantityChanged,
                o.NewQuantity,
                o.Operator,
                o.Comment ?? string.Empty
            );
        }

        public static StockViewDTO ComponentViewtoStockView(this ComponentViewDto c)
        {
            return new StockViewDTO(
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

        public static OperationHystoryViewDto OperationHystoryToViewDTO(this OperationHistory o)
        {
            return new OperationHystoryViewDto(
                o.OccurredAt,
                OperationToStr(o.Type),
                o.ComponentName,
                $"Стеллаж {o.RackName} Ячейка {o.CellName}",
                o.PreviousQuantity,
                o.Type == OperationType.Receipt ? $"+{o.QuantityChanged}" : $"-{Math.Abs(o.QuantityChanged)}",
                o.NewQuantity,
                o.Operator,
                o.Comment ?? string.Empty
            );
        }

        // Значение фильтра истории, при котором показываются операции любого типа
        public const string AllOperationsName = "Все типы";

        // null означает «все типы операций»
        public static OperationType? StrToOperation(string operationName)
        {
            return operationName switch
            {
                "Приёмка" => OperationType.Receipt,
                "Выдача" => OperationType.Issue,
                _ => null
            };
        }

        public static string OperationToStr(OperationType operationType)
        {
            return operationType switch
            {
                OperationType.Receipt => "Приёмка",
                OperationType.Issue => "Выдача",
                _ => operationType.ToString(),
            };
        }

        public static string CodeToDisplay(int column, int row)
        {
            return $"{NumToLetter(row)}-{column}";
        }

        public static string NumToLetter(int number)
        {
            return number switch
            {
                1 => "А",
                2 => "Б",
                3 => "В",
                4 => "Г",
                5 => "Д",
                6 => "Е",
                7 => "Ж",
                8 => "З",
                _ => number.ToString()
            };
        }
    }
}
