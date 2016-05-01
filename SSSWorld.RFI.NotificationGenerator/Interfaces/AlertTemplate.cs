using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSSWorld.RFI.NotificationGenerator.Interfaces
{
    /// <summary>
    /// Definition for a template.
    /// Not much  by default, but the customer email alerts will have a lot more options here
    /// </summary>
    public class AlertTemplate
    {
        public string Id { get; set; }
    }
}
