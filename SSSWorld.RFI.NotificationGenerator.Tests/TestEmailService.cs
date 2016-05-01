using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.Tests
{
    [TestFixture]
    public class TestEmailService
    {
        [Test]
        public void TestSendMessageWithAttachments()
        {
            var smtp = new Mock<ISmtpClientWrapper>();
            var emailService = new EmailService(smtp.Object);
            emailService.SendEmail("test@test.com", null, "Title", "Body", new[] { new FileAttachment { Path = "Samples\\CA.pdf", OutputName = "Sample" } });
        }
    }
}
