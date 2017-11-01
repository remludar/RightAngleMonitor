using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightAngleMonitor
{
    public static class SQLManager
    {
        private static ConnectionStringSettings _connectionString;
        private static SqlConnection _connection;
        public static ResultSet resultSet;

        public static void GetCurrentRAMQResults()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DBConnection"];
            _connection = new SqlConnection(_connectionString.ConnectionString);
            resultSet = new ResultSet();

            _QueryRAMQ();
        }
        public static void GetCurrentServiceMonitorResults()
        {
            _QueryServiceMonitor();
        }

        private static void _QueryRAMQ()
        {
            _connection.Open();
            var sqlString =
                @"
                select 
	                pl.PrcssLgID,
	                pg.prcssGrpNme, 
	                pl.prcsslgbgn, 
	                psII.prcssschdlrIInxtrndtetme, 
                    case pl.prcsslgstts
		                when 'I' then 'In-Process'
		                when 'C' then 'Complete'
		                when 'E' then 'Error'
		                when 'P' then 'Pending'
                    end currstatus, 
                    case pl.prcsslgstts 
		                when 'I' then datediff(second, pl.prcsslgbgn, getdate())    -- In Process
		                when 'C' then datediff(second,pl.prcsslgbgn,pl.prcsslgend)  -- Complete
		                when 'E' then datediff(second,pl.prcsslgbgn,pl.prcsslgend)	-- Error
		                when 'P' then 0
		                else -999
                    end runtime,
                    psII.prcssQNme, 
	                pl.prcsslghstcmptr, 
	                psII.prcssSchdlrStts, 
	                psII.prcssSchdlrPrd,
                    hc.machinename
                from 
	                processSchedulerII psII (nolock), 
                    processgroup pg (nolock), 
	                processlog pl (nolock)
                left outer join 
	                hostcomputer hc (nolock) 
                on 1=1
	                and lower(pl.prcsslghstcmptr) = lower(hc.hstcmptr)
                where 1=1
	                and psII.prcssSchdlrStts = 'I' 
	                and pl.prcsslgstts = 'E' 
	                and psII.prcssSchdlrPrd not in ('A','B','C','D','E','F','G','H','I','J','K','L','M','O','P','Q','S','T','U','W') 
	                and pg.prcssGrpID = psII.prcssSchdlrIIPrcssGrpID 
	                and pl.prcsslgprcssgrpID = psII.prcssSchdlrIIPrcssGrpID 
	                and pl.prcsslgprcssSchdlrIIID = psII.prcssSchdlrIIID 
	                and pl.prcsslgid = 
	                (
		                select 
			                max(pl2.prcsslgid)
                        from 
			                processlog pl2 (nolock)
                        where 1=1
			                and pl2.prcsslgprcssgrpid = pl.prcsslgprcssgrpid 
			                and pl2.prcsslgprcssSchdlrIIID = pl.prcsslgprcssSchdlrIIID
	                )
                    --and prcsslgbgn > DateADD(minute, -30, GetDate())
                order by 1 desc";
            using (SqlCommand command = new SqlCommand(sqlString, _connection))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(resultSet.Tables["CurrentRAMQStatus"]);

                _connection.Close();
                dataAdapter.Dispose();
            }
        }
        private static void _QueryServiceMonitor()
        {
            _connection.Open();
            var sqlString =
                @"
                select 
	                name,
                    lastrunstart, 
	                lastrunend, 
	                nextscheduleddate,
	                case status
		                when 'W' then 'Waiting'
		                when 'R' then 'Running'
		                when 'P' then 'Pending'
		                when 'F' then 'Faulted'
		                else ''
	                end currstatus,
	                case status 
		                when 'R' then datediff(second, lastrunstart, getdate()) 
		                when 'W' then datediff(second,lastrunstart,lastrunend) 
		                when 'P' then 0
		                when 'F' then datediff(second,lastrunstart,lastrunend)
		                else -999
	                end runtime,
	                cast(lastrunstart as time) prevrunstarttime,
	                cast(nextscheduleddate as time) schedruntime,
	                recurrencepattern,
	                status,
	                message
                from scheduledtask (nolock)
                --where enabled = 1
                order by 1
                ";
            using (SqlCommand command = new SqlCommand(sqlString, _connection))
            {
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(resultSet.Tables["CurrentServiceMonitorStatus"]);

                _connection.Close();
                dataAdapter.Dispose();
            }
        }

    }
}
