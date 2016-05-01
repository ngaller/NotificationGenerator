using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator;

namespace SSSWorld.RFI.NotificationGenerator.WoBundle
{
    /// <summary>
    /// Service configurator for NInject
    /// </summary>
    public class WoBundleModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDataService<WoBundleAlertMatch, WoBundleAlertTemplate>>().To<WoBundleDataService>();
            Bind<INotificationMatcher<WoBundleAlertTemplate, WoBundleAlertMatch>>().To<WoBundleNotificationMatcher>();
            // decorator for WO bundle templates
            Bind<ITemplateEngine<WoBundleAlertMatch, WoBundleAlertTemplate>>().To<WoBundleCopyToWeb>();
//                .WhenInjectedInto<AlertService<WoBundleAlertTemplate, WoBundleAlertMatch>>();
            Bind<ITemplateEngine<WoBundleAlertMatch, WoBundleAlertTemplate>>().To<WoBundleTemplateEngine>()
                .WhenInjectedInto<WoBundleCopyToWeb>();
            Bind<ITemplateProvider<WoBundleAlertTemplate>>().To<WoBundleTemplateProvider>();
        }
    }
}
