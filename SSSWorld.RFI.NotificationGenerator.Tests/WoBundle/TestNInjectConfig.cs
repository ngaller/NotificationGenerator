using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using NUnit.Framework;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.WoBundle;

namespace SSSWorld.RFI.NotificationGenerator.Tests.WoBundle
{
    [TestFixture]
    public class TestNInjectConfig
    {
        [Test]
        public void TestGetTemplateEngine()
        {
            var kernel = new StandardKernel(new WoBundleModule());
            Program.ConfigureKernel(kernel);
            var templateEngine = kernel.Get<ITemplateEngine<WoBundleAlertMatch, WoBundleAlertTemplate>>();
            Assert.IsNotNull(templateEngine);
            Assert.IsInstanceOf<WoBundleCopyToWeb>(templateEngine);
        }
    }
}
