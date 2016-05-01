using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.WoBundle;

namespace SSSWorld.RFI.NotificationGenerator.Tests.WoBundle
{
    /// <summary>
    /// Test for the WO bundle matcher.
    /// This looks in the ticket table for some specific data like contact id and scheduled date (specified on the request)
    /// </summary>
    [TestFixture]
    public class TestNotificationMatcher
    {
        private DBConnectionWrapper _db;
        private string _conId, _tick1Id, _tick2Id, _tick3Id;

        [Test]
        public void ShouldSendOnlyNotificationsForCurrentSchedule()
        {
            WoBundleNotificationMatcher matcher = new WoBundleNotificationMatcher(_db);
            // Need to make sure that the matched WOs include only the ones that are scheduled on the same date as the alert parameter.

            _db.ExecuteSQL("update ticket set isready='F', scheduleddate = ? where ticketid = ?", DateTime.Today.AddHours(7), _tick1Id);
            _db.ExecuteSQL("update ticket set isready='F', scheduleddate = ? where ticketid = ?", DateTime.Today.AddDays(-1).AddHours(19), _tick2Id);
            _db.ExecuteSQL("update ticket set isready='F', scheduleddate = ? where ticketid = ?", DateTime.Today.AddHours(7).AddDays(1), _tick3Id);
            var results = matcher.GetAlertMatches(new WoBundleAlertTemplate { CrewMemberId = _conId, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) }).ToList();
            Assert.Greater(results.Count, 0);
            Assert.IsTrue(results.Any(v => v.TicketId == _tick1Id), "Ticket id was not matched");
            Assert.IsFalse(results.Any(v => v.TicketId == _tick2Id), "Ticket id should not be matched");
            Assert.IsFalse(results.Any(v => v.TicketId == _tick3Id), "Ticket id should not be matched");
        }

        [Test]
        public void ShouldNotResendNotifications()
        {
            WoBundleNotificationMatcher matcher = new WoBundleNotificationMatcher(_db);

            _db.ExecuteSQL("update ticket set isready='T', scheduleddate = ? where ticketid = ?", DateTime.Today.AddHours(7), _tick1Id);
            var results = matcher.GetAlertMatches(new WoBundleAlertTemplate { CrewMemberId = _conId, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(1) }).ToList();
            Assert.IsFalse(results.Any(v => v.TicketId == _tick1Id), "Ticket id should not be matched");
        }
        
        [Test]
        public void ShouldTrackSentNotifications()
        {
            WoBundleNotificationMatcher matcher = new WoBundleNotificationMatcher(_db);

            _db.ExecuteSQL("update ticket set isready='F', scheduleddate = ? where ticketid = ?", DateTime.Today.AddHours(7), _tick1Id);
            WoBundleAlertMatch alert = new WoBundleAlertMatch { TicketId = _tick1Id, Recipient = new Recipient { RecipientAddress = "joe@test.com"} };
            _db.ExecuteSQL("delete from ticketactivity where ticketid=?", _tick1Id);
            matcher.RecordSentAlert(alert, new WoBundleAlertTemplate());
            Assert.AreEqual("T", (string) _db.GetField("isready", "ticket", "ticketid=?", _tick1Id));
            Assert.AreEqual(1, (int)_db.GetField("count(*)", "ticketactivity", "ticketid=?", _tick1Id));
        }

        [SetUp]
        public void SetUp()
        {
            _db = new DBConnectionWrapper("SalesLogix");
            _conId = (string)_db.DoSQL("select top 1 contactid from sysdba.ticket where statuscode=? and scheduleddate is not null order by scheduleddate desc", Constants.STATUS_SCHEDULED);
            _tick1Id = (string)_db.DoSQL("select ticketid from ticket where statuscode=? and contactid = ? order by scheduleddate desc", Constants.STATUS_SCHEDULED, _conId);
            _tick2Id = (string)_db.DoSQL("select ticketid from ticket where statuscode=? and contactid = ? and ticketid <> ? order by scheduleddate desc", Constants.STATUS_SCHEDULED, _conId, _tick1Id);
            _tick3Id = (string)_db.DoSQL("select ticketid from ticket where statuscode=? and contactid = ? and ticketid not in (?, ?) order by scheduleddate desc", Constants.STATUS_SCHEDULED, _conId, _tick1Id, _tick2Id);
            _db.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            _db.RollbackTransaction();
            _db.Dispose();
        }
    }
}
