using System;
using System.Net.Mail;
using log4net;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.Shared
{
    public class EmailService : IEmailService
    {
        private readonly ISmtpClientWrapper _smtpClientWrapper;
        private static readonly ILog LOG = LogManager.GetLogger(typeof(Program));

        public EmailService(ISmtpClientWrapper smtpClientWrapper)
        {
            _smtpClientWrapper = smtpClientWrapper;
        }

        public bool SendEmail(string emailTo, string emailCc, string emailTitle, string body, params FileAttachment[] attachments)
        {
            try
            {
                MailMessage msg = new MailMessage();
                msg.Body = body;
                msg.Subject = emailTitle;
                msg.IsBodyHtml = body.Contains("<");
                foreach (String email in emailTo.Split(',', ';'))
                {
                    if (email != "")
                        msg.To.Add(email.Trim());
                }                
                if (!String.IsNullOrEmpty(emailCc))
                {
                    String[] ccs = emailCc.Split(';');
                    foreach (String c in ccs)
                    {
                        msg.Bcc.Add(c);
                    }
                }

                foreach (var attach in attachments)
                {
                    Attachment mailAttachment = new Attachment(attach.Path);
                    if(!string.IsNullOrEmpty(attach.OutputName))
                        mailAttachment.ContentDisposition.FileName = attach.OutputName;
                    msg.Attachments.Add(mailAttachment);
                }

                _smtpClientWrapper.Send(msg);
                return true;
            }
            catch (FormatException x)
            {
                LOG.Debug("Invalid email address " + emailTo, x);
                return false;
            }
            catch (Exception x)
            {
                // we need to have error handling here so that the process can go on...
                LOG.Error("Error sending " + emailTitle + " to " + emailTo, x);
                return false;
            }
        }

    }
}
