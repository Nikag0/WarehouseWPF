using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Application.WarehouseVisualization;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class RackService
    {
        private readonly IRackRepository _rackRepo;
        private readonly IStockRepository _stockRepo;

        public RackService(
            IRackRepository rackRepo,
            IStockRepository stockRepo)
        {
            _rackRepo = rackRepo;
            _stockRepo = stockRepo;
        }

        //public async Task<List<Rack>> GetAllAsync()
        //{
        //    return await _rackRepo.GetAllAsync();
        //}

        //public async Task<Rack> GetRackAsync(Guid id)
        //    {
        //    return await _rackRepo.GetRackAsync(id);
        //}

        //public async Task<IEnumerable<(Rack Rack, RackLayout Layout)>> GetRacksWithLayoutsAsync()
        //{
        //    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RackDescription.json");
        //    if (!File.Exists(path))
        //    {
        //        throw new FileNotFoundException("JSON-файл конфигурации стеллажей не найден.", path);
        //    }

        //    var jsonText = await File.ReadAllTextAsync(path);
        //    var layouts = JsonSerializer.Deserialize<List<RackLayout>>(jsonText) ?? new();
        //    var layoutDict = layouts.ToDictionary(l => l.Code);

        //    var racksDb = await GetAllAsync();

        //    var result = new List<(Rack Rack, RackLayout Layout)>();
        //    foreach (var rack in racksDb)
        //    {
        //        if (!layoutDict.TryGetValue(rack.RackCode, out var layout))
        //            continue; // Можно выбрасывать предупреждение, если не удалось найти макет для какого-то стеллажа.

        //        var cell = rack.Cells.Count();
        //        result.Add((rack, layout));
        //    }

        //    return result;
        //}
    }
}
