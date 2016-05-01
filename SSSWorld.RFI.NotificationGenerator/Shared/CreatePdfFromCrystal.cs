using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

using SSSWorld.Common;
using System.IO;
using System.Data;
using SSSWorld.RFI.NotificationGenerator.Shared;


namespace SSSWorld.RFI.NotificationGenerator
{
    public class CreatePdfFromCrystal
    {
        private readonly IDBConnectionWrapper _db;
        private readonly Configuration _configuration;

        public CreatePdfFromCrystal(IDBConnectionWrapper db, Configuration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public string PrintSlxReport(string reportName, string rsf)
        {
            String rptFile = ExtractReport(reportName, _db);
            return PrintStaticReport(rptFile, rsf, null);
        }

        /// <summary>
        /// Print report from existing rpt file
        /// </summary>
        /// <param name="reportOutput">Where to save pdf output</param>
        /// <param name="reportFilename">Path to rpt file</param>
        /// <param name="rsf">Optional record selection formula</param>
        /// <param name="parameters">Optional parameters for report</param>
        public string PrintStaticReport(string reportFilename, string rsf, Dictionary<string, object> parameters)
        {
            using (CrystalDecisions.CrystalReports.Engine.ReportDocument doc = new ReportDocument())
            {
                doc.FileName = reportFilename;

                if (rsf != null)
                    doc.RecordSelectionFormula = rsf;
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> kvp in parameters)
                    {
                        doc.SetParameterValue(kvp.Key, kvp.Value);
                    }
                }

                SetLogonInfo(_db.ConnectionString, doc);

                var outputPath = Utils.GetUniqueFileName(_configuration.TemporaryFolder, Path.GetFileNameWithoutExtension(reportFilename), ".pdf");
                doc.ExportToDisk(ExportFormatType.PortableDocFormat, outputPath);
                doc.Close();
                return outputPath;
            }
        }


        private void SetLogonInfo(string connStr, ReportDocument reportDoc)
        {
            ConnectionInfo conInfo = GetConnectionInformation(connStr);

            SetDBLogonForReport(conInfo, reportDoc);
            SetDBLogonForSubreports(conInfo, reportDoc);
        }

        /// <summary>
        /// Extract database info from the connection string
        /// </summary>
        private ConnectionInfo GetConnectionInformation(string connStr)
        {
            ConnectionInfo conInfo = new ConnectionInfo();
            conInfo.DatabaseName = GetParameterFromConnection(connStr, "Initial Catalog");
            conInfo.UserID = GetParameterFromConnection(connStr, "User ID");
            conInfo.Password = GetParameterFromConnection(connStr, "Password");
            conInfo.ServerName = GetParameterFromConnection(connStr, "Data Source");
            return conInfo;
        }

        /// <summary>
        /// Extract a parameter from the connection string.
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        private String GetParameterFromConnection(String connectionString, String paramName)
        {
            Regex re = new Regex(paramName + "\\s*=\\s*\"?([^\";]+)", RegexOptions.IgnoreCase);
            Match m = re.Match(connectionString);
            if (m.Success)
                return m.Groups[1].Value;
            return "";
        }


        private void SetDBLogonForReport(ConnectionInfo connectionInfo, ReportDocument reportDocument)
        {
            Tables tables = reportDocument.Database.Tables;
            foreach (CrystalDecisions.CrystalReports.Engine.Table table in tables)
            {
                TableLogOnInfo tableLogonInfo = table.LogOnInfo;
                tableLogonInfo.ConnectionInfo = connectionInfo;
                table.ApplyLogOnInfo(tableLogonInfo);
            }
        }


        private void SetDBLogonForSubreports(ConnectionInfo connectionInfo, ReportDocument reportDocument)
        {
            Sections sections = reportDocument.ReportDefinition.Sections;
            foreach (Section section in sections)
            {
                ReportObjects reportObjects = section.ReportObjects;
                foreach (ReportObject reportObject in reportObjects)
                {
                    if (reportObject.Kind == ReportObjectKind.SubreportObject)
                    {
                        SubreportObject subreportObject = (SubreportObject)reportObject;
                        ReportDocument subReportDocument = subreportObject.OpenSubreport(subreportObject.SubreportName);
                        SetDBLogonForReport(connectionInfo, subReportDocument);
                    }
                }
            }
        }


        /// <summary>
        /// Extract report from database to local file.
        /// Return local path.
        /// </summary>
        /// <param name="reportName"></param>
        /// <returns></returns>
        private String ExtractReport(String reportName, IDBConnectionWrapper db)
        {
            int i;
            String[] familyName = reportName.Split(new char[] { ':' }, 2);
            if (familyName.Length != 2)
                throw new InvalidOperationException("Invalid report name");

            if (familyName[0].IndexOfAny(new char[] { '\\', '/', ':' }) >= 0 ||
                familyName[1].IndexOfAny(new char[] { '\\', '/', ':' }) >= 0)
                throw new InvalidOperationException("Invalid report name");

            String destination = Path.Combine("Reports", String.Format("{0}.rpt", familyName[1]));

            using (IDataReader reader = db.OpenDataReader("select modifydate, data from plugin where type=19 and family=? and name=?",
                familyName[0], familyName[1]))
            {
                if (!reader.Read())
                    throw new InvalidOperationException("Report " + reportName + " not found.");
                DateTime modifyDate = reader.GetDateTime(0);
                if (File.Exists(destination) && File.GetLastWriteTime(destination) >= modifyDate)
                    return destination;
                byte[] buffer = (byte[])reader[1];
                for (i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] == 0xd0 && buffer[i + 1] == 0xcf && buffer[i + 2] == 0x11)
                        break;
                    if (i >= buffer.Length)
                    {
                        // For troubleshooting..
                        //using (FileStream output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                        //{
                        //    output.Write(buffer, i, buffer.Length);
                        //}
                        throw new Exception("Invalid plugin data (could not locate report data)");
                    }
                }
                using (FileStream output = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    output.Write(buffer, i, buffer.Length - i);
                }
            }

            return destination;
        }

    }
}
