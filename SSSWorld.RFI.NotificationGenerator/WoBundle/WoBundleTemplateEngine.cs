using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using log4net;
using log4net.Config;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.WoBundle
{
    public class WoBundleTemplateEngine : ITemplateEngine<WoBundleAlertMatch, WoBundleAlertTemplate>
    {
        private readonly PdfPreparationService _pdfGen;
        private readonly MergePDF _mergePdf;
        private readonly Configuration _configuration;
        private static readonly ILog LOG = LogManager.GetLogger(typeof(WoBundleTemplateEngine));

        public WoBundleTemplateEngine(PdfPreparationService pdfGen, MergePDF mergePdf, Configuration configuration)
        {
            _pdfGen = pdfGen;
            _mergePdf = mergePdf;
            _configuration = configuration;
        }

        /// <summary>
        /// Pick output path in the output folder
        /// Prepare data for the template, for each matched ticket:
        ///  * Concatenate all the attachments
        ///  * Generate the cover letter PDF
        /// Then, print the overall cover page
        /// </summary>
        /// <param name="alertTemplate"></param>
        /// <param name="alertMatch"></param>
        /// <returns></returns>
        public IEnumerable<PopulatedTemplate> PopulateTemplate(WoBundleAlertTemplate alertTemplate, IList<WoBundleAlertMatch> alertMatches)
        {
            int totalPages = 0, numWorkOrders = 0;
            string bundlePdf = Utils.GetUniqueFileName(_configuration.TemporaryFolder, $"Bundle_{alertTemplate.CrewMemberId}", ".pdf"); ;
            LOG.Debug($"Prepare bundle for {alertTemplate.CrewMemberId} ({alertTemplate.Recipient.RecipientAddress})");
            foreach (WoBundleAlertMatch match in alertMatches)
            {
                int numPages = 0;
                string woPdf = Utils.GetUniqueFileName(_configuration.TemporaryFolder, $"WO_{match.TicketNumber}", ".pdf");
                foreach (DocumentToAttach attachment in match.Attachments)
                {
//                    LOG.Debug($"Merge {attachment.OutputName} into {woPdf}");
                    var file = _pdfGen.PreparePdf(attachment, match);
                    numPages = _mergePdf.AppendToPdf(woPdf, file.Path, numPages);
                }
                // cover page for individual WO
                PrintCoverPage(woPdf, match, numPages);
                totalPages = _mergePdf.AppendToPdf(bundlePdf, woPdf, totalPages);
                numWorkOrders++;
            }
            // overall cover page
            // note we pass enddate - 1 because Crystal is expecting inclusive range
            totalPages = PrintOverallCoverPage(bundlePdf, numWorkOrders, alertTemplate);
            LOG.Debug($"Generated bundle for {alertTemplate.Recipient.RecipientAddress}: {totalPages} pages");
            yield return new PopulatedTemplate
            {
                AlertSubject = "Installer Bundles",
                Recipient = alertTemplate.Recipient,
                PopulatedFrom = alertMatches.Cast<AlertMatch>().ToList(),
                Attachments = new[] { new FileAttachment { Path = bundlePdf, OutputName = "WO Bundle" } }
            };
        }

        private int PrintCoverPage(string tempOutput, WoBundleAlertMatch match, int numPages)
        {
            var doc = new StaticReportDocument
            {
                ReportFilename = "Reports\\CoverPage.rpt",
                ReportParameters = new Dictionary<string, object>
                {
                    ["TicketId"] = match.TicketId,
                    ["NumPagesAttached"] = numPages
                }
            };
            var coverPdf = _pdfGen.PreparePdf(doc, null);
            return _mergePdf.PrependToPdf(tempOutput, coverPdf.Path, numPages);
        }

        private int PrintOverallCoverPage(string curTempOutput, int numWorkOrders, WoBundleAlertTemplate alertTemplate)
        {
            var doc = new StaticReportDocument
            {
                ReportFilename = "Reports\\OverallCoverPage.rpt",
                ReportParameters = new Dictionary<string, object>
                {
                    ["CONTACTID"] = alertTemplate.CrewMemberId,
                    ["STARTDATE"] = alertTemplate.StartDate.Date,
                    ["ENDDATE"] = alertTemplate.EndDate.Date,
                    ["NUMWORKORDERS"] = numWorkOrders,
                    ["SPLIT"] = false
                }
            };
            var coverPdf = _pdfGen.PreparePdf(doc, null);
            return _mergePdf.PrependToPdf(curTempOutput, coverPdf.Path, 0);
        }

    }
}
