using System.Collections.Generic;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.CustomerNotifications
{
    public class CustomerNotifAlertMatch : AlertMatch
    {
        public CustomerNotifAlertMatch()
        {
            this.Attachments = new List<DocumentToAttach>();
            this.WorkOrderData = new Dictionary<string, string>();
        }

        public List<DocumentToAttach> Attachments { get; private set; }
        public Dictionary<string, string> WorkOrderData { get; private set; }
        /// <summary>
        /// Set for manual requests only
        /// </summary>
        public string ManualRequestId { get; set; }
    }
}
