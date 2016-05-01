using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSSWorld.RFI.NotificationGenerator.Interfaces
{
    /// <summary>
    /// Representation of a document to be attached: can be either a report to generate, or a file already in the system
    /// </summary>
    public abstract class DocumentToAttach
    {
        public String OutputName { get; set; }
    }
}
