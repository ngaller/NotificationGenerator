using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace SSSWorld.RFI.NotificationGenerator.Interfaces
{
    /// <summary>
    /// Wrapper for SmtpClient, used for testing
    /// </summary>
    public interface ISmtpClientWrapper
    {
        void Send(MailMessage msg);
    }
}
