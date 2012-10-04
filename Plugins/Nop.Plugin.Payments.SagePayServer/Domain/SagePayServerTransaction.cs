using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core;

namespace Nop.Plugin.Payments.SagePayServer.Domain
{
    public partial class SagePayServerTransaction : BaseEntity
    {
        public virtual string VPSTxId { get; set; }
        public virtual string SecurityKey { get; set; }
        public virtual string NotificationResponse { get; set; }

        public virtual string VendorTxCode { get; set; }
        public virtual string VPSSignature { get; set; }
        public virtual string Status { get; set; }
        public virtual string StatusDetail { get; set; }
        public virtual string TxAuthNo { get; set; }
        public virtual string AVSCV2 { get; set; }
        public virtual string AddressResult { get; set; }
        public virtual string PostCodeResult { get; set; }
        public virtual string CV2Result { get; set; }
        public virtual string GiftAid { get; set; }
        public virtual string ThreeDSecureStatus { get; set; }
        public virtual string CAVV { get; set; }
        public virtual string AddressStatus { get; set; }
        public virtual string PayerStatus { get; set; }
        public virtual string CardType { get; set; }
        public virtual string Last4Digits { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public virtual DateTime CreatedOnUtc { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SagePayServer Notification Page Results:");
            sb.AppendLine("Date Created: " + CreatedOnUtc);
            sb.AppendLine("VPSTxId: " + VPSTxId);
            sb.AppendLine("VendorTxCode: " + VendorTxCode);
            sb.AppendLine("VPSSignature: " + VPSSignature);
            sb.AppendLine("Status: " + Status);
            sb.AppendLine("StatusDetail: " + StatusDetail);

            sb.AppendLine("TxAuthNo: " + TxAuthNo);
            sb.AppendLine("AVSCV2: " + AVSCV2);
            sb.AppendLine("AddressResult: " + AddressResult);
            sb.AppendLine("PostCodeResult: " + PostCodeResult);
            sb.AppendLine("CV2Result: " + CV2Result);
            sb.AppendLine("GiftAid: " + GiftAid);
            sb.AppendLine("3DSecureStatus: " + ThreeDSecureStatus);
            sb.AppendLine("CAVV: " + CAVV);
            sb.AppendLine("AddressStatus: " + AddressStatus);
            sb.AppendLine("PayerStatus: " + PayerStatus);
            sb.AppendLine("CardType: " + CardType);
            sb.AppendLine("Last4Digits: " + Last4Digits);

            return sb.ToString();
        }
    }
}
