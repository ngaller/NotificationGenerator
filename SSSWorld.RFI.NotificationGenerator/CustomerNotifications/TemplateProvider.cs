using System;
using System.Collections.Generic;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.CustomerNotifications
{
    /// <summary>
    /// Read data from the C_ACC_EMAILALERT table
    /// TODO Manual requests
    /// </summary>
    public class TemplateProvider : ITemplateProvider<CustomerNotifAlertTemplate>
    {
        private readonly IDBConnectionWrapper _db;

        public TemplateProvider(IDBConnectionWrapper db)
        {
            _db = db;
        }

        public IEnumerable<CustomerNotifAlertTemplate> GetAvailableTemplates()
        {
            using (var reader = _db.OpenDataReader("select * from c_acc_email_alert"))
            {
                var result = new List<CustomerNotifAlertTemplate>();
                while (reader.Read())
                {
                    result.Add(new CustomerNotifAlertTemplate
                    {
                        Id = reader["C_ACC_EMAIL_ALERTID"].ToString(),
                        Name = reader["NAME"].ToString(),
                        AccountId = reader["ACCOUNTID"].ToString(),
                        WoStatus = reader["WO_STATUS"].ToString(),
                        IncludeStatusAbove = reader["INCLUDE_STATUS_ABOVE"].ToString() == "T",
                        JobTypeCategory = reader["JOB_TYPE_CATEGORY"].ToString(),
                        DocumentType = reader["DOCUMENT_TYPE"].ToString(),
                        EmailSubject = reader["EMAIL_SUBJECT"].ToString(),
                        EmailText = reader["EMAIL_TEXT"].ToString(),
                    });
                }
                return result;
            }
        }

        public void RecordProcessedTemplate(CustomerNotifAlertTemplate alert)
        {
            // nothing to do at this level
        }
    }
}