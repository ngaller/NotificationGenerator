using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.CustomerNotifications;

namespace SSSWorld.RFI.NotificationGenerator.Tests.CustomerNotification
{
    [TestFixture]
    public class TestTemplateProvider
    {
        [Test]
        public void TestGetTemplates()
        {
            using(var db = new DBConnectionWrapper("SalesLogix"))
            {
                var prov = new TemplateProvider(db);
                var result = prov.GetAvailableTemplates().ToList();
                Assert.Greater(result.Count, 0);
            }
        }
    }
}
