using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.WoBundle
{
    /// <summary>
    /// Prepare the bundle for distribution by copying it to the path from whech it is served on the web server, and
    /// prpare the email including that link for the installers.
    /// </summary>
    public class WoBundleCopyToWeb : ITemplateEngine<WoBundleAlertMatch, WoBundleAlertTemplate>
    {
        private readonly ITemplateEngine<WoBundleAlertMatch, WoBundleAlertTemplate> _decoratedTemplateEngine;
        private readonly Configuration _configuration;
        static String _woBundleMailTemplate = null;

        public WoBundleCopyToWeb(ITemplateEngine<WoBundleAlertMatch, WoBundleAlertTemplate> decoratedTemplateEngine, Configuration configuration)
        {
            _decoratedTemplateEngine = decoratedTemplateEngine;
            _configuration = configuration;
            _woBundleMailTemplate = File.ReadAllText("WOBundleMailTemplate.txt");
        }

        public IEnumerable<PopulatedTemplate> PopulateTemplate(WoBundleAlertTemplate alertTemplate, IList<WoBundleAlertMatch> alertMatch)
        {
            foreach (var populated in _decoratedTemplateEngine.PopulateTemplate(alertTemplate, alertMatch))
            {
                if (populated.Attachments.Length != 1)
                {
                    throw new ApplicationException("Should only be given one attachment");
                }
                string fileId = CopyToBundleFolder(populated.Attachments[0]);

                populated.AlertText = FillEmailTemplate(fileId, populated.Recipient);
                populated.Attachments = new FileAttachment[] {};
                yield return populated;
            }
        }

        private string FillEmailTemplate(string fileId, Recipient recipient)
        {
            String portalUrl = _configuration.CustomerPortalUrl;
            if (portalUrl == null)
                throw new InvalidOperationException("PortalUrl is not defined");
            String url = $"{portalUrl}/RFI/Services/GetWoBundle.ashx?id={fileId}";

            return String.Format(_woBundleMailTemplate, recipient.RecipientName, url);
        }

        /// <summary>
        /// Copy to bundle folder under attachment path and return the file unique id
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        private string CopyToBundleFolder(FileAttachment attachment)
        {
            String outputFileId = Guid.NewGuid().ToString();
            string outputFileName = Path.Combine(_configuration.BundleOutputFolder, "RFIWoBundle" + outputFileId + ".pdf");
            File.Copy(attachment.Path, outputFileName);
            return outputFileId;
        }
    }
}
