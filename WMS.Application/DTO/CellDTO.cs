using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.DTO
{
    public sealed class CellDto
    {
        public Guid Id { get; init; }

        public int Column { get; init; }
        public int Row { get; init; }

        public double X { get; init; }
        public double Y { get; init; }

        public double Width { get; init; }
        public double Height { get; init; }
    }
}
