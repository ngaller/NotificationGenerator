using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Shared;
using SSSWorld.RFI.NotificationGenerator.WoBundle;

namespace SSSWorld.RFI.NotificationGenerator.Tests.WoBundle
{
    [TestFixture]
    public class TestWoDataService
    {
        private DBConnectionWrapper _db;
        private WoBundleDataService _dataService;

        [Test]
        public void Pre1978WoWithLeadShouldIncludeLeadChecklistUnlessStoreHasOne()
        {
            // TODO need to add test for store, because the doc is only added if the store does not have one
            WoBundleAlertMatch alert = new WoBundleAlertMatch()
            {
                LeadPaintFound = true,
                JobType = "Measure",
                TicketId = "076afa7cc91",
                JobTypeId = "123",
                YearHomeBuilt = 1950
            };
            _dataService.LoadData(alert, new WoBundleAlertTemplate());
            Assert.IsTrue(alert.Attachments.Any(v => v is StaticReportDocument && ((StaticReportDocument)v).ReportFilename == "Reports\\Lead Test.rpt"));
        }

        [Test]
        public void Pre1978MeasureShouldIncludeRenovateRights()
        {
            // TODO need to add test for store, because the doc is only added if the store does not have one
            WoBundleAlertMatch alert = new WoBundleAlertMatch()
            {
                LeadPaintFound = true,
                JobType = "Measure",
                TicketId = "076afa7cc91",
                JobTypeId = "123",
                YearHomeBuilt = 1950
            };
            _dataService.LoadData(alert, new WoBundleAlertTemplate());
            Assert.IsTrue(alert.Attachments.Any(v => v is SlxReportDocument && ((SlxReportDocument)v).ReportName == "Form Docs:Lead Safe Renovation Checklist"));
        }

        /// <summary>
        /// Ensure that the ticket PDF attachments are included
        /// </summary>
        [Test]
        public void ShouldIncludeAttachments()
        {
            var ticketId = (string)_db.GetField("TICKETID", "ATTACHMENT", "FILENAME LIKE '%.pdf'  and ticketid is not null  order by attachdate desc");
            var attachId = (string)_db.GetField("ATTACHID", "ATTACHMENT", "FILENAME LIKE '%.pdf' and TICKETID=? order by attachdate desc", ticketId);
            WoBundleAlertMatch alert = new WoBundleAlertMatch()
            {
                LeadPaintFound = false,
                JobType = "Measure",
                TicketId = ticketId,
                JobTypeId = "123",
                YearHomeBuilt = 1950
            };
            _dataService.LoadData(alert, new WoBundleAlertTemplate());
            Assert.IsTrue(alert.Attachments.Any(v => v is FileAttachment && ((FileAttachment)v).Id == attachId));
        }

        /// <summary>
        /// Ensure that the store specific additional documents are included
        /// </summary>
        public void ShouldIncludeAdditionalDocuments()
        {

        }

        [SetUp]
        public void SetUp()
        {
            _db = new DBConnectionWrapper("SalesLogix");
            //            _conId = (string)_db.DoSQL("select top 1 contactid from sysdba.ticket where statuscode=? and scheduleddate is not null order by scheduleddate desc", Constants.STATUS_SCHEDULED);
            //            _tick1Id = (string)_db.DoSQL("select ticketid from ticket where statuscode=? and contactid = ? order by scheduleddate desc", Constants.STATUS_SCHEDULED, _conId);
            //            _tick2Id = (string)_db.DoSQL("select ticketid from ticket where statuscode=? and contactid = ? and ticketid <> ? order by scheduleddate desc", Constants.STATUS_SCHEDULED, _conId, _tick1Id);
            //            _tick3Id = (string)_db.DoSQL("select ticketid from ticket where statuscode=? and contactid = ? and ticketid not in (?, ?) order by scheduleddate desc", Constants.STATUS_SCHEDULED, _conId, _tick1Id, _tick2Id);
            _db.BeginTransaction();
            _dataService = new WoBundleDataService(_db);
        }

        [TearDown]
        public void TearDown()
        {
            _db.RollbackTransaction();
            _db.Dispose();
        }
    }
}
