using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SSSWorld.Common;
using SSSWorld.RFI.NotificationGenerator.Interfaces;
using SSSWorld.RFI.NotificationGenerator.Shared;

namespace SSSWorld.RFI.NotificationGenerator.CustomerNotifications
{
    /// <summary>
    /// Used to load the template data for customer notifications.
    /// </summary>
    public class DataService : IDataService<CustomerNotifAlertMatch, CustomerNotifAlertTemplate>
    {
        private readonly IDBConnectionWrapper _db;
        private readonly Configuration _configuration;

        public DataService(IDBConnectionWrapper db, Configuration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public void LoadData(CustomerNotifAlertMatch alert, CustomerNotifAlertTemplate alertTemplate)
        {
            if (!string.IsNullOrEmpty(alertTemplate.DocumentType))
            {
                FetchDocuments(alert, alertTemplate.DocumentType);
            }
            FetchWorkOrderData(alert);
        }

        /// <summary>
        /// Retrieve the data to be used in the email template
        /// </summary>
        /// <param name="alert"></param>
        private void FetchWorkOrderData(CustomerNotifAlertMatch alert)
        {
            string sql = @"select cust.firstname, cust.lastname, inst.account as installer, t.scheduleddate, po.ponum as ponumber, 
                caddr.address1, caddr.address2, caddr.city, caddr.state, caddr.postalcode, cust.contactid customerid, 
                cust.workphone customer_phone, jt.description as job_description, store.store_num as storenumber,
                crew.firstname + ' ' + crew.lastname as crewmember_name, crew.workphone crewmember_phone, crew.contactid as crewmemberid,
                case when crew.contact_picture is null then 0 else 1 end as has_crew_picture,
                fm.firstname + ' ' + fm.lastname as fieldmanager, fm.phone as fieldmanager_phone, 
                fm.email as fieldmanager_email,
                case when fmp.userpicture is null then 0 else 1 end as has_fm_picture, fm.userid as fieldmanagerid
            from ticket t 
            join c_jobtypes jt on jt.c_jobtypesid = t.jobtypeid
            join contact cust on cust.contactid = t.customerid
            join userinfo fm on t.fieldmanagerid = fm.userid
            join userprofile fmp on fmp.userid = fm.userid
            join address caddr on cust.addressid = caddr.addressid
            join contact crew on crew.contactid = t.contactid
            join account inst on inst.accountid = t.accountid
            join account store on store.accountid = t.store_accountid
            join purchaseorder po on po.purchaseorderid = t.purchaseorderid
            where t.ticketid=?";
            using (var dataTable = _db.OpenDataSet(sql, alert.TicketId).Tables[0])
            {
                if (dataTable.Rows.Count > 0)
                {
                    DataRow dr = dataTable.Rows[0];
                    foreach (DataColumn dc in dataTable.Columns)
                    {
                        alert.WorkOrderData[dc.ColumnName] = dr[dc].ToString();
                    }
                    // a few special ones
                    if (dr["scheduleddate"] != DBNull.Value)
                        alert.WorkOrderData["scheduleddate"] = ((DateTime)dr["scheduleddate"]).ToString("f");
                    alert.WorkOrderData["customer_address"] = FormatAddress(dr);
                    alert.WorkOrderData["customer_phone"] = FormatPhoneNumber(alert.WorkOrderData["customer_phone"]);
                    alert.WorkOrderData["crewmember_phone"] = FormatPhoneNumber(alert.WorkOrderData["crewmember_phone"]);
                    alert.WorkOrderData["fieldmanager_phone"] = FormatPhoneNumber(alert.WorkOrderData["fieldmanager_phone"]);
                    alert.WorkOrderData["fieldmanager_email"] = FormatEmailLink(alert.WorkOrderData["fieldmanager_email"]);
                    if(alert.WorkOrderData["has_fm_picture"] == "1")
                        alert.WorkOrderData["fieldmanager_photo"] = GetUserImage(alert.WorkOrderData["fieldmanagerid"]);
                    if(alert.WorkOrderData["has_crew_picture"] == "1")
                        alert.WorkOrderData["crewmember_photo"] = GetContactImage(alert.WorkOrderData["crewmemberid"]);
                    alert.WorkOrderData["unsubscribe_link"] = GetUnsubscribeLink(alert.WorkOrderData["customerid"]);
                    alert.WorkOrderData["portal_url"] = _configuration.CustomerPortalUrl;
                    alert.WorkOrderData["rfi_logo"] = GetRfiLogoTag();
                    alert.WorkOrderData["hd_logo"] = GetHdLogoTag();
                }
                else
                {
                    throw new Exception($"Unable to retrieve work order data for ticket id {alert.TicketId}");
                }
            }
        }

        private string FormatEmailLink(string address)
        {
            if (string.IsNullOrEmpty(address))
                return "";
            return $"<a href='mailto:{address}'>{address}</a>";
        }

        private string GetHdLogoTag()
        {
            var url = $"{_configuration.CustomerPortalUrl}/RFI/images/home-depot-authorized-service-provider.png";
            return $"<img src='{url}' width='148' height='82' alt='Home Depot Authorized Service Provider' />";
        }

        private string GetRfiLogoTag()
        {
            var url = $"{_configuration.CustomerPortalUrl}/RFI/images/rfi-small.png";
            return $"<img src='{url}' width='151' height='51' alt='RF Installations' />";
        }

        private string GetUserImage(string userId)
        {
            var url = $"{_configuration.CustomerPortalUrl}/RFI/Services/GetUserPhoto.ashx?id={userId}";
            // maybe we should read size and stuff?
            return $"<img src='{url}' alt='Contact Photo' />";
        }

        private string GetUnsubscribeLink(string contactId)
        {
            var url = $"{_configuration.CustomerPortalUrl}/RFI/Services/unsubscribe.ashx?id={contactId}";
            return $"<a href='{url}'>Unsubscribe to these emails by clicking here</a>";
        }

        /// <summary>
        /// Return img tag for contact image.
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        private string GetContactImage(string contactId)
        {
            var url = $"{_configuration.CustomerPortalUrl}/RFI/Services/GetContactPhoto.ashx?id={contactId}";
            return $"<img src='{url}' alt='Contact Photo' />";
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            if (phoneNumber == null || phoneNumber.Length < 10)
                return phoneNumber;
            return Regex.Replace(phoneNumber, @"^(\d{3})(\d{3})(\d{4})", "($1) $2-$3");
        }

        private string FormatAddress(DataRow dr)
        {
            var result = dr["address1"].ToString() + "<br>";
            if (dr["address2"].ToString() != "")
            {
                result += dr["address2"].ToString() + "<br>";
            }
            result += $"{dr["city"]}, {dr["state"]} {dr["postalcode"]}";
            return result;
        }

        private void FetchDocuments(CustomerNotifAlertMatch alert, string documentType)
        {
            using (var reader = _db.OpenDataReader("select top 1 attachid, filename from attachment where ticketid=? and documenttype=? order by attachdate desc", alert.TicketId, documentType))
            {
                while (reader.Read())
                {
                    alert.Attachments.Add(new FileAttachment
                    {
                        Id = reader[0].ToString(),
                        OutputName = documentType + Path.GetExtension(reader[1].ToString()),
                        Path = Path.Combine(_configuration.AttachmentPath, reader[1].ToString())
                    });
                }
            }
        }
    }
}
