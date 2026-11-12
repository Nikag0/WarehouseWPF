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
using WMS.Domain.ExceptionControl;

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
                throw new BusinessException("Оператор не найден.");

            return MappingExtensions.ToOperatorDTO(result);
        }

        public async Task AddAsync(string surname, string name, string patronymic)
        {
            // Сначала доменная проверка: она обрежет пробелы и сообщит о пустых полях
            var newOperator = Operator.Create(surname, name, patronymic);

            var isExists = await _operatorRepo.IsOperatorExistsAsync(
                newOperator.Surname,
                newOperator.Name,
                newOperator.Patronymic);

            if (isExists)
                throw new BusinessException("Оператор c таким именем уже зарегистрирован.");

            await _operatorRepo.AddAsync(newOperator);
        }

        public async Task UpdateAsync(OperatorDTO dto)
        {
            var @operator = await _operatorRepo.GetByIdAsync(dto.Id);
            if (@operator is null)
                throw new BusinessException("Оператор не найден.");

            // Сначала доменная проверка. Объект загружен в закрытом контексте, поэтому до сохранения в БД изменения не попадут
            @operator.Update(dto.Surname, dto.Name, dto.Patronymic);

            // Самого редактируемого оператора не учитываем, иначе сохранение без изменений ФИО считалось бы дублем
            var isExists = await _operatorRepo.IsOperatorExistsAsync(
                @operator.Surname,
                @operator.Name,
                @operator.Patronymic,
                excludeId: dto.Id);

            if (isExists)
                throw new BusinessException("Оператор c таким именем уже зарегистрирован.");

            // Объект загружен в уже закрытом контексте, поэтому сохраняем его через репозиторий
            await _operatorRepo.UpdateAsync(@operator);
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var op = await _operatorRepo.GetByIdAsync(id);
                if (op is null)
                    throw new BusinessException("Оператор не найден.");

                await _operatorRepo.DeleteAsync(op);

                _logger.LogInformation($"Оператор с ID {id} удален.");
            }
            catch (Exception ex) when (ex is not BusinessException)
            {
                _logger.LogError(ex, $"Ошибка при удалении оператора {id}");
                throw;
            }
        }
    }
}
