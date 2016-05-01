using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSSWorld.RFI.NotificationGenerator.Interfaces
{
    /// <summary>
    /// Represent a single match of alert, i.e. we need to fill and send the template to this destination
    /// </summary>
    public class AlertMatch
    {
        public string TicketId { get; set; }
        public Recipient Recipient { get; set; }
    }
}
