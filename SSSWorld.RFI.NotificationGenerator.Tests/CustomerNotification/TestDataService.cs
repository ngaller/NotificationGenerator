using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.CustomerNotifications;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.Tests.CustomerNotification
{
    [TestFixture]
    public class TestDataService
    {
        private DBConnectionWrapper _db;
        private DataService _data;

        [Test]
        public void TestIncludeAttachment()
        {
            var tickId = (string)_db.GetField("TICKETID", "TICKET T JOIN CONTACT C ON C.CONTACTID = T.CUSTOMERID JOIN CONTACT CREW ON CREW.CONTACTID=T.CONTACTID",
                "C.EMAIL IS NOT NULL AND TICKETID IN (SELECT TICKETID FROM ATTACHMENT WHERE DOCUMENTTYPE='Completed Measure')");
            var match = new CustomerNotifAlertMatch
            {
                TicketId = tickId
            };
            var tpl = new CustomerNotifAlertTemplate
            {
                DocumentType = "Completed Measure"
            };
            _data.LoadData(match, tpl);
            Assert.AreEqual(1, match.Attachments.Count);
        }

        [Test]
        public void TestPopulateTicketData()
        {
            var tickId = (string)_db.GetField("TICKETID", "TICKET T JOIN CONTACT C ON C.CONTACTID = T.CUSTOMERID JOIN CONTACT CREW ON CREW.CONTACTID=T.CONTACTID",
                "C.EMAIL IS NOT NULL AND CREW.WORKPHONE IS NOT NULL AND TICKETID IN (SELECT TICKETID FROM ATTACHMENT WHERE DOCUMENTTYPE='Completed Measure')");
            var match = new CustomerNotifAlertMatch
            {
                TicketId = tickId
            };
            var tpl = new CustomerNotifAlertTemplate
            {
                DocumentType = "Completed Measure"
            };
            _data.LoadData(match, tpl);
            Assert.Less(0, match.WorkOrderData.Count);
            Assert.IsNotNullOrEmpty(match.WorkOrderData["fieldmanager"]);
            Assert.IsTrue(Regex.IsMatch(match.WorkOrderData["crewmember_phone"], @"\(\d{3}\) \d{3}-\d{4}"));
            Assert.IsTrue(Regex.IsMatch(match.WorkOrderData["unsubscribe_link"], @"http.*unsubscribe.ashx", RegexOptions.IgnoreCase));
        }

        [SetUp]
        public void SetUp()
        {
            _db = new DBConnectionWrapper("SalesLogix");
            _db.BeginTransaction();
            _data = new DataService(_db, new Configuration(_db));
        }

        [TearDown]
        public void TearDown()
        {
            _db.RollbackTransaction();
            _db.Dispose();
        }
    }
}
