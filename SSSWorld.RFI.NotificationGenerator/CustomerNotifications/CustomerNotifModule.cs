using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using SSSWorld.RFI.NotificationGenerator.Interfaces;

namespace SSSWorld.RFI.NotificationGenerator.CustomerNotifications
{
    public class CustomerNotifModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDataService<CustomerNotifAlertMatch, CustomerNotifAlertTemplate>>().To<DataService>();
            Bind<INotificationMatcher<CustomerNotifAlertTemplate, CustomerNotifAlertMatch>>().To<NotificationMatcher>();
            // decorator for WO bundle templates
            Bind<ITemplateEngine<CustomerNotifAlertMatch, CustomerNotifAlertTemplate>>().To<TemplateEngine>();
            Bind<ITemplateProvider<CustomerNotifAlertTemplate>>().To<TemplateProvider>();
        }
    }
}
