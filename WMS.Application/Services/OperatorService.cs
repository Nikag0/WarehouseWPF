using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Application.Abstractions;
using WMS.Domain;

namespace WMS.Application.Services
{
    public class OperatorService
    {
        private readonly IOperatorRepository _operatorRepo;

        public OperatorService(IOperatorRepository userRepo)
        {
            _operatorRepo = userRepo;
        }

        public async Task<List<Operator>> GetAllAsync()
        {
            return await _operatorRepo.GetAllAsync();
        }

        public async Task<Operator> GetByIdAsync(Guid id)
        {
            var op = await _operatorRepo.GetByIdAsync(id);
            if (op is null)
                throw new Exception("Оператор с таким Id не найден");

            return op;
        }

        public async Task AddAsync(Operator operatorr)
        {
            await _operatorRepo.AddAsync(operatorr);
        }

        public async Task RemoveAsync(Operator operatorr)
        {
            await _operatorRepo.RemoveAsync(operatorr);
        }

        public async Task UpdateAsync(Operator operatorr)
        {
            await _operatorRepo.UpdateAsync(operatorr);
        }
    }
}
