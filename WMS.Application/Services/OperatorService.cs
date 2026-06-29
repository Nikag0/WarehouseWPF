using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
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

        public async Task<OperatorDTO> GetByIdAsync(Guid id)
        {
            var result = await _operatorRepo.GetByIdAsync(id);
            if (result is null)
                throw new Exception("Оператор с таким Id не найден");

            return MappingExtensions.ToOperatorDTO(result);
        }

        public async Task AddAsync(string surname, string name, string patronymic)
        {
            var @operator = Operator.Create(surname, name, patronymic);
            await _operatorRepo.AddAsync(@operator);
        }

        public async Task<Result> UpdateAsync(OperatorDTO dto)
        {
            var @operator = await _operatorRepo.GetByIdAsync(dto.Id);
            if (@operator is null)
                return Result.Failure("Оператор не найден.");

            @operator.Update(dto.Surname, dto.Name, dto.Patronymic);

            await _operatorRepo.UpdateAsync(@operator);

            return Result.Success();
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
    }
}
