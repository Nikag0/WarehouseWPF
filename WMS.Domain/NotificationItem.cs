using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMS.Domain
{
    public class NotificationItem
    {
        public string Title { get; set; }       
        public string Description { get; set; } 
        public NotificationType Type { get; set; }
        public bool IsNew { get; set; } = true; 
    }
}
