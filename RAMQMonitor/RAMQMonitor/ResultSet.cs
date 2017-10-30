using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightAngleMonitor
{
    public class ResultSet : DataSet
    {
        System.Type typeString = typeof(string);
        System.Type typeInt32 = typeof(int);
        System.Type typeDateTime = typeof(DateTime);
        System.Type typeTimeSpan = typeof(TimeSpan);

        public ResultSet()
        {
            _BuildRAMQDataSet();
            _BuildServiceMonitorDataSet();
        }


        private void _BuildRAMQDataSet()
        {
            DataTable RAMQStatus = new DataTable("CurrentRAMQStatus");
            DataColumn PrcssLgID = new DataColumn("PrcssLgID", typeString);
            DataColumn prcssGrpNme = new DataColumn("prcssGrpNme", typeString);
            DataColumn prcsslgbgn = new DataColumn("prcsslgbgn", typeDateTime);
            DataColumn prcssschdlrIInxtrndtetme = new DataColumn("prcssschdlrIInxtrndtetme", typeDateTime);
            DataColumn currstatus = new DataColumn("currstatus", typeString);
            DataColumn runtime = new DataColumn("runtime", typeInt32);
            DataColumn prcssQNme = new DataColumn("prcssQNme", typeString);
            DataColumn prcsslghstcmptr = new DataColumn("prcsslghstcmptr", typeString);
            DataColumn prcssSchdlrStts = new DataColumn("prcssSchdlrStts", typeString);
            DataColumn prcssSchdlrPrd = new DataColumn("prcssSchdlrPrd", typeString);
            DataColumn machinename = new DataColumn("machinename", typeString);


            RAMQStatus.Columns.Add(PrcssLgID);
            RAMQStatus.Columns.Add(prcssGrpNme);
            RAMQStatus.Columns.Add(prcsslgbgn);
            RAMQStatus.Columns.Add(prcssschdlrIInxtrndtetme);
            RAMQStatus.Columns.Add(currstatus);
            RAMQStatus.Columns.Add(runtime);
            RAMQStatus.Columns.Add(prcssQNme);
            RAMQStatus.Columns.Add(prcsslghstcmptr);
            RAMQStatus.Columns.Add(prcssSchdlrStts);
            RAMQStatus.Columns.Add(prcssSchdlrPrd);
            RAMQStatus.Columns.Add(machinename);


            Tables.Add(RAMQStatus);
        }

        private void _BuildServiceMonitorDataSet()
        {
            DataTable ServiceMonitorStatus = new DataTable("CurrentServiceMonitorStatus");
            DataColumn name = new DataColumn("name", typeString);
            DataColumn lastrunstart = new DataColumn("lastrunstart", typeDateTime);
            DataColumn nextscheduleddate = new DataColumn("nextscheduleddate", typeDateTime);
            DataColumn currstatus = new DataColumn("currstatus", typeString);
            DataColumn runtime = new DataColumn("runtime", typeInt32);
            DataColumn prevrunstarttime = new DataColumn("prevrunstarttime", typeTimeSpan);
            DataColumn schedruntime = new DataColumn("schedruntime", typeTimeSpan);
            DataColumn recurrencepattern = new DataColumn("recurrencepattern", typeString);
            DataColumn status = new DataColumn("status", typeString);
            DataColumn message = new DataColumn("message", typeString);


            ServiceMonitorStatus.Columns.Add(name);
            ServiceMonitorStatus.Columns.Add(lastrunstart);
            ServiceMonitorStatus.Columns.Add(nextscheduleddate);
            ServiceMonitorStatus.Columns.Add(currstatus);
            ServiceMonitorStatus.Columns.Add(runtime);
            ServiceMonitorStatus.Columns.Add(prevrunstarttime);
            ServiceMonitorStatus.Columns.Add(schedruntime);
            ServiceMonitorStatus.Columns.Add(recurrencepattern);
            ServiceMonitorStatus.Columns.Add(status);
            ServiceMonitorStatus.Columns.Add(message);

            Tables.Add(ServiceMonitorStatus);
        }

        
        public void SerializeToRAMQFile()
        {
            var stringBuilder = new StringBuilder();
            IEnumerable<string> columnNames = Tables["CurrentRAMQStatus"].Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            stringBuilder.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in Tables["CurrentRAMQStatus"].Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                stringBuilder.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText("RAMQResults.csv", stringBuilder.ToString());
        }

        public void SerializeToServiceMonitorFile()
        {
            var stringBuilder = new StringBuilder();
            IEnumerable<string> columnNames = Tables["CurrentServiceMonitorStatus"].Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            stringBuilder.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in Tables["CurrentServiceMonitorStatus"].Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                stringBuilder.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText("ServiceMonitorResults.csv", stringBuilder.ToString());
        }

        public void DeserializePreviousRAMQFile()
        {
            string CSVFilePathName = @"RAMQResults.csv";
            string[] Lines = File.ReadAllLines(CSVFilePathName);
            string[] Fields;
            Fields = Lines[0].Split(new char[] { ',' });
            int Cols = Fields.GetLength(0);
            DataTable dt = new DataTable("PreviousRAMQStatus");
            for (int i = 0; i < Cols; i++)
                dt.Columns.Add(Fields[i].ToLower(), typeof(string));
            DataRow Row;
            for (int i = 1; i < Lines.GetLength(0); i++)
            {
                Fields = Lines[i].Split(new char[] { ',' });
                Row = dt.NewRow();
                for (int f = 0; f < Cols; f++)
                    Row[f] = Fields[f];
                dt.Rows.Add(Row);
            }

            Tables.Add(dt);
        }

        public void DeserializePreviousServiceMonitorFile()
        {
            string CSVFilePathName = @"ServiceMonitorResults.csv";
            string[] Lines = File.ReadAllLines(CSVFilePathName);
            string[] Fields;
            Fields = Lines[0].Split(new char[] { ',' });
            int Cols = Fields.GetLength(0);
            DataTable dt = new DataTable("PreviousServiceMonitorStatus");
            for (int i = 0; i < Cols; i++)
                dt.Columns.Add(Fields[i].ToLower(), typeof(string));
            DataRow Row;
            for (int i = 1; i < Lines.GetLength(0); i++)
            {
                Fields = Lines[i].Split(new char[] { ',' });
                Row = dt.NewRow();
                for (int f = 0; f < Cols; f++)
                    Row[f] = Fields[f];
                dt.Rows.Add(Row);
            }

            Tables.Add(dt);
        }

        public void CreateAlertTable()
        {
            //RAMQ Rows
            var ramQAlerts = Tables["CurrentRAMQStatus"].Clone();
            ramQAlerts.TableName = "ramQAlerts";

            
            IEnumerable<DataRow> inProcessRAMQMatches = from dataRows1 in Tables["CurrentRAMQStatus"].AsEnumerable()
                                                         join dataRows2 in Tables["PreviousRAMQStatus"].AsEnumerable()
                                                         on dataRows1.Field<string>("PrcssLgID") equals dataRows2.Field<string>("PrcssLgID")
                                                         where (dataRows1.Field<string>("currstatus") == "In-Process")
                                                         select dataRows1;

            IEnumerable<DataRow> errorRAMQMatches = from dataRows in Tables["CurrentRAMQStatus"].AsEnumerable()
                               where dataRows.Field<string>("currstatus") == "Error"
                               select dataRows;

            
            foreach(var item in inProcessRAMQMatches)
            {
                ramQAlerts.ImportRow(item);
            }

            foreach(var item in errorRAMQMatches)
            {
                ramQAlerts.ImportRow(item);
            }

            Tables.Add(ramQAlerts);

            //Service Monitor Rows

            var serviceMonitorAlerts = Tables["CurrentServiceMonitorStatus"].Clone();
            serviceMonitorAlerts.TableName = "serviceMonitorAlerts";
            
            IEnumerable<DataRow> runningServiceMonitorMatches = from dataRows1 in Tables["CurrentServiceMonitorStatus"].AsEnumerable()
                                                                join dataRows2 in Tables["PreviousServiceMonitorStatus"].AsEnumerable()
                                                                on dataRows1.Field<string>("name") equals dataRows2.Field<string>("name")
                                                                where (dataRows1.Field<string>("currstatus") == "Waiting")
                                                                select dataRows1;

            IEnumerable<DataRow> pendingServiceMonitorMatches = from dataRows1 in Tables["CurrentServiceMonitorStatus"].AsEnumerable()
                                                                join dataRows2 in Tables["PreviousServiceMonitorStatus"].AsEnumerable()
                                                                on dataRows1.Field<string>("name") equals dataRows2.Field<string>("name")
                                                                where (dataRows1.Field<string>("currstatus") == "Pending")
                                                                select dataRows1;

            IEnumerable<DataRow> errorServiceMonitorMatches = from dataRows in Tables["CurrentServiceMonitorStatus"].AsEnumerable()
                                                                where dataRows.Field<string>("currstatus") == "Faulted"
                                                                select dataRows;
            foreach (var item in runningServiceMonitorMatches)
            {
                serviceMonitorAlerts.ImportRow(item);
            }

            foreach (var item in pendingServiceMonitorMatches)
            {
                serviceMonitorAlerts.ImportRow(item);
            }

            foreach (var item in errorServiceMonitorMatches)
            {
                serviceMonitorAlerts.ImportRow(item);
            }

            Tables.Add(serviceMonitorAlerts);
        }

        public void PrintResults()
        {
            foreach (DataRow dataRow in Tables["ramQAlerts"].Rows)
            {
                foreach (var item in dataRow.ItemArray)
                {
                    Console.Write(item + "|");
                }
                Console.WriteLine();
            }

            foreach (DataRow dataRow in Tables["serviceMonitorAlerts"].Rows)
            {
                foreach (var item in dataRow.ItemArray)
                {
                    Console.Write(item + "|");
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }


        public void EmailResults()
        {
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.To.Add("jason.ewton@motiva.com");
            message.Subject = "This is the Subject line";
            message.From = new System.Net.Mail.MailAddress("nearshoresupportteam@motiva.com");
            message.Body = "This is the message body";
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("10.58.40.17");
            smtp.Send(message);
        }
    }
}
