using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSSWorld.RFI.NotificationGenerator.Interfaces
{
    /// <summary>
    /// Used to fetch additional data used to prepare an alert
    /// </summary>
    public interface IDataService<TAlertMatch, TAlertTemplate> 
        where TAlertMatch : AlertMatch
        where TAlertTemplate : AlertTemplate
    {
        void LoadData(TAlertMatch alert, TAlertTemplate alertTemplate);
    }
}
