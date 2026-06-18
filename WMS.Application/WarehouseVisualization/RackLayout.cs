using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WMS.Application.WarehouseVisualization
{
    public class RackLayout
    {
        public string Code { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RackType Type { get; set; }
        public List<CellLayout> Cells { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }
    }
}
