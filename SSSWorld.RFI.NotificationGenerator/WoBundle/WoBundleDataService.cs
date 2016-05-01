using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.WoBundle
{
    public class WoBundleDataService : IDataService<WoBundleAlertMatch, WoBundleAlertTemplate>
    {
        private readonly IDBConnectionWrapper _db;
        private readonly string _attachmentPath;
        private readonly string _libraryPath;

        public WoBundleDataService(IDBConnectionWrapper db)
        {
            _db = db;
            _attachmentPath = (string)_db.GetField("ATTACHMENTPATH", "BRANCHOPTIONS", "1=1");
            _libraryPath = (string)_db.GetField("SalesLibraryPath", "BranchOptions", "1=1");
        }

        public void LoadData(WoBundleAlertMatch alert, WoBundleAlertTemplate alertTemplate)
        {
            // In this order:
            // - Renovate Rights, if needed
            // - Lead Test, if needed
            // - Lead safe checklist, if needed
            // - RFI Attachments
            // - Customer approval (if install)
            // - Additional documents
            if (NeedRenovateRight(alert))
            {
                alert.Attachments.Add(new StaticReportDocument { OutputName = "Renovate Rights", ReportFilename = "Reports\\Renovate Right.rpt" });
            }
            if (NeedLeadTest(alert))
            {
                alert.Attachments.Add(new StaticReportDocument { OutputName = "Lead Test", ReportFilename = "Reports\\Lead Test.rpt" });
            }
            if (NeedLeadSafe(alert))
            {
                alert.Attachments.Add(new SlxReportDocument { OutputName = "Renovate Rights", ReportName = "Form Docs:Lead Safe Renovation Checklist" });
            }
            alert.Attachments.AddRange(GetRFIAttachments(alert.TicketId));
            bool isInstall = alert.JobType == "Install";
            if (isInstall)
            {
                alert.Attachments.Add(new SlxReportDocument { OutputName = "Customer Approval", ReportName = GetCustomCAReport(alert.TicketId) });
            }
            alert.Attachments.AddRange(GetAdditionalDocuments(alert.TicketId, alert.YearHomeBuilt.GetValueOrDefault(), alert.JobTypeId, isInstall));
        }

        private bool NeedLeadSafe(WoBundleAlertMatch alert)
        {
            return alert.YearHomeBuilt.GetValueOrDefault() < 1978 && alert.LeadPaintFound && !HasChecklistDocument(alert.TicketId);
        }

        private bool NeedLeadTest(WoBundleAlertMatch alert)
        {
            return NeedRenovateRight(alert) && !HasLeadTestDocument(alert.TicketId);
        }

        private bool NeedRenovateRight(WoBundleAlertMatch alert)
        {
            return alert.YearHomeBuilt.GetValueOrDefault() < 1978 && alert.JobType == "Measure";
        }

        /// <summary>
        /// Return list of PDF attached to the WO
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        private IEnumerable<FileAttachment> GetRFIAttachments(String ticketId)
        {
            using (var reader = _db.OpenDataReader(@"select FILENAME, ATTACHID, DESCRIPTION from ATTACHMENT 
                where TICKETID = ? and ATTACHMENT.FILESIZE < 20000000 and ATTACHMENT.FILENAME like '%pdf' 
                order by case when description like '%po%' then 1 when description like '%measure%' then 2 else 3 end, ATTACHDATE",
                ticketId))
            {
                while (reader.Read())
                {
                    yield return new FileAttachment
                    {
                        Id = reader["ATTACHID"].ToString(),
                        OutputName = reader["DESCRIPTION"].ToString(),
                        Path = Path.Combine(_attachmentPath, reader["FILENAME"].ToString())
                    };
                }
            }
        }

        /// <summary>
        /// Additional documents associated with the store and job type
        /// </summary>
        private IEnumerable<DocumentToAttach> GetAdditionalDocuments(string ticketId, int yearHomeBuilt, String jobTypeId, bool install)
        {
            if (jobTypeId == null)
            {
                throw new ArgumentNullException(nameof(jobTypeId));
            }
            String yearHomeBuiltCondition = "";
            if (yearHomeBuilt >= 1978)
            {
                yearHomeBuiltCondition = "AND (ISNULL(F.PRE_1978_ONLY,'F') = 'F') ";
            }
            using (var reader = _db.OpenDataReader(@"select f.report_name, f.library_path from C_FORM_DOC F 
                            JOIN C_ACC_FORM AF ON AF.C_FORM_DOCID=F.C_FORM_DOCID 
                            JOIN TICKET T ON T.STORE_ACCOUNTID=AF.ACCOUNTID
                            WHERE AF.BUNDLE='T' " + yearHomeBuiltCondition + @"
                                AND (F.JOB_TYPE IS NULL OR F.JOB_TYPE LIKE ? AND (ISNULL(F.HDMS,'F')='F' or T.HDMS_MEASURED='T'))                                       
                                AND (F.JOBTYPEID IS NULL OR F.JOBTYPEID = ?)
                                AND F.FORMTYPE = 'Additional Doc' AND T.TICKETID=?", "%" + (install ? "Install" : "Measure") + "%", jobTypeId, ticketId))
            {
                while (reader.Read())
                {
                    if (reader[1].ToString() != "")
                    {
                        yield return new FileAttachment
                        {
                            Id = null,
                            Path = GetLibraryFile(reader[1].ToString())
                        };
                    }
                    else
                    {
                        yield return new SlxReportDocument
                        {
                            ReportName = "Form Docs:" + reader[0],
                            OutputName = reader[0].ToString()
                        };
                    }
                }
            }
        }

        private string GetCustomCAReport(string ticketNumber)
        {
            String reportName = (String)_db.GetField("F.REPORT_NAME", @"C_FORM_DOC F 
                            JOIN C_ACC_FORM AF ON AF.C_FORM_DOCID=F.C_FORM_DOCID 
                            JOIN TICKET T ON T.STORE_ACCOUNTID=AF.ACCOUNTID",
                            "AF.BUNDLE='T' AND F.FORMTYPE='CA' AND T.TICKETID=?", ticketNumber);
            if (reportName == null)
                // default report
                return "Form Docs:RFI CA";
            return "Form Docs:" + reportName;
        }


        /// <summary>
        /// Retrieve path of library file
        /// </summary>
        /// <param name="libraryFilePath"></param>
        /// <returns>Absolute path to file</returns>
        private string GetLibraryFile(string libraryFilePath)
        {
            if (libraryFilePath.Contains(".."))
                throw new Exception("Invalid library path");
            return Path.Combine(_libraryPath, libraryFilePath);
        }


        /// <summary>
        /// Return true if the ticket's store has a checklist form doc already.
        /// If that is the case then we don't need to include the lead safe checklist
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        private bool HasChecklistDocument(string ticketId)
        {
            // OK to use the quickdocument collection because this code is called from the QuickDoc form where we already load it anyway
            DataSet ds = _db.OpenDataSet(@"select ticketid from sysdba.ticket t
                                                    left join sysdba.ACCOUNT a on t.STORE_ACCOUNTID = a.accountid
                                                    left join sysdba.C_ACC_FORM af on af.ACCOUNTID = a.ACCOUNTID
                                                    left join sysdba.c_form_doc fd on fd.c_form_docid = af.c_form_docid 
                                                    where af.FORMTYPE = 'Additional Doc' 
                                                    and fd.REPORT_NAME like '%checklist%' 
                                                    and t.ticketid = '" + ticketId + "'");
            return (ds.Tables[0].Rows.Count > 0);
        }


        private bool HasLeadTestDocument(string ticketId)
        {
            // OK to use the quickdocument collection because this code is called from the QuickDoc form where we already load it anyway
            DataSet ds = _db.OpenDataSet(@"select ticketid from sysdba.ticket t
                                                    left join sysdba.ACCOUNT a on t.STORE_ACCOUNTID = a.accountid
                                                    left join sysdba.C_ACC_FORM af on af.ACCOUNTID = a.ACCOUNTID
                                                    left join sysdba.c_form_doc fd on fd.c_form_docid = af.c_form_docid 
                                                    where af.FORMTYPE = 'Additional Doc' 
                                                    and (fd.REPORT_NAME like '%lead test%' or 
                                                        fd.REPORT_NAME like '%test kit%')
                                                    and t.ticketid = '" + ticketId + "'");
            return (ds.Tables[0].Rows.Count > 0);
        }
    }
}
