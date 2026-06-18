using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Application.WarehouseVisualization
{
    public class CellLayoutRoot
    {
        public string Type { get; set; }
        public List<CellLayout> Cells { get; set; }
    }
}
