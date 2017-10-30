using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightAngleMonitor
{
    public class RAMQMonitor
    {
        public RAMQMonitor()
        {
            SQLManager.GetCurrentRAMQResults();
            SQLManager.resultSet.DeserializePreviousRAMQFile();
            SQLManager.resultSet.SerializeToRAMQFile();
            
        }
    }
}
