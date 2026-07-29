using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Application.DTO
{
    public record RackDTO
    {
        public Guid Id { get; set; }
        public int Column { get; set; }
        public int Row { get; set; }
        public RackType Type { get; set; }
        //public IReadOnlyList<CellDTO> Cells { get; set; }
        public IReadOnlyList<CellDTO> Cells { get; init; }
            = Array.Empty<CellDTO>();
    }

    //public sealed class RackDTO
    //{
    //    public Guid Id { get; init; }

    //    public int Column { get; init; }
    //    public int Row { get; init; }

    //    public RackType Type { get; init; }

    //    public double X { get; init; }
    //    public double Y { get; init; }
    //    public double Width { get; init; }
    //    public double Height { get; init; }

    //}
}
