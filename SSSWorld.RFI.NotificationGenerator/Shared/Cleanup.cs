using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;

namespace SSSWorld.RFI.NotificationGenerator.Shared
{
    public class Cleanup
    {
        private readonly Configuration _configuration;
        private static readonly ILog LOG = LogManager.GetLogger(typeof(Cleanup));

        public Cleanup(Configuration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Clean the folders used for saving the bundles that need to be retrieved.
        /// This should only be done once per run
        /// </summary>
        public void CleanOutputFolders()
        {
            LOG.Debug("Cleaning output folder: " + _configuration.BundleOutputFolder);
            CleanFolder(_configuration.BundleOutputFolder, 24 * 4);
        }

        /// <summary>
        /// Clean the temporary folders used to assemble the template.
        /// This can be done after each bundle has been sent
        /// </summary>
        public void CleanTempFolders()
        {
            CleanFolder(_configuration.TemporaryFolder, _configuration.TemporaryFolderCleanupInterval);
        }

        internal static void CleanFolder(String outputPath, int hoursValid = 4)
        {
            foreach (String f in Directory.GetFiles(outputPath))
            {
                FileInfo fi = new FileInfo(f);
                if (hoursValid == 0 || fi.CreationTime.AddHours(hoursValid) < DateTime.Now)
                {
                    fi.Delete();
                }
            }
        }

    }
}
