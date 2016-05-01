using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.Shared
{
    /// <summary>
    /// Represents report that is stored as a static file under the application folder
    /// </summary>
    public class StaticReportDocument : DocumentToAttach
    {
        /// <summary>
        /// Path to rpt file (absolute or relative to application directory)
        /// </summary>
        public String ReportFilename { get; set; }
        /// <summary>
        /// Substitute report parameters - by default we'll pass in the ticket id but this can be overridden via this property
        /// </summary>
        public Dictionary<string, object> ReportParameters { get; set; }
    }
}
