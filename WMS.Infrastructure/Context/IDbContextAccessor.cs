using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Infrastructure.Context
{
    public interface IDbContextAccessor
    {
        AppDbContext CurrentContext { get; }

        bool IsContextInitialized { get; }
    }
}
