using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WMS.Application.Services;
using WMS.Application.DTO;
using WMS.Desktop.Models;
using WMS.Domain;
using WMS.Application;

namespace WMS.Desktop.Services
{
    public class WarehouseVisualizationService
    {
        private readonly WarehouseService _warehouseService;
        public WarehouseVisualizationService(WarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        public async Task<IReadOnlyList<RackViewModel>> GetWarehouseAsync(IReadOnlyCollection<RackDTO> racks)
        {
            var rackLayouts = await LoadRackLayoutsAsync();
            var cellLayouts = await LoadCellLayoutsAsync();

            var result = new List<RackViewModel>();

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

                var rackDto = new RackViewModel
                (
                    id: rack.Id,
                    code: LocationFormatter.CodeToDisplay(rack.Column, rack.Row),
                    rackType: rack.Type,
                    cells: rack.Cells
                        .Select(cell =>
                        {
                            var cellCode = $"{cell.Column}-{cell.Row}";

                            if (!layoutsForRack.TryGetValue(cellCode, out var cellLayout))
                            {
                                throw new InvalidOperationException(
                                    $"Не найден CellLayout {cellCode} для типа {rack.Type}");
                            }

                            return new CellViewModel
                            (
                                id: cell.Id,
                                code: LocationFormatter.CodeToDisplay(cell.Column, cell.Row),
                                x: cellLayout.X,
                                y: cellLayout.Y,
                                width: cellLayout.Width,
                                height: cellLayout.Height
                            );
                        })
                    .ToList(),
                    x: rackLayout.X,
                    y: rackLayout.Y,
                    width: rackLayout.Width,
                    height: rackLayout.Height
                );

                result.Add(rackDto);
            }

            return result;
        }

        public async Task<Dictionary<string, RackLayout>> LoadRackLayoutsAsync()
        {
            //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RackDescription.json");
            var path = @"D:\WarehouseWPF\WMS.Desktop\VisualizationResources\RackDescription.json";

            if (!File.Exists(path))
                throw new FileNotFoundException("JSON-файл конфигурации стеллажей не найден.", path);

            var json = await File.ReadAllTextAsync(path);
            var layouts = JsonSerializer.Deserialize<List<RackLayout>>(json) ?? [];

            return layouts.ToDictionary(x => x.Code);
        }

        public async Task<Dictionary<RackType, Dictionary<string, CellLayout>>> LoadCellLayoutsAsync()
        {
            var path = @"D:\WarehouseWPF\WMS.Desktop\VisualizationResources\CellDescription.json";

            if (!File.Exists(path))
                throw new FileNotFoundException("JSON-файл конфигурации ячеек не найден.", path);

            using var fileStream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite,  
                    bufferSize: 4096,
                    useAsync: true);

            using var reader = new StreamReader(fileStream);
            var json = await reader.ReadToEndAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var layouts = JsonSerializer.Deserialize<List<CellLayoutRoot>>(json, options) ?? [];

            return layouts.ToDictionary(
                rackType => rackType.RackType,
                rackType => rackType.Cells.ToDictionary(cell => cell.Code));
        }
    }
}
