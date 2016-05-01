using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.Interfaces
{
    /// <summary>
    /// A completely instantiated and ready to send alert
    /// </summary>
    public class PopulatedTemplate
    {
        public string AlertSubject { get; set; }
        public string AlertText { get; set; }
        /// <summary>
        /// Email address
        /// </summary>
        public Recipient Recipient { get; set; }
        /// <summary>
        /// Optional attachments
        /// </summary>
        public FileAttachment[] Attachments { get; set; }
        /// <summary>
        /// What alerts we generated the template from.
        /// Needed for tracking in case more than one matches were combined into one template
        /// </summary>
        public IList<AlertMatch> PopulatedFrom;
    }
}
