using System.ComponentModel;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.SagePayServer.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            ConnectToList = new List<SelectListItem>();
            TransactTypeList = new List<SelectListItem>();
            ProfileList = new List<SelectListItem>();
        }

        [DisplayName("Connect To")]
        public string ConnectTo { get; set; }

        public IList<SelectListItem> ConnectToList { get; set; }

        [DisplayName("Vendor Name")]
        public string VendorName { get; set; }

        [DisplayName("Partner ID")]
        public string PartnerID { get; set; }

        [DisplayName("Notification Fully Qualified Domain Name")]
        public string NotificationFullyQualifiedDomainName { get; set; }

        [DisplayName("Return Fully Qualified Domain Name")]
        public string ReturnFullyQualifiedDomainName { get; set; }

        [DisplayName("Transaction Type")]
        public string TransactType { get; set; }

        public IList<SelectListItem> TransactTypeList { get; set; }

        [DisplayName("Additional fee")]
        public decimal AdditionalFee { get; set; }

        [DisplayName("Profile")]
        public string Profile { get; set; }

        public IList<SelectListItem> ProfileList { get; set; }
    }
}