using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightAngleMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            var rm = new RAMQMonitor();
            var sm = new ServiceMonitorMonitor();

            SQLManager.resultSet.CreateAlertTable();
            //SQLManager.resultSet.EmailResults();
            SQLManager.resultSet.PrintResults();
        }
    }
}
