using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.Abstractions
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create();
    }
}
