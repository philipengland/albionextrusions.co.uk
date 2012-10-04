using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Web.Models.Common;

namespace Nop.Web.Themes.AlbionExtrusions.Models
{
    public class CheckOutModel
    {
        public AddressModel BillingAddress { get; set; }
        public AddressModel DeliveryAddress { get; set; }
    }
}