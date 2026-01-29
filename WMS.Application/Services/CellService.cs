using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class CellService
    {
        private readonly ICellRepository _repo;

        public CellService(ICellRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<Cell>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }
    }
}
