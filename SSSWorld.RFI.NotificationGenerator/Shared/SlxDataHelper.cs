using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SSSWorld.Common;

namespace SSSWorld.RFI.NotificationGenerator.Shared
{
    public class SlxDataHelper
    {
        /// <summary>
        /// Add a ticket activity record using a direct DB method.
        /// Return ticket activity id.
        /// </summary>
        public static String AddTicketActivityDirect(IDBConnectionWrapper db, String ticketId, String description, string userField1=null)
        {
            String tickActId = db.GetIDFor("TICKETACTIVITY");
            db.DoInsert("TICKETACTIVITY",
            "TICKETACTIVITYID,TICKETID,ACTIVITYTYPECODE,USERID,SHORTDESC,UNITS,ELAPSEDUNITS,ASSIGNEDDATE,COMPLETEDDATE,ACTIVITYDESC,FOLLOWUP,PUBLICACCESSCODE,SPN_EXPORT_FLAG,CONTACTID,USERFIELD1",
            new object[] { tickActId, ticketId, "k6UJ9A0003LG", "ADMIN", description, 0, 0,
                db.Now, db.Now, description, "F", "k6UJ9A0000OW", "F", null, userField1 },
            true);
            return tickActId;
        }

    }
}
