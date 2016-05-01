using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSSWorld.RFI.NotificationGenerator.Interfaces
{
    public interface ITemplateEngine<TAlertMatch, TAlertTemplate>
        where TAlertTemplate : AlertTemplate
        where TAlertMatch : AlertMatch
    {
        /// <summary>
        /// Use data in alert match to populate the alert template fields.
        /// </summary>
        /// <param name="alertTemplate"></param>
        /// <param name="alertMatch"></param>
        /// <returns></returns>
        IEnumerable<PopulatedTemplate> PopulateTemplate(TAlertTemplate alertTemplate, IList<TAlertMatch> alertMatch);
    }
}
