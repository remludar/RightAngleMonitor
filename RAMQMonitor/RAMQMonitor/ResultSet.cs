using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RightAngleMonitor
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class ResultSet : DataSet
    {
        System.Type typeString = typeof(string);
        System.Type typeInt32 = typeof(int);
        System.Type typeDateTime = typeof(DateTime);
        System.Type typeTimeSpan = typeof(TimeSpan);

        string delimiter = "~~";
        string ramQEmailTable;
        string serviceMonitorEmailTable;

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
            DataColumn lastrunend = new DataColumn("lastrunend", typeDateTime);
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
            ServiceMonitorStatus.Columns.Add(lastrunend);
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

        private static string _ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table>";
            //add header row
            html += "<tr>";
            for (int i = 0; i < dt.Columns.Count; i++)
                html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }

        public void SerializeToRAMQFile()
        {
            var stringBuilder = new StringBuilder();
            IEnumerable<string> columnNames = Tables["CurrentRAMQStatus"].Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            stringBuilder.AppendLine(string.Join(delimiter, columnNames));

            for (int i = 0; i < Tables["CurrentRAMQStatus"].Rows.Count; i++)
            {
                List<string> columnValues = new List<string>();
                for (int j = 0; j < Tables["CurrentRAMQStatus"].Columns.Count; j++)
                {
                    columnValues.Add(Tables["CurrentRAMQStatus"].Rows[i][j].ToString());
                }
                var newRow = string.Join(delimiter, columnValues);
                newRow = newRow.Replace("\n", "");
                newRow = newRow.Replace("\r", "");
                stringBuilder.AppendLine(newRow);

            }
            File.WriteAllText("RAMQ", stringBuilder.ToString());
        }

        public void SerializeToServiceMonitorFile()
        {
            var stringBuilder = new StringBuilder();
            IEnumerable<string> columnNames = Tables["CurrentServiceMonitorStatus"].Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            stringBuilder.AppendLine(string.Join(delimiter, columnNames));

            for (int i = 0; i < Tables["CurrentServiceMonitorStatus"].Rows.Count; i++)
            {
                List<string> columnValues = new List<string>();
                for (int j = 0; j < Tables["CurrentServiceMonitorStatus"].Columns.Count; j++)
                {
                    columnValues.Add(Tables["CurrentServiceMonitorStatus"].Rows[i][j].ToString());
                }
                var newRow = string.Join(delimiter, columnValues);
                newRow = newRow.Replace("\n", "");
                newRow = newRow.Replace("\r", "");
                stringBuilder.AppendLine(newRow);

            }

            File.WriteAllText("ServiceMonitor", stringBuilder.ToString());
        }

        public void DeserializePreviousRAMQFile()
        {
            string CSVFilePathName = @"RAMQ";
            if (!File.Exists(CSVFilePathName))
            {
                return;
            }

            string[] Lines = File.ReadAllLines(CSVFilePathName);
            string[] Fields;
            
            Fields = Lines[0].Split(new string[] { delimiter }, StringSplitOptions.None);
            int Cols = Fields.GetLength(0);
            DataTable dt = new DataTable("PreviousRAMQStatus");
            for (int i = 0; i < Cols; i++)
                dt.Columns.Add(Fields[i].ToLower(), typeof(string));


            DataRow Row;
            for (int i = 1; i < Lines.GetLength(0); i++)
            {
                Fields = Lines[i].Split(new string[] { delimiter }, StringSplitOptions.None);
                Row = dt.NewRow();
                for (int j = 0; j < Cols; j++)
                {
                    Row[j] = "";
                    if (Fields.Count() > j)
                        Row[j] = Fields[j];
                }

                dt.Rows.Add(Row);
            }

            Tables.Add(dt);
        }

        public void DeserializePreviousServiceMonitorFile()
        {
            string CSVFilePathName = @"ServiceMonitor";
            if (!File.Exists(CSVFilePathName))
            {
                return;
            }

            string[] Lines = File.ReadAllLines(CSVFilePathName);
            
            string[] Fields;
            Fields = Lines[0].Split(new string[] { delimiter }, StringSplitOptions.None);
            int Cols = Fields.GetLength(0);
            DataTable dt = new DataTable("PreviousServiceMonitorStatus");
            for (int i = 0; i < Cols; i++)
                dt.Columns.Add(Fields[i].ToLower(), typeof(string));


            DataRow Row;
            for (int i = 1; i < Lines.GetLength(0); i++)
            {
                Fields = Lines[i].Split(new string[] { delimiter }, StringSplitOptions.None);
                Row = dt.NewRow();
                for (int j = 0; j < Cols; j++)
                {
                    Row[j] = "";
                    if(Fields.Count() > j)
                        Row[j] = Fields[j];
                }
                    
                dt.Rows.Add(Row);
            }

            Tables.Add(dt);
        }

        public void CreateAlertTable()
        {
            //RAMQ Rows
            IEnumerable<DataRow> inProcessRAMQMatches;
            if (!Tables.Contains("PreviousRAMQStatus"))
            {
                inProcessRAMQMatches = from dataRows1 in Tables["CurrentRAMQStatus"].AsEnumerable()
                                       where (dataRows1.Field<string>("currstatus") == "In-Process")
                                       select dataRows1;
            }
            else
            {
                inProcessRAMQMatches = from dataRows1 in Tables["CurrentRAMQStatus"].AsEnumerable()
                                       join dataRows2 in Tables["PreviousRAMQStatus"].AsEnumerable()
                                       on dataRows1.Field<string>("PrcssLgID") equals dataRows2.Field<string>("PrcssLgID")
                                       where (dataRows1.Field<string>("currstatus") == "In-Process")
                                       select dataRows1;
            }
            
            IEnumerable<DataRow> errorRAMQMatches = from dataRows in Tables["CurrentRAMQStatus"].AsEnumerable()
                                                    where dataRows.Field<string>("currstatus") == "Error"
                                                    select dataRows;

            var ramQAlerts = Tables["CurrentRAMQStatus"].Clone();
            ramQAlerts.TableName = "ramQAlerts";
            foreach (var item in inProcessRAMQMatches)
            {
                ramQAlerts.ImportRow(item);
            }

            foreach(var item in errorRAMQMatches)
            {
                ramQAlerts.ImportRow(item);
            }

            ramQEmailTable = _ConvertDataTableToHTML(ramQAlerts);
            Tables.Add(ramQAlerts);

            //Service Monitor Rows
            IEnumerable<DataRow> runningServiceMonitorMatches;
            IEnumerable<DataRow> pendingServiceMonitorMatches;
            if (!Tables.Contains("PreviousServiceMonitorStatus"))
            {
                runningServiceMonitorMatches = from dataRows1 in Tables["CurrentServiceMonitorStatus"].AsEnumerable()
                                               where (dataRows1.Field<string>("currstatus") == "Waiting")
                                               select dataRows1;

                pendingServiceMonitorMatches = from dataRows1 in Tables["CurrentServiceMonitorStatus"].AsEnumerable()
                                               where (dataRows1.Field<string>("currstatus") == "Pending")
                                               select dataRows1;
            }
            else
            {
                runningServiceMonitorMatches = from dataRows1 in Tables["CurrentServiceMonitorStatus"].AsEnumerable()
                                               join dataRows2 in Tables["PreviousServiceMonitorStatus"].AsEnumerable()
                                               on dataRows1.Field<string>("name") equals dataRows2.Field<string>("name")
                                               where (dataRows1.Field<string>("currstatus") == "Waiting")
                                               select dataRows1;

                pendingServiceMonitorMatches = from dataRows1 in Tables["CurrentServiceMonitorStatus"].AsEnumerable()
                                               join dataRows2 in Tables["PreviousServiceMonitorStatus"].AsEnumerable()
                                               on dataRows1.Field<string>("name") equals dataRows2.Field<string>("name")
                                               where (dataRows1.Field<string>("currstatus") == "Pending")
                                               select dataRows1;
            }

            IEnumerable<DataRow> errorServiceMonitorMatches = from dataRows in Tables["CurrentServiceMonitorStatus"].AsEnumerable()
                                                              where dataRows.Field<string>("currstatus") == "Faulted"
                                                              select dataRows;

            var serviceMonitorAlerts = Tables["CurrentServiceMonitorStatus"].Clone();
            serviceMonitorAlerts.TableName = "serviceMonitorAlerts";
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

            serviceMonitorEmailTable = _ConvertDataTableToHTML(serviceMonitorAlerts);
            Tables.Add(serviceMonitorAlerts);
        }

        public void PrintResults()
        {
            //if (Tables.Contains("ramQAlerts"))
            //{
            //    foreach (DataRow dataRow in Tables["ramQAlerts"].Rows)
            //    {
            //        foreach (var item in dataRow.ItemArray)
            //        {
            //            Console.Write(item + "|");
            //        }
            //        Console.WriteLine();
            //    }
            //}

            if (Tables.Contains("serviceMonitorAlerts"))
            {
                foreach (DataRow dataRow in Tables["serviceMonitorAlerts"].Rows)
                {
                    foreach (var item in dataRow.ItemArray)
                    {
                        Console.Write(item + "|");
                    }
                    Console.WriteLine();
                }
            }
            

            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }

        public void EmailResults()
        {

            var stringBuilder = new StringBuilder();
            IEnumerable<string> columnNames = Tables["ramQAlerts"].Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            stringBuilder.AppendLine(string.Join(",", columnNames));

            for (int i = 0; i < Tables["ramQAlerts"].Rows.Count; i++)
            {
                List<string> columnValues = new List<string>();
                for (int j = 0; j < Tables["ramQAlerts"].Columns.Count; j++)
                {
                    columnValues.Add(Tables["ramQAlerts"].Rows[i][j].ToString());
                }
                var newRow = string.Join(",", columnValues);
                newRow = newRow.Replace("\n", "");
                newRow = newRow.Replace("\r", "");
                stringBuilder.AppendLine(newRow);

            }
            File.WriteAllText("RAMQ.csv", stringBuilder.ToString());

            var stringBuilder2 = new StringBuilder();
            IEnumerable<string> columnNames2 = Tables["CurrentServiceMonitorStatus"].Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            stringBuilder2.AppendLine(string.Join(",", columnNames2));

            for (int i = 0; i < Tables["CurrentServiceMonitorStatus"].Rows.Count; i++)
            {
                List<string> columnValues = new List<string>();
                for (int j = 0; j < Tables["CurrentServiceMonitorStatus"].Columns.Count; j++)
                {
                    columnValues.Add(Tables["CurrentServiceMonitorStatus"].Rows[i][j].ToString());
                }
                var newRow = string.Join(",", columnValues);
                newRow = newRow.Replace("\n", "");
                newRow = newRow.Replace("\r", "");
                stringBuilder2.AppendLine(newRow);

            }
            File.WriteAllText("ServiceMonitor.csv", stringBuilder2.ToString());


            System.Net.Mail.Attachment ramQCSV = new System.Net.Mail.Attachment("RAMQ.csv");
            System.Net.Mail.Attachment serviceMonitorCSV = new System.Net.Mail.Attachment("ServiceMonitor.csv");

            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.To.Add("jason.ewton@motiva.com; reed.mattingly@motiva.com; Sushma.Bhat@motiva.com");
            message.Subject = "RightAngle Monitor Alert";
            message.From = new System.Net.Mail.MailAddress("nearshoresupportteam@motiva.com");
            message.IsBodyHtml = true;
            message.Body = "See attached documents for processes to troubleshoot";
            message.Attachments.Add(ramQCSV);
            message.Attachments.Add(serviceMonitorCSV);
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("10.58.40.17");
            smtp.Send(message);
        }
    }
}
