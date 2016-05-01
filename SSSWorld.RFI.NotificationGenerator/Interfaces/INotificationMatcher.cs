using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSSWorld.RFI.NotificationGenerator.Interfaces
{
    /// <summary>
    /// What are existing records matching this particular template (i.e. who do we need to send the notification to).
    /// Also used to track the sent notifications, since we need to use that data to avoid sending the same notification twice.
    /// </summary>
    public interface INotificationMatcher<TAlertTemplate, TAlertMatch>
        where TAlertTemplate : AlertTemplate
        where TAlertMatch : AlertMatch
    {
        IList<TAlertMatch> GetAlertMatches(TAlertTemplate alertTemplate);

        void RecordSentAlert(TAlertMatch alert, TAlertTemplate populated);
    }
}
