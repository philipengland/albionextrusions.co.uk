using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Common;
using Nop.Web.Themes.AlbionExtrusions.Models;
using Nop.Core.Domain.Common;

namespace Nop.Web.Controllers
{
    public partial class CheckoutController : BaseNopController
    {
        public ActionResult CompleteCheckout(CheckOutModel model)
        {
            //STEP ONE: ADDRESSES
            //validation
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            if (cart.Count == 0)
                throw new Exception("Your cart is empty");

            if (model.BillingAddress == null)
            {
                model.BillingAddress = model.DeliveryAddress;

                Address address = new Address();
                if (address == null)
                {
                    //address is not found. let's create a new one
                    address = model.BillingAddress.ToEntity();
                    address.CreatedOnUtc = DateTime.UtcNow;
                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;
                    _workContext.CurrentCustomer.Addresses.Add(address);
                }
                _workContext.CurrentCustomer.BillingAddress = address;
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }

            //STEP TWO: SHIPPING METHOD
            //var shippingModel = new CheckoutShippingMethodModel.ShippingMethodModel();

            //var getShippingOptionResponse = _shippingService.GetShippingOptions(cart, _workContext.CurrentCustomer.ShippingAddress);
            //if (getShippingOptionResponse.Success)
            //{
            //    //performance optimization. cache returned shipping options.
            //    //we'll use them later (after a customer has selected an option).
            //    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.OfferedShippingOptions, getShippingOptionResponse.ShippingOptions);

            //    foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
            //    {
            //        var soModel = new CheckoutShippingMethodModel.ShippingMethodModel()
            //        {
            //            Name = shippingOption.Name,
            //            Description = shippingOption.Description,
            //            ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName,
            //        };

            //        //adjust rate
            //        Discount appliedDiscount = null;
            //        var shippingTotal = _orderTotalCalculationService.AdjustShippingRate(
            //            shippingOption.Rate, cart, out appliedDiscount);

            //        decimal rateBase = _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer);
            //        decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
            //        soModel.Fee = _priceFormatter.FormatShippingPrice(rate, true);

            //        model.ShippingMethods.Add(soModel);
            //    }

            //    //find a selected (previously) shipping method
            //    var lastShippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.LastShippingOption);
            //    if (lastShippingOption != null)
            //    {
            //        var shippingOptionToSelect = model.ShippingMethods.ToList()
            //            .Find(so => !String.IsNullOrEmpty(so.Name) && so.Name.Equals(lastShippingOption.Name, StringComparison.InvariantCultureIgnoreCase) &&
            //            !String.IsNullOrEmpty(so.ShippingRateComputationMethodSystemName) && so.ShippingRateComputationMethodSystemName.Equals(lastShippingOption.ShippingRateComputationMethodSystemName, StringComparison.InvariantCultureIgnoreCase));
            //        if (shippingOptionToSelect != null)
            //            shippingOptionToSelect.Selected = true;
            //    }
            //    //if no option has been selected, let's do it for the first one
            //    if (model.ShippingMethods.Where(so => so.Selected).FirstOrDefault() == null)
            //    {
            //        var shippingOptionToSelect = model.ShippingMethods.FirstOrDefault();
            //        if (shippingOptionToSelect != null)
            //            shippingOptionToSelect.Selected = true;
            //    }
            //}
            //else
            //    foreach (var error in getShippingOptionResponse.Errors)
            //        model.Warnings.Add(error);

            return View(model);
        }

    }
}
