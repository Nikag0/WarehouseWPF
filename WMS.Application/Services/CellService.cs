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
        private readonly ICellRepository _cellRepo;
        private readonly IStockRepository _stockRepo;

        public CellService(
            ICellRepository cellRepo, 
            IStockRepository stockRepo)
        {
            _cellRepo = cellRepo;
            _stockRepo = stockRepo;
        }

        public async Task<List<Cell>> GetAllAsync()
        {
            return await _cellRepo.GetAllAsync();
        }

        public async Task<List<Cell>> GetFreeCellsAsync()
        {
            var cells = await _cellRepo.GetAllAsync();  
            var stocks = await _stockRepo.GetAllAsync();

            var busyCells = stocks
                .Select(s => s.CellId)
                .ToHashSet();

            var freeCells = cells
                .Where(c => !busyCells.Contains(c.Id))
                .ToList();

            return freeCells;
        }
    }
}
