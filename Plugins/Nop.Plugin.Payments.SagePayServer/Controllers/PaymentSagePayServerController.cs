using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualBasic;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.SagePayServer.Domain;
using Nop.Plugin.Payments.SagePayServer.Models;
using Nop.Plugin.Payments.SagePayServer.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using System.Web;

namespace Nop.Plugin.Payments.SagePayServer.Controllers
{
    public class PaymentSagePayServerController : BaseNopPaymentController
    {
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly SagePayServerPaymentSettings _sagePayServerPaymentSettings;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;

        private readonly IWorkContext _workContext;
        private readonly ISagePayServerTransactionService _sagePayServerTransactionService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;

        private readonly IMobileDeviceHelper _mobileDeviceHelper;
        private readonly OrderSettings _orderSettings;

        private readonly HttpContextBase _httpContext;

        public PaymentSagePayServerController(ISettingService settingService, 
            IPaymentService paymentService, IOrderService orderService, 
            IOrderProcessingService orderProcessingService,
            ILogger logger, SagePayServerPaymentSettings sagePayServerPaymentSettings,
            PaymentSettings paymentSettings, ILocalizationService localizationService,
            IWorkContext workContext, ISagePayServerTransactionService sagePayServerTransactionService,
            IOrderTotalCalculationService orderTotalCalculationService, ICurrencyService currencyService, CurrencySettings currencySettings,
            IMobileDeviceHelper mobileDeviceHelper, OrderSettings orderSettings, HttpContextBase httpContext)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
            this._sagePayServerTransactionService = sagePayServerTransactionService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._currencyService = currencyService;

            this._sagePayServerPaymentSettings = sagePayServerPaymentSettings;
            this._paymentSettings = paymentSettings;
            this._currencySettings = currencySettings;
            this._orderSettings = orderSettings;

            this._logger = logger;

            this._workContext = workContext;

            this._httpContext = httpContext;

            this._mobileDeviceHelper = mobileDeviceHelper;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.AdditionalFee = _sagePayServerPaymentSettings.AdditionalFee;
            model.ConnectTo = _sagePayServerPaymentSettings.ConnectTo;

            model.PartnerID = _sagePayServerPaymentSettings.PartnerID;
            model.TransactType = _sagePayServerPaymentSettings.TransactType;
           // model.VendorDescription = _sagePayServerPaymentSettings.VendorDescription;
            model.VendorName = _sagePayServerPaymentSettings.VendorName;
            model.NotificationFullyQualifiedDomainName = _sagePayServerPaymentSettings.NotificationFullyQualifiedDomainName;
            model.ReturnFullyQualifiedDomainName = _sagePayServerPaymentSettings.ReturnFullyQualifiedDomainName;
            model.Profile = _sagePayServerPaymentSettings.Profile;

