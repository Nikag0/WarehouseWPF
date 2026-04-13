using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
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

        public async Task<List<Rack>> GetAllAsync()
        {
            return await _rackRepo.GetAllAsync();
        }

        public async Task<Rack> GetRackAsync(Guid id)
            {
            return await _rackRepo.GetRackAsync(id);
        }
    }
}
