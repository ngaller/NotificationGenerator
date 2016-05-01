using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.Tests
{
    [TestFixture]
    class TestMergePdf
    {
        [Test]
        public void TestMerge2Pdf_Valid()
        {
            int inpages = MergePDF.CountPagesInPdf("Samples\\CA.pdf") + MergePDF.CountPagesInPdf("Samples\\CA2.pdf");
            int pageCount = 0;
            bool mergeResult = MergePDF.MergeTwoPDFs("output.pdf", "Samples\\CA.pdf", "Samples\\CA2.pdf", ref pageCount);
            Assert.IsTrue(mergeResult);
            int outpages = MergePDF.CountPagesInPdf("output.pdf");
            Assert.AreEqual(inpages, outpages);
            Assert.AreEqual(outpages, pageCount);
        }

        [Test]
        public void TestAppendToPdf_NotValidSource_ShouldSkip()
        {
            using (var db = new DBConnectionWrapper("SalesLogix"))
            {
                var config = new Configuration(db);
                var mergePdf = new MergePDF(config);
                // just make sure we don't get an error
                var result = mergePdf.AppendToPdf("samples\\CA.pdf", "notthere.pdf", 1);
                Assert.AreEqual(1, result);
            }
        }

        [Test]
        public void TestAppendToPdf_NotValidTarget_ShouldCopyToTarget()
        {
            using (var db = new DBConnectionWrapper("SalesLogix"))
            {
                var config = new Configuration(db);
                var mergePdf = new MergePDF(config);

                Cleanup.CleanFolder(config.TemporaryFolder, 0);
                var result = mergePdf.AppendToPdf("Tmp\\merged.pdf", "samples\\CA.pdf", 0);
                Assert.AreEqual(1, result, "We should count the pages in the source pdf");
                Assert.IsTrue(File.Exists("Tmp\\merged.pdf"));
            }
        }

        [Test]
        public void TestAppendToPdf_Valid()
        {
            using (var db = new DBConnectionWrapper("SalesLogix"))
            {
                var config = new Configuration(db);
                var mergePdf = new MergePDF(config);

                Cleanup.CleanFolder(config.TemporaryFolder, 0);
                File.Copy("samples\\CA.pdf", "Tmp\\merged2.pdf");
                var result = mergePdf.AppendToPdf("Tmp\\merged2.pdf", "samples\\CA.pdf", 22);
                Assert.AreEqual(2, result);
                Assert.IsTrue(File.Exists("Tmp\\merged2.pdf"));
            }
        }
    }
}
