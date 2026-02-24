using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain.ExceptionControl
{
    public class OverallDomainException : DomainException
    {
        public OverallDomainException(string message) : base(message) { }
    }
}
