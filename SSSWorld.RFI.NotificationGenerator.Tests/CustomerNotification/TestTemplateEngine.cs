using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSSWorld.RFI.NotificationGenerator.CustomerNotifications;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.Tests.CustomerNotification
{
    [TestFixture]
    public class TestTemplateEngine
    {
        [Test]
        public void TestPopulateTemplateSimple()
        {
            TemplateEngine eng = new TemplateEngine(null);
            CustomerNotifAlertTemplate template = new CustomerNotifAlertTemplate
            {
                EmailText = "bla bla bla {{testing}} {{something}}"
            };
            CustomerNotifAlertMatch match = new CustomerNotifAlertMatch
            {
                Recipient = new Recipient(), TicketId = "123"
            };
            match.WorkOrderData["testing"] = "XXX";
            var result = eng.PopulateTemplate(template, new[] { match }).First();
            Assert.AreEqual("bla bla bla XXX ", result.AlertText);
        }

        [Test]
        public void TestExpandVariablesInSubject()
        {
            TemplateEngine eng = new TemplateEngine(null);
            CustomerNotifAlertTemplate template = new CustomerNotifAlertTemplate
            {
                EmailSubject = "bla bla bla {{testing}} {{something}}"
            };
            CustomerNotifAlertMatch match = new CustomerNotifAlertMatch
            {
                Recipient = new Recipient(), TicketId = "123"
            };
            match.WorkOrderData["testing"] = "XXX";
            var result = eng.PopulateTemplate(template, new[] { match }).First();
            Assert.AreEqual("bla bla bla XXX ", result.AlertSubject);
        }
    }
}
