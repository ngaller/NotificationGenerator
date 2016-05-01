using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.WoBundle
{
    /// <summary>
    /// WO Bundle template is extracted from the KS Request table and designate a specific crew member + scheduled date or scheduled date range.
    /// </summary>
    public class WoBundleAlertTemplate : AlertTemplate
    {
        public String CrewMemberId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        /// <summary>
        /// For these we can have the recipient at the template level since we are sending to a single contact each time
        /// </summary>
        public Recipient Recipient { get; set; }
    }
}
