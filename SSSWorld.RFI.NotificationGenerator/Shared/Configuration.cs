using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SSSWorld.Common;

namespace SSSWorld.RFI.NotificationGenerator.Shared
{
    public class Configuration
    {
        private readonly IDBConnectionWrapper _db;

        public Configuration(IDBConnectionWrapper db)
        {
            _db = db;
            if (!Directory.Exists(BundleOutputFolder))
                Directory.CreateDirectory(BundleOutputFolder);
            if (!Directory.Exists(TemporaryFolder))
                Directory.CreateDirectory(TemporaryFolder);
        }

        private string _attachmentPath = null;
        public string AttachmentPath
        {
            get
            {
                _attachmentPath = _attachmentPath ?? (string)_db.GetField("ATTACHMENTPATH", "BRANCHOPTIONS", "1=1");
                return _attachmentPath;
            }
        }

        public string BundleOutputFolder => AttachmentPath + "\\WOBundles";

        public string TemporaryFolder => MyConfiguration.Instance.GetAppSetting("TmpFolder");
        public string CustomerPortalUrl => MyConfiguration.Instance.GetAppSetting("PortalUrl");
        public int TemporaryFolderCleanupInterval => MyConfiguration.Instance.GetAppSettingInt32("TmpFolderCleanupInterval") ?? 4;
    }
}
