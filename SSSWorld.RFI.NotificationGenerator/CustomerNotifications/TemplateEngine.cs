using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.CustomerNotifications
{
    public class TemplateEngine : ITemplateEngine<CustomerNotifAlertMatch, CustomerNotifAlertTemplate>
    {
        private readonly PdfPreparationService _pdfPreparation;

        public TemplateEngine(PdfPreparationService pdfPreparation)
        {
            _pdfPreparation = pdfPreparation;
        }

        public IEnumerable<PopulatedTemplate> PopulateTemplate(CustomerNotifAlertTemplate alertTemplate, IList<CustomerNotifAlertMatch> alertMatch)
        {
            foreach (CustomerNotifAlertMatch match in alertMatch)
            {

                PopulatedTemplate result = new PopulatedTemplate();
                result.Recipient = match.Recipient;
                result.AlertSubject = alertTemplate.EmailSubject == null ? "" : ExpandTemplateVariables(alertTemplate.EmailSubject, match);
                result.AlertText = alertTemplate.EmailText == null ? "" : ExpandTemplateVariables(alertTemplate.EmailText, match);
                result.Attachments = match.Attachments.Select(a => _pdfPreparation.PreparePdf(a, match)).ToArray();
                result.PopulatedFrom = new AlertMatch[] { match };
                yield return result;
            }
        }

        private string ExpandTemplateVariables(string emailText, CustomerNotifAlertMatch alertData)
        {
            var woData = alertData.WorkOrderData;
            emailText = Regex.Replace(emailText, @"\{\{([^\}]*)\}\}", patternMatch =>
            {
                string result;
                if (woData.TryGetValue(patternMatch.Groups[1].Value, out result))
                    return result;
                return "";
            });
            return emailText;
        }
    }
}
