using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;
using SSSWorld.RFI.NotificationGenerator.WoBundle;

namespace SSSWorld.RFI.NotificationGenerator.Tests.WoBundle
{
    [TestFixture]
    public class TestWoTemplateEngine
    {
        private WoBundleTemplateEngine _templateEngine;
        private IDBConnectionWrapper _db;
        private string _conId, _tick1Id, _tick2Id;

        [Test]
        public void TestPrintCoverPageAndOverAllCoverPage()
        {
            var populated = _templateEngine.PopulateTemplate(new WoBundleAlertTemplate
            {
                StartDate = DateTime.Today,
                CrewMemberId = _conId,
                EndDate = DateTime.Today,
                Recipient = new Recipient()
            }, new WoBundleAlertMatch[] { new WoBundleAlertMatch
            {
                 TicketId = _tick1Id
            } }).First();

            Assert.AreEqual(1, populated.Attachments.Length, "We should have one (and only one) attachment, as the 2 cover pages should be merged");
            Assert.IsTrue(File.Exists(populated.Attachments[0].Path), "Should generate a valid file and store the path");
            Assert.AreEqual(2, MergePDF.CountPagesInPdf(populated.Attachments[0].Path), "We are not printing anything but the cover pages, therefore we should have 2 pages");
        }

        [Test]
        public void TestPrintCoverPageMultipleTickets()
        {
            var populated = _templateEngine.PopulateTemplate(new WoBundleAlertTemplate
            {
                StartDate = DateTime.Today,
                CrewMemberId = _conId,
                EndDate = DateTime.Today,
                Recipient = new Recipient()
            }, new WoBundleAlertMatch[] { new WoBundleAlertMatch
            {
                 TicketId = _tick1Id
            }, new WoBundleAlertMatch { TicketId = _tick2Id} }).First();

            Assert.AreEqual(1, populated.Attachments.Length, "We should have one (and only one) attachment, as the 2 cover pages should be merged");
            Assert.IsTrue(File.Exists(populated.Attachments[0].Path), "Should generate a valid file and store the path");
            Assert.AreEqual(3, MergePDF.CountPagesInPdf(populated.Attachments[0].Path), "We are not printing anything but the cover pages, therefore we should have 3 pages (2 tickets + cover)");
            // Note: may want to manually review output to ensure the page counts are correct (should all be 1)
        }

        [Test]
        public void TestIncludeAttachments()
        {
            var match = new WoBundleAlertMatch
            {
                 TicketId = _tick1Id,
            };
            match.Attachments.Add(new FileAttachment { Path = "Samples\\CA.pdf", OutputName = "Sample" });
            var populated = _templateEngine.PopulateTemplate(new WoBundleAlertTemplate
            {
                StartDate = DateTime.Today,
                CrewMemberId = _conId,
                EndDate = DateTime.Today,
                Recipient = new Recipient()
            }, new[] { match } ).First();
            Assert.AreEqual(3, MergePDF.CountPagesInPdf(populated.Attachments[0].Path), "Should have the 2 cover pages and 1 CA page");
        }

        [SetUp]
        public void SetUp()
        {
            _db = new DBConnectionWrapper("SalesLogix");
            _conId = (string)_db.DoSQL("select top 1 contactid from sysdba.ticket where statuscode=? and scheduleddate is not null order by scheduleddate desc", Constants.STATUS_SCHEDULED);
            _tick1Id = (string)_db.DoSQL("select ticketid from ticket where statuscode=? and contactid = ? order by scheduleddate desc", Constants.STATUS_SCHEDULED, _conId);
            _tick2Id = (string)_db.DoSQL("select ticketid from ticket where statuscode=? and contactid = ? and ticketid <> ? order by scheduleddate desc", Constants.STATUS_SCHEDULED, _conId, _tick1Id);
            //            _tick3Id = (string)_db.DoSQL("select ticketid from ticket where statuscode=? and contactid = ? and ticketid not in (?, ?) order by scheduleddate desc", Constants.STATUS_SCHEDULED, _conId, _tick1Id, _tick2Id);
            _db.BeginTransaction();
            // ideally we should use a mock and test them separately... but that would require a bit more work
            // and I am feeling lazy
            var config = new Configuration(_db);
            var pdfPrep = new PdfPreparationService(new CreatePdfFromCrystal(_db, config), config);
            var mergePdf = new MergePDF(config);
            _templateEngine = new WoBundleTemplateEngine(pdfPrep, mergePdf, config);
        }

        [TearDown]
        public void TearDown()
        {
            _db.RollbackTransaction();
            _db.Dispose();
        }
    }
}
