using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.SagePayServer
{
    public class SagePayServerPaymentSettings : ISettings
    {

        public string TransactType { get; set; }

        public string ConnectTo { get; set; }

        public string VendorName { get; set; }

        public string PartnerID { get; set; }

        /// <summary>
        /// IMPORTANT. This should start http:// or https:// and should be the name by which our servers can call back to yours
        /// i.e. it MUST be resolvable externally, and have access granted to the Sage Pay servers. Examples would be https://www.mysite.com or http://212.111.32.22/.
        /// NOTE: You should leave the final / in place. 
        /// </summary>
        public string NotificationFullyQualifiedDomainName { get; set; }

        public string ReturnFullyQualifiedDomainName { get; set; }

        public decimal AdditionalFee { get; set; }

        public string Profile { get; set; }

        public string VendorDescription { get; set; }

        public class ConnectToValues
        {
            public const string SIMULATOR = "SIMULATOR";
            public const string TEST = "TEST";
            public const string LIVE = "LIVE";
        }
        public class TransactTypeValues
        {
            public const string PAYMENT = "PAYMENT";
            public const string DEFERRED = "DEFERRED";
            public const string AUTHENTICATE = "AUTHENTICATE";
        }
        public class ProfileValues
        {
            public const string LOW = "LOW";
            public const string NORMAL = "NORMAL";
        }

    }
}
