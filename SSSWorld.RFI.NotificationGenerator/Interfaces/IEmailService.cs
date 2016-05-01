using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.Interfaces
{
    public interface IEmailService
    {
        bool SendEmail(string emailTo, string emailCc, string emailTitle, string body, params FileAttachment[] attachments);
    }
}