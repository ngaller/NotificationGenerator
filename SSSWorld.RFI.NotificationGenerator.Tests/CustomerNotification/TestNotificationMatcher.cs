using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.CustomerNotifications;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.Tests.CustomerNotification
{
    [TestFixture]
    public class TestNotificationMatcher
    {
        private DBConnectionWrapper _db;
        private NotificationMatcher _matcher;

        [Test]
        public void SelectCustomersByWoStatus()
        {
            var tickId = (string)_db.GetField("TICKETID", "TICKET T JOIN CONTACT C ON C.CONTACTID = T.CUSTOMERID",
                "C.EMAIL IS NOT NULL AND T.STATUSCODE='" + Constants.STATUS_SCHEDULED + "'");
            var accId = (string)_db.GetField("S.PARENTID", "TICKET T JOIN ACCOUNT S ON S.ACCOUNTID = T.STORE_ACCOUNTID", "T.TICKETID=?", tickId);
            var customerEmail = (string)_db.GetField("C.EMAIL", "TICKET T JOIN CONTACT C ON C.CONTACTID = T.CUSTOMERID", "T.TICKETID=?", tickId);
            _db.ExecuteSQL("UPDATE TICKET SET MODIFYDATE=? WHERE TICKETID=?", DateTime.Today, tickId);
            _db.ExecuteSQL("DELETE FROM TICKETACTIVITY WHERE TICKETID=?", tickId);
            var tpl = new CustomerNotifAlertTemplate
            {
                AccountId = accId,
                Name = "Testing",
                WoStatus = Constants.STATUS_SCHEDULED
            };
            var result = _matcher.GetAlertMatches(tpl);
            Assert.Greater(result.Count, 0);
            var match = result.FirstOrDefault(r => r.TicketId == tickId);
            Assert.IsNotNull(match);
            Assert.AreEqual(customerEmail, match.Recipient.RecipientAddress);
        }

        [Test]
        public void SelectCustomersByDocumentType()
        {
            var tickId = (string)_db.GetField("TICKETID", "TICKET T JOIN CONTACT C ON C.CONTACTID = T.CUSTOMERID", 
                "C.EMAIL IS NOT NULL AND TICKETID IN (SELECT TICKETID FROM ATTACHMENT WHERE DOCUMENTTYPE='Completed Measure')");
            var accId = (string)_db.GetField("S.PARENTID", "TICKET T JOIN ACCOUNT S ON S.ACCOUNTID = T.STORE_ACCOUNTID", "T.TICKETID=?", tickId);
            _db.ExecuteSQL("UPDATE TICKET SET MODIFYDATE=? WHERE TICKETID=?", DateTime.Today, tickId);
            _db.ExecuteSQL("DELETE FROM TICKETACTIVITY WHERE TICKETID=?", tickId);
            var tpl = new CustomerNotifAlertTemplate
            {
                AccountId = accId,
                Name = "Testing",
                DocumentType = "Completed Measure"
            };
            var result = _matcher.GetAlertMatches(tpl);
            Assert.Greater(result.Count, 0);
            var match = result.FirstOrDefault(r => r.TicketId == tickId);
            Assert.IsNotNull(match);
        }

        [Test]
        public void SelectCustomersByStatusAbove()
        {
            var tickId = (string)_db.GetField("TICKETID", "TICKET T JOIN CONTACT C ON C.CONTACTID = T.CUSTOMERID",
                "C.EMAIL IS NOT NULL AND T.STATUSCODE='" + Constants.STATUS_COMPLETED + "'");
            var accId = (string)_db.GetField("S.PARENTID", "TICKET T JOIN ACCOUNT S ON S.ACCOUNTID = T.STORE_ACCOUNTID", "T.TICKETID=?", tickId);
            _db.ExecuteSQL("UPDATE TICKET SET MODIFYDATE=? WHERE TICKETID=?", DateTime.Today, tickId);
            _db.ExecuteSQL("DELETE FROM TICKETACTIVITY WHERE TICKETID=?", tickId);
            var tpl = new CustomerNotifAlertTemplate
            {
                AccountId = accId,
                WoStatus = Constants.STATUS_SCHEDULED,
                Name = "Testing",
                IncludeStatusAbove = true
            };
            var result = _matcher.GetAlertMatches(tpl);
            Assert.Greater(result.Count, 0);
            var match = result.FirstOrDefault(r => r.TicketId == tickId);
            Assert.IsNotNull(match);
        }

        [Test]
        public void DoNotSelectCustomersWhoHaveReceivedTheNotification()
        {
            var tickId = (string)_db.GetField("TICKETID", "TICKET T JOIN CONTACT C ON C.CONTACTID = T.CUSTOMERID",
                "C.EMAIL IS NOT NULL AND T.STATUSCODE='" + Constants.STATUS_COMPLETED + "'");
            var accId = (string)_db.GetField("S.PARENTID", "TICKET T JOIN ACCOUNT S ON S.ACCOUNTID = T.STORE_ACCOUNTID", "T.TICKETID=?", tickId);
            _db.ExecuteSQL("UPDATE TICKET SET MODIFYDATE=? WHERE TICKETID=?", DateTime.Today, tickId);
            _db.ExecuteSQL("DELETE FROM TICKETACTIVITY WHERE TICKETID=?", tickId);
            var tpl = new CustomerNotifAlertTemplate
            {
                Name = "Booyah",
                AccountId = accId,
                WoStatus = Constants.STATUS_COMPLETED
            };
            _matcher.RecordSentAlert(new CustomerNotifAlertMatch { TicketId = tickId, Recipient = new Recipient() }, tpl);
            var result = _matcher.GetAlertMatches(tpl);
            var match = result.FirstOrDefault(r => r.TicketId == tickId);
            Assert.IsNull(match);
        }

        [Test]
        public void TestGetManualRequests()
        {
            var tickId = (string)_db.GetField("TICKETID", "TICKET T JOIN CONTACT C ON C.CONTACTID = T.CUSTOMERID",
                "C.EMAIL IS NOT NULL AND T.STATUSCODE='" + Constants.STATUS_COMPLETED + "'");
            var accId = (string)_db.GetField("S.PARENTID", "TICKET T JOIN ACCOUNT S ON S.ACCOUNTID = T.STORE_ACCOUNTID", "T.TICKETID=?", tickId);
            _db.ExecuteSQL("UPDATE TICKET SET MODIFYDATE=? WHERE TICKETID=?", DateTime.Today, tickId);
            _db.ExecuteSQL("DELETE FROM TICKETACTIVITY WHERE TICKETID=?", tickId);
            var tpl = new CustomerNotifAlertTemplate
            {
                Name = "Booyah",
                AccountId = accId,
                WoStatus = "INVALID",
                Id = "456"
            };
            _db.ExecuteSQL("insert into ksrequesttable (ksrequesttableid, requesttype, requestorid, ticketid, email) values ('123', 'Customer Notif Preview', '456', ?, 'test@ne.cim')",
                tickId);
            var result = _matcher.GetAlertMatches(tpl);
            Assert.AreEqual(1, result.Count);
            _matcher.RecordSentAlert(result.First(), tpl);
            Assert.AreNotEqual(null, _db.GetField("completeddate", "ksrequesttable", "ksrequesttableid=?", "123"));
        }

        [Test]
        public void TestDoNotGetCompletedManualRequest()
        {
            var tickId = (string)_db.GetField("TICKETID", "TICKET T JOIN CONTACT C ON C.CONTACTID = T.CUSTOMERID",
                "C.EMAIL IS NOT NULL AND T.STATUSCODE='" + Constants.STATUS_COMPLETED + "'");
            var accId = (string)_db.GetField("S.PARENTID", "TICKET T JOIN ACCOUNT S ON S.ACCOUNTID = T.STORE_ACCOUNTID", "T.TICKETID=?", tickId);
            _db.ExecuteSQL("UPDATE TICKET SET MODIFYDATE=? WHERE TICKETID=?", DateTime.Today, tickId);
            _db.ExecuteSQL("DELETE FROM TICKETACTIVITY WHERE TICKETID=?", tickId);
            var tpl = new CustomerNotifAlertTemplate
            {
                Name = "Booyah",
                AccountId = accId,
                WoStatus = "INVALID",
                Id = "456"
            };
            _db.ExecuteSQL("insert into ksrequesttable (ksrequesttableid, requesttype, requestorid, ticketid, email, completeddate) values ('789', 'Customer Notif Preview', '456', ?, 'test@ne.cim', getdate())",
                tickId);
            var result = _matcher.GetAlertMatches(tpl);
            Assert.AreEqual(0, result.Count);
        }

        [SetUp]
        public void SetUp()
        {
            _db = new DBConnectionWrapper("SalesLogix");
            _db.BeginTransaction();
            _db.ExecuteSQL("delete from ksrequesttable");
            _matcher = new NotificationMatcher(_db);
        }

        [TearDown]
        public void TearDown()
        {
            _db.RollbackTransaction();
            _db.Dispose();
        }
    }
}
