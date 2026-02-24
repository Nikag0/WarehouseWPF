using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain.ExceptionControl
{
    public class WrongValueExeption : DomainException
    {
        public WrongValueExeption(string message) : base(message) { }
    }
}
