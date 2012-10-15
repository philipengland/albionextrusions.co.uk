using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Web.Models.Common;

namespace Nop.Web.Models.Checkout
{
    public partial class OnePageCheckoutModel
    {
        public AddressModel BillingAddress { get; set; }
        public AddressModel DeliveryAddress { get; set; }
    }
}