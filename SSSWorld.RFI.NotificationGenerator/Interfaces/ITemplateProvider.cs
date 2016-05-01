using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSSWorld.RFI.NotificationGenerator.Interfaces
{
    /// <summary>
    /// What are available templates in the system?
    /// </summary>
    public interface ITemplateProvider<TTemplate> where TTemplate : AlertTemplate
    {
        /// <summary>
        /// What are available templates in the system?
        /// </summary>
        /// <returns></returns>
        IEnumerable<TTemplate> GetAvailableTemplates();

        /// <summary>
        /// Mark a template as processed (if appropriate for this system - e.g. if they come from KSRequestTable)
        /// </summary>
        /// <param name="alert"></param>
        void RecordProcessedTemplate(TTemplate alert);
    }
}
