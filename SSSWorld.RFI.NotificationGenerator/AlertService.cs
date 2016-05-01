using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator
{
    /// <summary>
    /// Generic alert processor.
    /// </summary>
    /// <typeparam name="TTemplate"></typeparam>
    /// <typeparam name="TAlertMatch"></typeparam>
    public class AlertService<TTemplate, TAlertMatch>
        where TTemplate : AlertTemplate
        where TAlertMatch : AlertMatch
    {
        private readonly ITemplateProvider<TTemplate> _templateProvider;
        private readonly INotificationMatcher<TTemplate, TAlertMatch> _notificationMatcher;
        private readonly ITemplateEngine<TAlertMatch, TTemplate> _templateEngine;
        private readonly IDataService<TAlertMatch, TTemplate> _dataService;
        private readonly IEmailService _emailService;
        private readonly Cleanup _cleanup;
        private static readonly ILog LOG = LogManager.GetLogger(typeof(AlertService<TTemplate, TAlertMatch>));

        public AlertService(ITemplateProvider<TTemplate> templateProvider,
            INotificationMatcher<TTemplate, TAlertMatch> notificationMatcher,
            ITemplateEngine<TAlertMatch, TTemplate> templateEngine,
            IDataService<TAlertMatch, TTemplate> dataService,
            IEmailService emailService,
            Cleanup cleanup)
        {
            _templateProvider = templateProvider;
            _notificationMatcher = notificationMatcher;
            _templateEngine = templateEngine;
            _dataService = dataService;
            _emailService = emailService;
            _cleanup = cleanup;
        }

        public void PrepareAndSendAlerts()
        {
            _cleanup.CleanOutputFolders();
            foreach (var template in _templateProvider.GetAvailableTemplates())
            {
                var matches = _notificationMatcher.GetAlertMatches(template);
                if (matches.Any())
                {
                    foreach (var match in matches)
                    {
                        _dataService.LoadData(match, template);
                    }
                    foreach (var populated in _templateEngine.PopulateTemplate(template, matches))
                    {
                        if (SendTemplateEmail(populated))
                        {
                            foreach (var match in populated.PopulatedFrom)
                            {
                                _notificationMatcher.RecordSentAlert((TAlertMatch)match, template);
                            }
                        }
                        else
                        {
                            LOG.Warn($"Could not send email to {populated.Recipient.RecipientAddress}");
                        }
                    }
                }
                _templateProvider.RecordProcessedTemplate(template);
                _cleanup.CleanTempFolders();
            }
        }

        private bool SendTemplateEmail(PopulatedTemplate populated)
        {
            var cc = MyConfiguration.Instance.GetAppSetting("WOBundles_CC");
            return _emailService.SendEmail(populated.Recipient.RecipientAddress, cc, populated.AlertSubject, populated.AlertText, populated.Attachments);
        }
    }
}
