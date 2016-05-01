using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.Shared
{
    public class SmtpClientWrapper : ISmtpClientWrapper
    {
        private static SmtpClient _client = null;
        private static SmtpClient InitializeSmtpClient()
        {
            if (_client != null)
                return _client;
            SmtpClient client = new SmtpClient();
            // use default settings specified in mailSettings
            client.Timeout = 60 * 60 * 1000; //1h

            return _client = client;
        }

        public void Send(MailMessage msg)
        {
            // XXX this should be async
            InitializeSmtpClient().Send(msg);
        }
    }
}
