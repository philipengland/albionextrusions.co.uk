using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.UI.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Web.Controllers
{
    public partial class ShoppingCartController : BaseNopController
    {
        //
        // GET: /ShoppingCart/

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("startalbioncheckout")]
        public ActionResult StartAlbionCheckout(FormCollection form)
        {
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();

            //apply checkout attributes
            string selectedAttributes = "";
            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(!cart.RequiresShipping());
            foreach (var attribute in checkoutAttributes)
            {
                string controlId = string.Format("checkout_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                        {
                            var ddlAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(ddlAttributes))
                            {
                                int selectedAttributeId = int.Parse(ddlAttributes);
                                if (selectedAttributeId > 0)
                                    selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.RadioList:
                        {
                            var rblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(rblAttributes))
                            {
                                int selectedAttributeId = int.Parse(rblAttributes);
                                if (selectedAttributeId > 0)
                                    selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                        {
                            var txtAttribute = form[controlId];
                            if (!String.IsNullOrEmpty(txtAttribute))
                            {
                                string enteredText = txtAttribute.Trim();
                                selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.MultilineTextbox:
                        {
                            var txtAttribute = form[controlId];
                            if (!String.IsNullOrEmpty(txtAttribute))
                            {
                                string enteredText = txtAttribute.Trim();
                                selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            var httpPostedFile = this.Request.Files[controlId];
                            if ((httpPostedFile != null) && (!String.IsNullOrEmpty(httpPostedFile.FileName)))
                            {
                                int fileMaxSize = _catalogSettings.FileUploadMaximumSizeBytes;
                                if (httpPostedFile.ContentLength > fileMaxSize)
                                {
                                    //TODO display warning
                                    //warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), (int)(fileMaxSize / 1024)));
                                }
                                else
                                {
                                    //save an uploaded file
                                    var download = new Download()
                                    {
                                        DownloadGuid = Guid.NewGuid(),
                                        UseDownloadUrl = false,
                                        DownloadUrl = "",
                                        DownloadBinary = httpPostedFile.GetDownloadBits(),
                                        ContentType = httpPostedFile.ContentType,
                                        Filename = System.IO.Path.GetFileNameWithoutExtension(httpPostedFile.FileName),
                                        Extension = System.IO.Path.GetExtension(httpPostedFile.FileName),
                                        IsNew = true
                                    };
                                    _downloadService.InsertDownload(download);
                                    //save attribute
                                    selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                        attribute, download.DownloadGuid.ToString());
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //save checkout attributes
            _workContext.CurrentCustomer.CheckoutAttributes = selectedAttributes;
            _customerService.UpdateCustomer(_workContext.CurrentCustomer);

            //validate attributes
            var checkoutAttributeWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, _workContext.CurrentCustomer.CheckoutAttributes, true);
            if (checkoutAttributeWarnings.Count > 0)
            {
                //something wrong, redisplay the page with warnings
                var model = new ShoppingCartModel();
                PrepareShoppingCartModel(model, cart, validateCheckoutAttributes: true);
                return View(model);
            }

                return RedirectToRoute("Checkout");
        }

        [HttpPost]
        public ActionResult AddAlbionProductToCart(int productId, bool forceredirection = false)
        {
            //current we support only ShoppingCartType.ShoppingCart
            const ShoppingCartType shoppingCartType = ShoppingCartType.ShoppingCart;

            var product = _productService.GetProductById(productId);
            if (product == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            var productVariants = _productService.GetProductVariantsByProductId(productId);
            //if (productVariants.Count != 1)
            //{
            //    //we can add a product to the cart only if it has exactly one product variant
            //    return Json(new
            //    {
            //        redirect = Url.RouteUrl("Product", new { productId = product.Id, SeName = product.GetSeName() }),
            //    });
            //}

            //get default product variant
            var defaultProductVariant = productVariants[0];
            //if (defaultProductVariant.CustomerEntersPrice)
            //{
            //    //cannot be added to the cart (requires a customer to enter price)
            //    return Json(new
            //    {
            //        redirect = Url.RouteUrl("Product", new { productId = product.Id, SeName = product.GetSeName() }),
            //    });
            //}

            //quantity to add
            var qtyToAdd = defaultProductVariant.OrderMinimumQuantity > 0 ?
                defaultProductVariant.OrderMinimumQuantity : 1;

            var allowedQuantities = defaultProductVariant.ParseAllowedQuatities();
            //if (allowedQuantities.Length > 0)
            //{
            //    //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
            //    return Json(new
            //    {
            //        redirect = Url.RouteUrl("Product", new { productId = product.Id, SeName = product.GetSeName() }),
            //    });
            //}

            //get standard warnings without attribute validations
           // first, try to find existing shopping cart item
            var cart = _workContext
                .CurrentCustomer
                .ShoppingCartItems
                .Where(sci => sci.ShoppingCartType == shoppingCartType)
                .ToList();
            var shoppingCartItem = _shoppingCartService
                .FindShoppingCartItemInTheCart(cart, shoppingCartType, defaultProductVariant);
            //if we already have the same product variant in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ?
                shoppingCartItem.Quantity + qtyToAdd : qtyToAdd;
            var addToCartWarnings = _shoppingCartService
                .GetShoppingCartItemWarnings(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart,
                defaultProductVariant, string.Empty, decimal.Zero, quantityToValidate, false, true, false, false, false);
            if (addToCartWarnings.Count > 0)
            {
                //cannot be added to the cart
                //let's display standard warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = _shoppingCartService.AddToCart(_workContext.CurrentCustomer,
                defaultProductVariant, ShoppingCartType.ShoppingCart,
                string.Empty, decimal.Zero, qtyToAdd, true);
            //if (addToCartWarnings.Count > 0)
            //{
            //    //cannot be added to the cart
            //    //but we do not display attribute and gift card warnings here. let's do it on the product details page
            //    return Json(new
            //    {
            //        redirect = Url.RouteUrl("Product", new { productId = product.Id, SeName = product.GetSeName() }),
            //    });
            //}

            //added to the cart
            if (_shoppingCartSettings.DisplayCartAfterAddingProduct ||
                forceredirection)
            {
                //redirect to the shopping cart page
                return Json(new
                {
                    redirect = Url.RouteUrl("ShoppingCart"),
                });
            }

            //display notification message and update appropriate blocks
            var updatetopcartsectionhtml = string.Format("({0})",
                 _workContext
                 .CurrentCustomer
                 .ShoppingCartItems
                 .Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart)
                 .ToList()
                 .GetTotalProducts());
            var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                ? this.RenderPartialViewToString("FlyoutShoppingCart", PrepareMiniShoppingCartModel())
                : "";

            return Json(new
            {
                success = true,
                message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                updatetopcartsectionhtml = updatetopcartsectionhtml,
                updateflyoutcartsectionhtml = updateflyoutcartsectionhtml,
            });

        }

    }
}
