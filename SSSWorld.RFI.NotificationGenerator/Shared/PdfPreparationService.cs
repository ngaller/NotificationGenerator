using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CrystalDecisions.ReportAppServer.Controllers;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.Shared
{
    public class PdfPreparationService
    {
        private readonly CreatePdfFromCrystal _crystal;
        private readonly Configuration _configuration;

        public PdfPreparationService(CreatePdfFromCrystal crystal, Configuration configuration)
        {
            _crystal = crystal;
            _configuration = configuration;
        }

        public FileAttachment PreparePdf(DocumentToAttach document, AlertMatch relatedAlert)
        {
            if (document.GetType() == typeof(FileAttachment))
            {
                return (FileAttachment)document;
            }
            if (document.GetType() == typeof(SlxReportDocument))
            {
                return PrintReport((SlxReportDocument)document, relatedAlert);
            }
            if (document.GetType() == typeof(StaticReportDocument))
            {
                return PrintReport((StaticReportDocument)document, relatedAlert);
            }
            throw new ApplicationException("Invalid document type");
        }

        private FileAttachment PrintReport(StaticReportDocument document, AlertMatch relatedAlert)
        {
            var parameters = document.ReportParameters;
            if (parameters == null)
            {
                if (relatedAlert == null)
                    throw new ArgumentNullException(nameof(relatedAlert));
                parameters = new Dictionary<string, object> {["TicketId"] = relatedAlert.TicketId };
            }
            return new FileAttachment
            {
                Path = _crystal.PrintStaticReport(document.ReportFilename, null, parameters),
                OutputName = GetOutputName(document.OutputName)
            };
        }

        private FileAttachment PrintReport(SlxReportDocument document, AlertMatch relatedAlert)
        {
            if (relatedAlert == null)
                throw new ArgumentNullException(nameof(relatedAlert));
            var rsf = "{TICKET.TICKETID} = \"" + relatedAlert.TicketId + "\"";
            return new FileAttachment
            {
                Path = _crystal.PrintSlxReport(document.ReportName, rsf),
                OutputName = GetOutputName(document.OutputName)
            };
        }

        private string GetOutputName(string outputName)
        {
            if(string.IsNullOrEmpty(outputName) || outputName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return "document.pdf";
            }
            return Path.GetFileNameWithoutExtension(outputName) + ".pdf";
        }
    }
}
