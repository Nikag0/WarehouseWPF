using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Application.Abstractions
{
    public interface IOperatorRepository
    {
        Task<IReadOnlyList<Operator>> GetAllAsync();
        Task<Operator?> GetByIdAsync(Guid id);
        // Возвращает true, если оператор с таким ФИО уже есть. excludeId — оператор, которого не учитывать (при редактировании).
        Task<bool> IsOperatorExistsAsync(string surname, string name, string patronymic, Guid? excludeId = null);
        Task AddAsync(Operator operatorr);
        Task UpdateAsync(Operator operatorr);
    }
}
