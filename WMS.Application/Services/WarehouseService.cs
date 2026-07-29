using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Application.DTO;

namespace WMS.Application.Services
{
    public class WarehouseService
    {
        private readonly IRackRepository _rackRepository;

        public WarehouseService(IRackRepository rackRepository)
        {
            _rackRepository = rackRepository;
        }

        public async Task<IReadOnlyList<RackDTO>> GetRacksAsync()
        {
            var racks = await _rackRepository.GetAllAsync();

            return racks.Select(rack => new RackDTO
            {
                Id = rack.Id,
                Column = rack.Column,
                Row = rack.Row,
                Type = rack.Type,
                Cells = rack.Cells.Select(cell => new CellDTO
                {
                    Id = cell.Id,
                    Column = cell.Column,
                    Row = cell.Row
                }).ToList()
            }).ToList();
        }
    }
}
