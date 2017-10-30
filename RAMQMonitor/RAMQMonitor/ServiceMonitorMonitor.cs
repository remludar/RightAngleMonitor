using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightAngleMonitor
{
    class ServiceMonitorMonitor
    {
        public ServiceMonitorMonitor()
        {
            SQLManager.GetCurrentServiceMonitorResults();
            SQLManager.resultSet.DeserializePreviousServiceMonitorFile();
            SQLManager.resultSet.SerializeToServiceMonitorFile();
        }
    }
}
