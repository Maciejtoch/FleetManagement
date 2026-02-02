using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetManagement.Core.Models
{
    public class DailyReportStop
    {
        public int DailyReportId { get; set; }
        public DailyReport DailyReport { get; set; }
        public int StopId { get; set; }
        public Stop Stop { get; set; }
    }
}
