using Microsoft.Extensions.Logging;
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
        private readonly ILogger<OperatorService> _logger;


        public OperatorService(IOperatorRepository userRepo,
                               ILogger<OperatorService> logger)
        {
            _operatorRepo = userRepo;
            _logger = logger;
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

        public async Task<Result> DeleteAsync(Guid id)
        {
            try
            {
                var op = await _operatorRepo.GetByIdAsync(id);
                if (op is null)
                {
                    return Result.Failure("Оператор не найден.");
                }

                await _operatorRepo.RemoveAsync(op);

                _logger.LogInformation($"Оператор с ID {id} успешно удален.");

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении оператора {id}");
                throw;
            }
        }


        public async Task UpdateAsync(Operator operatorr)
        {
            await _operatorRepo.UpdateAsync(operatorr);
        }
    }
}
