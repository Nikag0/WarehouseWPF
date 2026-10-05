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
        bool IsOperatorUniqueAsync(string surname, string name, string patronymic);
        Task AddAsync(Operator operatorr);

        Task SaveChangesAsync();
    }
}
