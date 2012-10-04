using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using System;

namespace Nop.Plugin.Payments.SagePayServer.Models
{
    public class PaymentSagePayServerModel : BaseNopModel
    {
        public PaymentSagePayServerModel()
        {
            Warnings = new List<string>();
        }

        public string FrameURL { get; set; }
        public Guid? TransactionId { get; set; }

        public List<string> Warnings { get; set; }
    }
}