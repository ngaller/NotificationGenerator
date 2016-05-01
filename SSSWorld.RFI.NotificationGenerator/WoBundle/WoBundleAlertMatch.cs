using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.WoBundle
{
    public class WoBundleAlertMatch : AlertMatch
    {
        public List<DocumentToAttach> Attachments { get; private set; }
        public string JobTypeId { get; set; }
        public bool LeadPaintFound { get; set; }
        public string TicketNumber { get; set; }
        /// <summary>
        /// Install or Measure
        /// </summary>
        public string JobType { get; set; }
        public short? YearHomeBuilt { get; set; }

        public WoBundleAlertMatch()
        {
            this.Attachments = new List<DocumentToAttach>();                 
        }
    }
}