            model.ConnectToList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.ConnectToValues.SIMULATOR });
            model.ConnectToList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.ConnectToValues.TEST });
            model.ConnectToList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.ConnectToValues.LIVE });
            model.TransactTypeList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.TransactTypeValues.PAYMENT });
            model.TransactTypeList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.TransactTypeValues.DEFERRED });
            model.TransactTypeList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.TransactTypeValues.AUTHENTICATE });
            model.ProfileList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.ProfileValues.NORMAL });
            model.ProfileList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.ProfileValues.LOW });

            return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _sagePayServerPaymentSettings.AdditionalFee = model.AdditionalFee;
            _sagePayServerPaymentSettings.ConnectTo = model.ConnectTo;
            _sagePayServerPaymentSettings.PartnerID = model.PartnerID;
            _sagePayServerPaymentSettings.TransactType = model.TransactType;
           // _sagePayServerPaymentSettings.VendorDescription = model.VendorDescription;
            _sagePayServerPaymentSettings.VendorName = model.VendorName;
            _sagePayServerPaymentSettings.NotificationFullyQualifiedDomainName = model.NotificationFullyQualifiedDomainName;
            _sagePayServerPaymentSettings.ReturnFullyQualifiedDomainName = model.ReturnFullyQualifiedDomainName;
            _sagePayServerPaymentSettings.Profile = model.Profile;
            _settingService.SaveSetting(_sagePayServerPaymentSettings);


            model.ConnectToList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.ConnectToValues.SIMULATOR });
            model.ConnectToList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.ConnectToValues.TEST });
            model.ConnectToList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.ConnectToValues.LIVE });
            model.TransactTypeList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.TransactTypeValues.PAYMENT });
            model.TransactTypeList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.TransactTypeValues.DEFERRED });
            model.TransactTypeList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.TransactTypeValues.AUTHENTICATE });
            model.ProfileList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.ProfileValues.NORMAL });
            model.ProfileList.Add(new SelectListItem() { Text = SagePayServerPaymentSettings.ProfileValues.LOW });

            return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentSagePayServerModel();

            //First validate if this is the response of failed transaction (Status INVALID)
            var StatusDetail = Request.QueryString["StatusDetail"];

            if (StatusDetail != null)
            {
                model.Warnings.Add(StatusDetail);
                return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.PaymentInfo", model);
            }

            var webClient = new WebClient();

            var data = new NVPCodec();

            data.Add("VPSProtocol", SagePayHelper.GetProtocol());
            data.Add("TxType", _sagePayServerPaymentSettings.TransactType);
            data.Add("Vendor", _sagePayServerPaymentSettings.VendorName.ToLower());

            var orderGuid = Guid.NewGuid();

            data.Add("VendorTxCode", orderGuid.ToString());

            if (!String.IsNullOrWhiteSpace(_sagePayServerPaymentSettings.PartnerID))
                data.Add("ReferrerID", _sagePayServerPaymentSettings.PartnerID);

            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            
            decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart);

            var OrderTotal = shoppingCartTotalBase.GetValueOrDefault();

            data.Add("Amount", OrderTotal.ToString("F2", CultureInfo.InvariantCulture));

            if (_workContext.WorkingCurrency != null)
                data.Add("Currency", _workContext.WorkingCurrency.CurrencyCode);
            else if (_workContext.CurrentCustomer.CurrencyId.HasValue && _workContext.CurrentCustomer.Currency != null)
                data.Add("Currency", _workContext.CurrentCustomer.Currency.CurrencyCode);
            else
                data.Add("Currency", _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode);                        

        //    data.Add("Description", _sagePayServerPaymentSettings.VendorDescription);

            // The Notification URL is the page to which Server calls back when a transaction completes

            var notificationUrl = _sagePayServerPaymentSettings.NotificationFullyQualifiedDomainName;

            data.Add("NotificationURL", notificationUrl + "Plugins/PaymentSagePayServer/NotificationPage");

            // Billing Details
            data.Add("BillingSurname", _workContext.CurrentCustomer.BillingAddress.LastName);
            data.Add("BillingFirstnames", _workContext.CurrentCustomer.BillingAddress.FirstName);
            data.Add("BillingAddress1", _workContext.CurrentCustomer.BillingAddress.Address1);

            if (!String.IsNullOrWhiteSpace(_workContext.CurrentCustomer.BillingAddress.Address2))
                data.Add("BillingAddress2", _workContext.CurrentCustomer.BillingAddress.Address2);

            data.Add("BillingCity", _workContext.CurrentCustomer.BillingAddress.City);
            data.Add("BillingPostCode", _workContext.CurrentCustomer.BillingAddress.ZipPostalCode);
            data.Add("BillingCountry", _workContext.CurrentCustomer.BillingAddress.Country.TwoLetterIsoCode); //TODO: Verify if it is ISO 3166-1 country code

            if (_workContext.CurrentCustomer.BillingAddress.StateProvince != null)
                data.Add("BillingState", _workContext.CurrentCustomer.BillingAddress.StateProvince.Abbreviation);

            if (!String.IsNullOrWhiteSpace(_workContext.CurrentCustomer.BillingAddress.PhoneNumber))
                data.Add("BillingPhone", _workContext.CurrentCustomer.BillingAddress.PhoneNumber);


            // Delivery Details
            if (_workContext.CurrentCustomer.ShippingAddress != null)
            {
                data.Add("DeliverySurname", _workContext.CurrentCustomer.ShippingAddress.LastName);
                data.Add("DeliveryFirstnames", _workContext.CurrentCustomer.ShippingAddress.FirstName);
                data.Add("DeliveryAddress1", _workContext.CurrentCustomer.ShippingAddress.Address1);

                if (!String.IsNullOrWhiteSpace(_workContext.CurrentCustomer.ShippingAddress.Address2))
                    data.Add("DeliveryAddress2", _workContext.CurrentCustomer.ShippingAddress.Address2);

                data.Add("DeliveryCity", _workContext.CurrentCustomer.ShippingAddress.City);
                data.Add("DeliveryPostCode", _workContext.CurrentCustomer.ShippingAddress.ZipPostalCode);

                if (_workContext.CurrentCustomer.ShippingAddress.Country != null)
                {
                    data.Add("DeliveryCountry", _workContext.CurrentCustomer.ShippingAddress.Country.TwoLetterIsoCode);
                }

                if (_workContext.CurrentCustomer.ShippingAddress.StateProvince != null)
                    data.Add("DeliveryState", _workContext.CurrentCustomer.ShippingAddress.StateProvince.Abbreviation);

                if (!String.IsNullOrWhiteSpace(_workContext.CurrentCustomer.ShippingAddress.PhoneNumber))
                    data.Add("DeliveryPhone", _workContext.CurrentCustomer.ShippingAddress.PhoneNumber);

            }
            else {
                //Thanks to 'nomisit' for pointing this out. http://www.nopcommerce.com/p/258/sagepay-server-integration-iframe-and-redirect-methods.aspx
                data.Add("DeliverySurname", "");
                data.Add("DeliveryFirstnames", "");
                data.Add("DeliveryAddress1", "");
                data.Add("DeliveryAddress2", "");
                data.Add("DeliveryCity", "");
                data.Add("DeliveryPostCode", "");
                data.Add("DeliveryCountry", "");
                data.Add("DeliveryState", "");
                data.Add("DeliveryPhone", "");
            }

            data.Add("CustomerEMail", _workContext.CurrentCustomer.Email);

            //var strBasket = String.Empty;
            //strBasket = cart.Count + ":";

            //for (int i = 0; i < cart.Count; i++)
            //{
            //    ShoppingCartItem item = cart[i];
            //    strBasket += item.ProductVariant.FullProductName) + ":" +
            //                    item.Quantity + ":" + item.ProductVariant.Price + ":" +
            //                    item.ProductVariant.TaxCategoryId;
            //};

            //data.Add("Basket", strBasket);

            data.Add("AllowGiftAid", "0");

            // Allow fine control over AVS/CV2 checks and rules by changing this value. 0 is Default
            if (_sagePayServerPaymentSettings.TransactType != "AUTHENTICATE") data.Add("ApplyAVSCV2", "0");

            // Allow fine control over 3D-Secure checks and rules by changing this value. 0 is Default
            data.Add("Apply3DSecure", "0");

            if (String.Compare(_sagePayServerPaymentSettings.Profile, "LOW", true) == 0)
            {
                data.Add("Profile", "LOW"); //simpler payment page version.
            }

            var postURL = SagePayHelper.GetSageSystemUrl(_sagePayServerPaymentSettings.ConnectTo, "purchase");

            string strResponse = string.Empty;

            try
            {

                Byte[] responseData = webClient.UploadValues(postURL, data);

                strResponse = Encoding.ASCII.GetString(responseData);


            }
            catch (WebException ex)
            {

                return Content(String.Format(
                    @"Your server was unable to register this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}. <br/>
                    The Status Number is: {1}<br/>
                    The Description given is: {2}", postURL, ex.Status, ex.Message));

            }

            if (string.IsNullOrWhiteSpace(strResponse))
                return Content(String.Format(
                    @"Your server was unable to register this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}.", postURL));

            var strStatus = SagePayHelper.FindField("Status", strResponse);
            var strStatusDetail = SagePayHelper.FindField("StatusDetail", strResponse);

            switch (strStatus)
            {
                case "OK":

                    var strVPSTxId = SagePayHelper.FindField("VPSTxId", strResponse);
                    var strSecurityKey = SagePayHelper.FindField("SecurityKey", strResponse);
                    var strNextURL = SagePayHelper.FindField("NextURL", strResponse);

                    var transx = new SagePayServerTransaction()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        VPSTxId = strVPSTxId,
                        SecurityKey = strSecurityKey,
                        NotificationResponse = strResponse,
                        VendorTxCode = orderGuid.ToString()
                    };

                    //Store this record in DB
                    _sagePayServerTransactionService.InsertSagePayServerTransaction(transx);
                   
                    
                    ViewBag.UseOnePageCheckout = UseOnePageCheckout();

                    if (_sagePayServerPaymentSettings.Profile == SagePayServerPaymentSettings.ProfileValues.LOW || ViewBag.UseOnePageCheckout)
                    {//Iframe
                        model.FrameURL = strNextURL;

                        return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.PaymentInfo", model);
                    }
                    else {
                        HttpContext.Response.Redirect(strNextURL);
                        HttpContext.Response.End();

                        return null;
                    }


                case "MALFORMED":
                    return Content(string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));

                case "INVALID":
                    return Content(string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));

                default:
                    return Content(string.Format("Error ({0}: {1})", strStatus, strStatusDetail));

            }

        }
        /// <summary>
        /// Action performed right after Sage pay Redirects from the Notification page. **It does not validate the session (if inside an iframe)
        /// </summary>
        /// <returns></returns>
        public ActionResult PaymentInfoScripts()
        {
            return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.PaymentInfoScripts");
        }
        
        public ActionResult NotificationPage()
        {
            string strTxAuthNo = String.Empty;
            string strAVSCV2 = String.Empty;
            string strAddressResult = String.Empty;
            string strPostCodeResult = String.Empty;
            string strCV2Result = String.Empty;
            string strGiftAid = String.Empty;
            string str3DSecureStatus = String.Empty;
            string strCAVV = String.Empty;
            string strAddressStatus = String.Empty;
            string strPayerStatus = String.Empty;
            string strCardType = String.Empty;
            string strLast4Digits = String.Empty;

            string strVPSTxId = (String)Request.Params["VPSTxId"];
            string strVPSSignature = (String)Request.Params["VPSSignature"];
            string strStatus = (String)Request.Params["Status"];
            string strStatusDetail = (String)Request.Params["StatusDetail"];
            string strVendorTxCode = (String)Request.Params["VendorTxCode"];


            string strVendorName = _sagePayServerPaymentSettings.VendorName.ToLower();
       
            //Obtain from DB
            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(strVendorTxCode);
            

            var returnUrl = _sagePayServerPaymentSettings.ReturnFullyQualifiedDomainName;

            if (transx == null)
            {
                strStatusDetail = "Vendor Transaction code " + strVendorTxCode + " does not exist.";

                return Content("Status=INVALID" + Environment.NewLine +
                               "RedirectURL=" + returnUrl + "Plugins/PaymentSagePayServer/ResponsePage?uid=" + strVendorTxCode + Environment.NewLine +
                               "StatusDetail=" + strStatusDetail);
            }

            if (string.IsNullOrWhiteSpace(transx.SecurityKey))
            {
                strStatusDetail = "Security Key for transaction " + strVendorTxCode + " is empty.";

                return Content("Status=INVALID" + Environment.NewLine +
                               "RedirectURL=" + returnUrl + "Plugins/PaymentSagePayServer/ResponsePage?uid=" + strVendorTxCode + Environment.NewLine +
                               "StatusDetail=" + strStatusDetail);
            }

            

            if (String.IsNullOrWhiteSpace(Request.Params["TxAuthNo"]) == false) strTxAuthNo = (string)Request.Params["TxAuthNo"];
            if (String.IsNullOrWhiteSpace(Request.Params["AVSCV2"]) == false) strAVSCV2 = (string)Request.Params["AVSCV2"];
            if (String.IsNullOrWhiteSpace(Request.Params["AddressResult"]) == false) strAddressResult = (string)Request.Params["AddressResult"];
            if (String.IsNullOrWhiteSpace(Request.Params["PostCodeResult"]) == false) strPostCodeResult = (string)Request.Params["PostCodeResult"];

            if (String.IsNullOrWhiteSpace(Request.Params["CV2Result"]) == false) strCV2Result = (string)Request.Params["CV2Result"];
            if (String.IsNullOrWhiteSpace(Request.Params["GiftAid"]) == false) strGiftAid = (string)Request.Params["GiftAid"];
            if (String.IsNullOrWhiteSpace(Request.Params["3DSecureStatus"]) == false) str3DSecureStatus = (string)Request.Params["3DSecureStatus"];
            if (String.IsNullOrWhiteSpace(Request.Params["CAVV"]) == false) strCAVV = (string)Request.Params["CAVV"];

            if (String.IsNullOrWhiteSpace(Request.Params["AddressStatus"]) == false) strAddressStatus = (string)Request.Params["AddressStatus"];
            if (String.IsNullOrWhiteSpace(Request.Params["PayerStatus"]) == false) strPayerStatus = (string)Request.Params["PayerStatus"];
            if (String.IsNullOrWhiteSpace(Request.Params["CardType"]) == false) strCardType = (string)Request.Params["CardType"];
            if (String.IsNullOrWhiteSpace(Request.Params["Last4Digits"]) == false) strLast4Digits = (string)Request.Params["Last4Digits"];


            //Update DB with what we've got so far
            transx.VPSTxId = strVPSTxId;
            transx.VPSSignature = strVPSSignature;
            transx.Status = strStatus;
            transx.StatusDetail = strStatusDetail;
            transx.TxAuthNo = strTxAuthNo;
            transx.AVSCV2 = strAVSCV2;
            transx.AddressResult = strAddressResult;
            transx.PostCodeResult = strPostCodeResult;
            transx.CV2Result = strCV2Result;
            transx.GiftAid = strGiftAid;
            transx.ThreeDSecureStatus = str3DSecureStatus;
            transx.CAVV = strCAVV;
            transx.AddressStatus = strAddressStatus;
            transx.PayerStatus = strPayerStatus;
            transx.CardType = strCardType;
            transx.Last4Digits = strLast4Digits;

            //Update DB with what we've got so far
            _sagePayServerTransactionService.UpdateSagePayServerTransaction(transx);

            string strMessage = strVPSTxId + strVendorTxCode + strStatus + strTxAuthNo + strVendorName + strAVSCV2 + transx.SecurityKey +
               strAddressResult + strPostCodeResult + strCV2Result + strGiftAid + str3DSecureStatus + strCAVV +
               strAddressStatus + strPayerStatus + strCardType + strLast4Digits;

            string strMySignature = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(strMessage, "MD5");

            if (strMySignature != strVPSSignature)
            {
                transx.StatusDetail = "Your server was unable to register this transaction with Sage Pay. Cannot match the MD5 Hash. Order might be tampered with: " + strMessage;
                
                _sagePayServerTransactionService.UpdateSagePayServerTransaction(transx);

                return Content("Status=INVALID" + Environment.NewLine +
                               "RedirectURL=" + returnUrl + "Plugins/PaymentSagePayServer/ResponsePage?uid=" + strVendorTxCode + Environment.NewLine +
                               "StatusDetail=Your server was unable to register this transaction with Sage Pay. Cannot match the MD5 Hash. Order might be tampered with");
            }

            //Always send a Status of OK if we've read everything correctly. Only INVALID for messages with a Status of ERROR
            string strResponseStatus = String.Empty;

            if (strStatus == "ERROR")
                strResponseStatus = "INVALID";
            else
                strResponseStatus = "OK";

            return Content("Status=" + strResponseStatus + Environment.NewLine +
                           "RedirectURL=" + returnUrl + "Plugins/PaymentSagePayServer/ResponsePage?uid=" + strVendorTxCode);

        }

        /// <summary>
        /// Action performed right after Sage pay Redirects from the Notification page. **It does not validate the session (if inside an iframe)
        /// </summary>
        /// <returns></returns>
        public ActionResult ResponsePage()
        {

            var model = new PaymentSagePayServerModel();

            var strOrderGuid = Request.QueryString["uid"];

            if (String.IsNullOrWhiteSpace(strOrderGuid))
            {
                model.Warnings.Add("Order Unique identifier code does not exist!");
                return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.ResponsePage", model);
            }

            var orderGuid = new Guid();

            if (Guid.TryParse(strOrderGuid, out orderGuid) == false)
            {
                model.Warnings.Add("Order Unique identifier is not valid!");
                return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.ResponsePage", model);
            }

            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(orderGuid.ToString());

            if (transx == null)
            {
                model.Warnings.Add(String.Format("SagePay Server vendor transaction code {0} does not exist.", orderGuid.ToString()));
                return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.ResponsePage", model);
            }

            if ((transx.Status == "OK") || (transx.Status == "AUTHENTICATED") || (transx.Status == "REGISTERED"))
            {
                model.TransactionId = orderGuid;
            }
            else
            {
                model.Warnings.Add(transx.StatusDetail);
            }

            ViewBag.UseOnePageCheckout = UseOnePageCheckout();
            ViewBag.Iframe = false;

            if (_sagePayServerPaymentSettings.Profile == SagePayServerPaymentSettings.ProfileValues.LOW || ViewBag.UseOnePageCheckout)
            {
                ViewBag.Iframe = true;
            }



            return View("Nop.Plugin.Payments.SagePayServer.Views.PaymentSagePayServer.ResponsePage", model);
        }

        [NonAction]
        protected bool UseOnePageCheckout()
        {
            bool useMobileDevice = _mobileDeviceHelper.IsMobileDevice(_httpContext)
                && _mobileDeviceHelper.MobileDevicesSupported()
                && !_mobileDeviceHelper.CustomerDontUseMobileVersion();

            //mobile version doesn't support one-page checkout
            if (useMobileDevice)
                return false;

            //check the appropriate setting
            return _orderSettings.OnePageCheckoutEnabled;
        }

        /// <summary>
        /// Validates the iframe (not form) to see if the Notification response was valid
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {

            var list = new List<string>();

            var strTransactionId = _httpContext.Request.Form["transactionId"];

            if (String.IsNullOrWhiteSpace(strTransactionId))
            {
                list.Add("Transaction Id does not exist or it is empty.");
                return list;
            }

            Guid transactionId;

            if (Guid.TryParse(strTransactionId, out transactionId) == false)
            {
                list.Add(String.Format("Transcation Id {0} is invalid", strTransactionId));
            }

            return list;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {

            var strTransactionId = _httpContext.Request.Form["transactionId"];

            Guid transactionId;

            Guid.TryParse(strTransactionId, out transactionId);


            var paymentInfo = new ProcessPaymentRequest();
            paymentInfo.OrderGuid = transactionId;

            return paymentInfo;
        }

    }
}