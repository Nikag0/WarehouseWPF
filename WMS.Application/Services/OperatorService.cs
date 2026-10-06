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

        public async Task<IReadOnlyList<Operator>> GetAllAsync()
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

        public Result AddAsync(string surname, string name, string patronymic)
        {
            var isAdd = _operatorRepo.IsOperatorUniqueAsync(
                surname.Trim().ToLower(),
                name.Trim().ToLower(),
                patronymic.Trim().ToLower());

            if (isAdd)
                return Result.Failure("Оператор c таким именем уже зарегистрирован.");

            var newOperator = Operator.Create(surname, name, patronymic);
            _operatorRepo.AddAsync(newOperator);

            return Result.Success();
        }

        public async Task<Result> UpdateAsync(OperatorDTO dto)
        {
            var @operator = await _operatorRepo.GetByIdAsync(dto.Id);
            if (@operator is null)
                return Result.Failure("Оператор не найден.");

            var isAdd = _operatorRepo.IsOperatorUniqueAsync(
                dto.Surname.Trim().ToLower(),
                dto.Name.Trim().ToLower(),
                dto.Patronymic.Trim().ToLower());

            if (isAdd)
                return Result.Failure("Оператор c таким именем уже зарегистрирован.");

            @operator.Update(dto.Surname, dto.Name, dto.Patronymic);

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

                op.Delete();

                await _operatorRepo.UpdateAsync(op);

                _logger.LogInformation($"Оператор с ID {id} удален.");

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
