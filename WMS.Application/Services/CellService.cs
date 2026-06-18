using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Application.WarehouseVisualization;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class CellService
    {
        private readonly ICellRepository _cellRepo;
        private readonly IRackRepository _rackRepo;
        private readonly IStockRepository _stockRepo;

        public CellService(
            ICellRepository cellRepo, 
            IStockRepository stockRepo,
            IRackRepository rackRepo)
        {
            _cellRepo = cellRepo;
            _stockRepo = stockRepo;
            _rackRepo = rackRepo;
        }

        public async Task<List<Cell>> GetAllAsync()
        {
            return await _cellRepo.GetAllAsync();
        }

        public async Task<Cell> GetCellAsync(Guid id)
        {
            return await _cellRepo.GetCellAsync(id);
        }

        public async Task<List<Cell>> GetFreeCellsAsync(Guid? componentId = null)
        {
            var cells = await _cellRepo.GetAllAsync();
            var stocks = await _stockRepo.GetAllAsync();

            var busyCells = stocks
            .Where(s => componentId == null || s.ComponentId != componentId)
            .Select(s => s.CellId)
            .ToHashSet();

            var result = cells
            .Where(c => !busyCells.Contains(c.Id))
            .ToList();

            return result;
        }

        public async Task<IEnumerable<(Cell cell, CellLayout layout)>> GetCellsWithLayoutsAsync()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CellDescription.json");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("JSON-файл конфигурации ячеек не найден.", path);
            }

            var jsonText = await File.ReadAllTextAsync(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Десериализуем иерархическую структуру (свойства Type и Cells)
            var layouts = JsonSerializer.Deserialize<List<CellLayoutRoot>>(jsonText, options) ?? new();

            // Ключ = "RackType1", Значение = Список макетов для этого типа
            var layoutDict = layouts.ToDictionary(
                x => x.Type,
                x => x.Cells);

            var cellsDb = await GetAllAsync();
            var result = new List<(Cell cell, CellLayout layout)>();

            foreach (var cell in cellsDb)
            {
                if (cell.Rack == null)
                    continue;

                // 1. Извлекаем тип стеллажа (например, "RackType1")
                var rackType = cell.Rack.RackCode;

                // 2. Ищем массив макетов ячеек для "RackType1"
                if (!layoutDict.TryGetValue(rackType, out var cellLayoutsForType))
                    continue;

                // 3. Ищем конкретный макет по коду ячейки (например, "1-1")
                // cell.CellCode в вашей БД должен содержать строки вида "1-1", "1-2"
                var targetLayout = cellLayoutsForType.FirstOrDefault(l => l.Code == cell.CellCode);
                if (targetLayout == null)
                    continue;

                result.Add((cell, targetLayout));
            }

            return result;
        }

    }
}
