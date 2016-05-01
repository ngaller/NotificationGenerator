using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SSSWorld.Common;
using SSSWorld.Common.Query;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.CustomerNotifications
{
    public class NotificationMatcher : INotificationMatcher<CustomerNotifAlertTemplate, CustomerNotifAlertMatch>
    {
        private readonly IDBConnectionWrapper _db;

        public NotificationMatcher(IDBConnectionWrapper db)
        {
            _db = db;
        }

        /// <summary>
        /// Apply the condition in the template to select a set of work orders.
        /// For performance reason we should look only at work orders that were modified recently.
        /// </summary>
        public IList<CustomerNotifAlertMatch> GetAlertMatches(CustomerNotifAlertTemplate alertTemplate)
        {
            List<CustomerNotifAlertMatch> matches = new List<CustomerNotifAlertMatch>();
            GetTemplateMatches(alertTemplate, matches);
            GetManualRequests(alertTemplate, matches);
            return matches;
        }

        private void GetManualRequests(CustomerNotifAlertTemplate alertTemplate, List<CustomerNotifAlertMatch> matches)
        {
            if(alertTemplate.Id == null)
                return;
            using (var reader = _db.OpenDataReader(@"select k.ticketid, k.email, k.ksrequesttableid from ksrequesttable k where 
                    requesttype = 'Customer Notif Preview' and requestorid = ? and completeddate is null", alertTemplate.Id))
            {
                while (reader.Read())
                {
                    matches.Add(new CustomerNotifAlertMatch
                    {
                        TicketId = reader[0].ToString(),
                        Recipient = new Recipient
                        {
                            RecipientAddress = reader[1].ToString(),
                            RecipientName = "Preview"
                        },
                        ManualRequestId = reader[2].ToString()
                    });
                }
            }
        }

        private void GetTemplateMatches(CustomerNotifAlertTemplate alertTemplate, List<CustomerNotifAlertMatch> matches)
        {
            var query = @"select t.ticketid, c.email, c.firstname + ' ' + c.lastname 
                    from ticket t 
                    join contact c on t.customerid=c.contactid 
                    join account store on store.accountid = t.store_accountid
                    join c_jobtypes jt on jt.c_jobtypesid = t.jobtypeid
                    where t.modifydate > getutcdate() - 10 and c.email is not null 
                    and store.parentid = ?
                    and (isnull(c.donotsolicit, 'F') = 'F' and isnull(c.donotemail, 'F') = 'F')
                    and not exists (select 1 from ticketactivity where ticketid=t.ticketid and userfield1=?) 
                    and ";
            var where = new List<string>();
            var paramValues = new ArrayList();
            paramValues.Add(alertTemplate.AccountId);
            paramValues.Add(alertTemplate.Name);
            PopulateWhere(alertTemplate, where, paramValues);
            if (!where.Any())
            {
                throw new ApplicationException("Must have at least one condition");
            }
            using (var reader = _db.OpenDataReader(query + string.Join(" and ", where.ToArray()), paramValues))
            {
                while (reader.Read())
                {
                    matches.Add(new CustomerNotifAlertMatch
                    {
                        TicketId = reader.GetString(0),
                        Recipient = new Recipient
                        {
                            RecipientAddress = reader[1].ToString(),
                            RecipientName = reader[2].ToString()
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Track the sent alert under ticket activities.
        /// This is done both for audit and to ensure we don't repeat alerts.
        /// </summary>
        public void RecordSentAlert(CustomerNotifAlertMatch alert, CustomerNotifAlertTemplate template)
        {
            if (string.IsNullOrEmpty(alert.ManualRequestId))
            {
                string description = $"Customer notification {template.Name} sent to {alert.Recipient.RecipientAddress}";
                SlxDataHelper.AddTicketActivityDirect(_db, alert.TicketId, description, template.Name);
            }
            else
            {
                _db.ExecuteSQL("update ksrequesttable set completeddate=? where ksrequesttableid=?", _db.Now, alert.ManualRequestId);
            }
        }

        /// <summary>
        /// Use criteria defined in alertTemplate to populate a list of condition and corresponding parameter values 
        /// to be passed when opening the data reader.
        /// </summary>
        private void PopulateWhere(CustomerNotifAlertTemplate alertTemplate, List<string> where, ArrayList paramValues)
        {
            if (!string.IsNullOrEmpty(alertTemplate.DocumentType))
            {
                where.Add("exists (select 1 from attachment where ticketid=t.ticketid and documenttype=?)");
                paramValues.Add(alertTemplate.DocumentType);
            }
            if (!string.IsNullOrEmpty(alertTemplate.WoStatus))
            {
                string c = "(t.statuscode = ?";
                paramValues.Add(alertTemplate.WoStatus);
                if (alertTemplate.IncludeStatusAbove)
                {
                    foreach (string status in GetStatusAbove(alertTemplate.WoStatus))
                    {
                        c += " or t.statuscode = ?";
                        paramValues.Add(status);
                    }
                }
                where.Add(c + ")");
            }
            if (!string.IsNullOrEmpty(alertTemplate.JobTypeCategory))
            {
                where.Add("jt.job_type = ?");
                paramValues.Add(alertTemplate.JobTypeCategory);
            }
        }

        private IEnumerable<string> GetStatusAbove(string woStatus)
        {
            switch (woStatus)
            {
                case Constants.STATUS_SCHEDULED:
                    return new[] { Constants.STATUS_COMPLETED, Constants.STATUS_EXPORTED };
                case Constants.STATUS_COMPLETED:
                    return new[] { Constants.STATUS_EXPORTED };
            }
            return new string[] { };
        }
    }
}
