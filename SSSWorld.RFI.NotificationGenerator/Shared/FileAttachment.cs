using System;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.Shared
{
    /// <summary>
    /// A static file to be attached
    /// </summary>
    public class FileAttachment : DocumentToAttach
    {
        /// <summary>
        /// Optional id (e.g. for SLX attachments)
        /// </summary>
        public String Id { get; set; }
        /// <summary>
        /// Absolute path to the file
        /// </summary>
        public String Path { get; set; }
    }
}
