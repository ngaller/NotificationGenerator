using System;
using SSSWorld.Common;
using log4net;
using Ninject;
using SSSWorld.RFI.NotificationGenerator.CustomerNotifications;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;
using SSSWorld.RFI.NotificationGenerator.WoBundle;

namespace SSSWorld.RFI.NotificationGenerator
{
    /// <summary>
    /// This program is used to generate the "WO Bundle" and "CA Measures" document, and send them to the installers.
    /// </summary>
    public class Program
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(Program));

        public static void Main(string[] args)
        {
            try
            {
                using (var sg = new SingleGlobalInstance(1000))
                {
                    log4net.Config.XmlConfigurator.Configure();
                    LOG.Debug("Starting up");
                    try
                    {
                        StandardKernel kernel = new StandardKernel(new WoBundleModule(), new CustomerNotifModule());
                        ConfigureKernel(kernel);

                        SendWoBundles(kernel);
                        SendCustomerNotifications(kernel);
                    }
                    catch (Exception e)
                    {
                        LOG.Error("Unhandled exception in Main method", e);
                    }
                }
            }
            catch (TimeoutException)
            {
                Console.Error.WriteLine("Already running - aborting");
            }
        }

        private static void SendCustomerNotifications(IKernel kernel)
        {
            var alertService = kernel.Get<AlertService<CustomerNotifAlertTemplate, CustomerNotifAlertMatch>>();
            alertService.PrepareAndSendAlerts();
        }

        private static void SendWoBundles(IKernel kernel)
        {
            var alertService = kernel.Get<AlertService<WoBundleAlertTemplate, WoBundleAlertMatch>>();
            alertService.PrepareAndSendAlerts();
        }

        /// <summary>
        /// Marked internal for testing
        /// </summary>
        /// <param name="kernel"></param>
        internal static void ConfigureKernel(IKernel kernel)
        {
            kernel.Bind<IDBConnectionWrapper>().ToConstant(new DBConnectionWrapper("SalesLogix"));
            kernel.Bind<IEmailService>().To<EmailService>();
            kernel.Bind<ISmtpClientWrapper>().To<SmtpClientWrapper>();
        }
    }
}
