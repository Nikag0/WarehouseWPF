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
        Task<List<Operator>> GetAllAsync();
    }
}
