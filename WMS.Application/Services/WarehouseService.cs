using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Application.DTO;
using WMS.Application.WarehouseVisualization;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class WarehouseService
    {
        private readonly IRackRepository _rackRepository;

        public WarehouseService(IRackRepository rackRepository)
        {
            _rackRepository = rackRepository;
        }

        public async Task<IReadOnlyList<RackDTO>> GetWarehouseAsync()
        {
            var rackLayouts = await LoadRackLayoutsAsync();
            var cellLayouts = await LoadCellLayoutsAsync();

            var racks = await _rackRepository.GetAllAsync();

            var result = new List<RackDTO>();

            foreach (var rack in racks)
            {
                var rackCode = $"{rack.Column}-{rack.Row}";

                if (!rackLayouts.TryGetValue(rackCode, out var rackLayout))
                {
                    throw new InvalidOperationException(
                        $"Не найден RackLayout для стеллажа {rackCode}");
                }

                if (!cellLayouts.TryGetValue(rackLayout.Type, out var layoutsForRack))
                {
                    throw new InvalidOperationException(
                        $"Не найден CellLayout для типа стеллажа {rackLayout.Type}");
                }

                var rackDto = new RackDTO
                {
                    Id = rack.Id,

                    Column = rack.Column,
                    Row = rack.Row,

                    Type = rackLayout.Type,

                    X = rackLayout.X,
                    Y = rackLayout.Y,

                    Width = rackLayout.Width,
                    Height = rackLayout.Height,

                    Cells = rack.Cells
                        .Select(cell =>
                        {
                            var cellCode = $"{cell.Column}-{cell.Row}";

                            if (!layoutsForRack.TryGetValue(cellCode, out var cellLayout))
                            {
                                throw new InvalidOperationException(
                                    $"Не найден CellLayout {cellCode} для типа {rackLayout.Type}");
                            }

                            return new CellDto
                            {
                                Id = cell.Id,

                                Column = cell.Column,
                                Row = cell.Row,

                                X = cellLayout.X,
                                Y = cellLayout.Y,

                                Width = cellLayout.Width,
                                Height = cellLayout.Height
                            };
                        })
                        .ToList()
                };

                result.Add(rackDto);
            }

            return result;
        }

        private async Task<Dictionary<string, RackLayout>> LoadRackLayoutsAsync()
        {
            var path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "RackDescription.json");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "JSON-файл конфигурации стеллажей не найден.",
                    path);
            }

            var json = await File.ReadAllTextAsync(path);

            var layouts = JsonSerializer.Deserialize<List<RackLayout>>(json)?? [];

            return layouts.ToDictionary(x => x.Code);
        }

        private async Task<Dictionary<RackType, Dictionary<string, CellLayout>>> LoadCellLayoutsAsync()
        {
            var path = @"E:\Code\WarehouseManagementSystem\WMS.Desktop\CellDescription.json";

            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "JSON-файл конфигурации ячеек не найден.",
                    path);
            }

            // Принудительно читаем файл без кэширования
            using var fileStream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,  // Важно: разрешаем совместный доступ
                bufferSize: 4096,
                useAsync: true);

            using var reader = new StreamReader(fileStream);
            var json = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var layouts = JsonSerializer.Deserialize<List<CellLayoutRoot>>(json, options) ?? [];

            return layouts.ToDictionary(
                rackType => rackType.RackType,
                rackType => rackType.Cells.ToDictionary(
                    cell => cell.Code));
        }

        //private async Task<Dictionary<RackType, Dictionary<string, CellLayout>>> LoadCellLayoutsAsync()
        //    {
        //    var path = Path.Combine(
        //        AppDomain.CurrentDomain.BaseDirectory,
        //        "CellDescription.json");

        //    if (!File.Exists(path))
        //    {
        //        throw new FileNotFoundException(
        //            "JSON-файл конфигурации ячеек не найден.",
        //            path);
        //    }

        //    var json = await File.ReadAllTextAsync(path);

        //    var options = new JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true
        //    };

        //    var layouts = JsonSerializer.Deserialize<List<CellLayoutRoot>>(json, options)?? [];

        //    return layouts.ToDictionary(
        //        rackType => rackType.RackType,
        //        rackType => rackType.Cells.ToDictionary(
        //            cell => cell.Code));
        //}
    }
}
