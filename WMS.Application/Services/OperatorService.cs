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
    }
}
