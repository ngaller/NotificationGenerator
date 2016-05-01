using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CrystalDecisions.ReportAppServer.CommonControls;
using log4net;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.WoBundle
{
    public class WoBundleTemplateProvider : ITemplateProvider<WoBundleAlertTemplate>
    {
        private readonly IDBConnectionWrapper _db;
        private static readonly ILog LOG = LogManager.GetLogger(typeof(WoBundleTemplateProvider));

        public WoBundleTemplateProvider(IDBConnectionWrapper db)
        {
            _db = db;
        }

        public IEnumerable<WoBundleAlertTemplate> GetAvailableTemplates()
        {
            List<WoBundleAlertTemplate> results = new List<WoBundleAlertTemplate>();
            using (var reader = _db.OpenDataReader("select K.KSREQUESTTABLEID, K.REQUESTTYPE, K.REQUESTORID, K.DATESTART, K.DATEEND, " +
                                                  "isnull(C.salutation, C.firstname) CONTACTNAME, C.PREFERRED_REPORTS, C.EMAIL, C.FAX " +
                                                  "from KSREQUESTTABLE K JOIN CONTACT C ON C.CONTACTID = K.REQUESTORID " +
                                                  "WHERE K.DATESTART IS NOT NULL AND K.DATEEND IS NOT NULL AND K.COMPLETEDDATE IS NULL and K.REQUESTTYPE = '1'"))
            {
                while (reader.Read())
                {
                    results.Add(new WoBundleAlertTemplate
                    {
                        CrewMemberId = reader["REQUESTORID"].ToString(),
                        Recipient = new Recipient
                        {
                            RecipientName = reader["CONTACTNAME"].ToString(),
                            RecipientAddress = GetEmailAddress(reader)
                        },
                        StartDate = (DateTime)reader["DATESTART"],
                        EndDate = (DateTime)reader["DATEEND"],
                        Id = reader["KSREQUESTTABLEID"].ToString()
                    });
                }
            }
            return results;
        }

        public void RecordProcessedTemplate(WoBundleAlertTemplate alert)
        {
            _db.ExecuteSQL("update KSRequestTable set CompletedDate=? where KSRequestTableId=?", _db.Now, alert.Id);
            LOG.Debug(alert.Id + "Completed execution of request");
        }

        /// <summary>
        /// Retrieve the e-mail address for the contact. 
        /// </summary>
        /// <param name="wrapper"></param>
        /// <param name="contactID"></param>
        /// <returns></returns>
        static string GetEmailAddress(IDataReader reader)
        {
            string sendEmail = "";
            if ("FAX".Equals(reader["PREFERRED_REPORTS"].ToString(), StringComparison.OrdinalIgnoreCase) && reader["FAX"].ToString() != "")
            {
                // should probably have something more generic, so we could do text messages too
                string faxNum = Regex.Replace(reader["FAX"].ToString(), "[^0-9]", "");
                if (!faxNum.StartsWith("1"))
                {
                    faxNum = "1" + faxNum;
                }
                return faxNum + "@myfax.com";
            }
            return reader["EMAIL"].ToString();
        }

    }
}
