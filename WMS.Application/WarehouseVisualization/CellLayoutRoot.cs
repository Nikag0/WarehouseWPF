using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WMS.Domain;

namespace WMS.Application.WarehouseVisualization
{
    public class CellLayoutRoot
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public RackType RackType { get; set; }
        public List<CellLayout> Cells { get; set; }
    }
}
