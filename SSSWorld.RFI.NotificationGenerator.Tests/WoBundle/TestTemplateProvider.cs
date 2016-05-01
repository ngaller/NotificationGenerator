using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.WoBundle;

namespace SSSWorld.RFI.NotificationGenerator.Tests.WoBundle
{
    [TestFixture]
    public class TestTemplateProvider
    {
        private DBConnectionWrapper _db;
        private string _conId;

        [Test]
        public void ShouldMatchNonCompletedNotifications()
        {
            _db.ExecuteSQL("delete from ksrequesttable");
            _db.DoInsert("ksrequesttable", "ksrequesttableid,datestart,dateend,requestorid,requesttype", new object[] { "templateid", DateTime.Today, DateTime.Today.AddDays(1), _conId, "1" });
            _db.DoInsert("ksrequesttable", "ksrequesttableid,datestart,dateend,requestorid,requesttype,completeddate", new object[] { "templateid2", DateTime.Today, DateTime.Today.AddDays(1), _conId, "1", DateTime.Today });
            var templateProvider = new WoBundleTemplateProvider(_db);
            var results = templateProvider.GetAvailableTemplates();
            Assert.AreEqual(1, results.Count());
            var alert = results.First();
            Assert.AreEqual("templateid", alert.Id.Trim());
        }

        [Test]
        public void ShouldNotMatchNotificationsWithNoEndTime()
        {
            _db.ExecuteSQL("delete from ksrequesttable");
            _db.DoInsert("ksrequesttable", "ksrequesttableid,datestart,dateend,requestorid,requesttype", new object[] { "templateid", DateTime.Today, DateTime.Today.AddDays(1), _conId, "1" });
            _db.DoInsert("ksrequesttable", "ksrequesttableid,datestart,requestorid,requesttype", new object[] { "templateid2", DateTime.Today, _conId, "1" });
            var templateProvider = new WoBundleTemplateProvider(_db);
            var results = templateProvider.GetAvailableTemplates();
            Assert.AreEqual(1, results.Count());
            var alert = results.First();
            Assert.AreEqual("templateid", alert.Id.Trim());
        }

        [Test]
        public void ShouldMarkRequestAsProcessed()
        {

            _db.DoInsert("ksrequesttable", "ksrequesttableid,datestart,dateend,requestorid,requesttype", new object[] { "templateidxx", DateTime.Today, DateTime.Today.AddDays(1), _conId, "1" });
            var templateProvider = new WoBundleTemplateProvider(_db);
            templateProvider.RecordProcessedTemplate(new WoBundleAlertTemplate { Id = "templateidxx" });
            Assert.IsNotNull(_db.GetField("CompletedDate", "KSRequestTable", "KSRequestTableId='templateidxx'"));
        }

        [SetUp]
        public void SetUp()
        {
            _db = new DBConnectionWrapper("SalesLogix");
            _conId = (string)_db.DoSQL("select top 1 contactid from sysdba.ticket where statuscode=? and scheduleddate is not null order by scheduleddate desc", Constants.STATUS_SCHEDULED);
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
