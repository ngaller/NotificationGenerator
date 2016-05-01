using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.CustomerNotifications
{
    public class CustomerNotifAlertTemplate : AlertTemplate
    {
        public string Name { get; set; }
        public string AccountId { get; set; }
        public string WoStatus { get; set; }
        public bool IncludeStatusAbove { get; set; }
        public string JobTypeCategory { get; set; }
        public string DocumentType { get; set; }
        public string EmailSubject { get; set; }
        public string EmailText { get; set; }
    }
}
