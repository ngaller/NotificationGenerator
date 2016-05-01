using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.WoBundle
{
    /// <summary>
    /// What are existing records matching this particular template (i.e. who do we need to send the notification to).
    /// In this case we need to look at tickets for which we are ready to send the bundle.
    /// </summary>
    public class WoBundleNotificationMatcher : INotificationMatcher<WoBundleAlertTemplate, WoBundleAlertMatch>
    {
        private readonly IDBConnectionWrapper _db;

        public WoBundleNotificationMatcher(IDBConnectionWrapper db)
        {
            _db = db;
        }

        public IList<WoBundleAlertMatch> GetAlertMatches(WoBundleAlertTemplate alertTemplate)
        {
            DataSet ds = _db.OpenDataSet(@"select T.TICKETID, C.EMAIL, T.YEARHOMEBUILT, JT.JOB_TYPE, T.TICKETNUMBER, T.LEADPAINTFOUND, T.JOBTYPEID
                from TICKET T
                JOIN C_JOBTYPES JT on JT.C_JOBTYPESID=T.JOBTYPEID 
                JOIN CONTACT C on C.CONTACTID = T.CONTACTID 
                where T.CONTACTID = ? and T.SCHEDULEDDATE >= ?  and T.STATUSCODE <> ?
                and T.SCHEDULEDDATE < ? and (T.ISREADY = 'F' 
                or T.ISREADY is null) order by T.SCHEDULEDDATE",
                alertTemplate.CrewMemberId, alertTemplate.StartDate, Constants.STATUS_CANCELED, alertTemplate.EndDate);

            return (from DataRow dr in ds.Tables[0].Rows
                    select new WoBundleAlertMatch
                    {
                        TicketId = dr[0].ToString(),
                        Recipient = alertTemplate.Recipient,
                        YearHomeBuilt = dr["YEARHOMEBUILT"] == DBNull.Value ? null : (short?)dr["YEARHOMEBUILT"],
                        JobType = dr["JOB_TYPE"].ToString(),
                        TicketNumber = dr["TICKETNUMBER"].ToString(),
                        LeadPaintFound = dr["LEADPAINTFOUND"].ToString() == "T",
                        JobTypeId = dr["JOBTYPEID"].ToString()
                    }).ToList();
        }

        public void RecordSentAlert(WoBundleAlertMatch alert, WoBundleAlertTemplate alertTemplate)
        {
            _db.ExecuteSQL(
                "UPDATE TICKET set ISREADY = 'T' where TICKETID = ?", alert.TicketId);
            SlxDataHelper.AddTicketActivityDirect(_db, alert.TicketId,
                "WO Bundle sent to " + alert.Recipient.RecipientAddress);
        }
    }
}
