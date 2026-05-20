using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain.Interfaces
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; }
        void Delete();
    }
}
