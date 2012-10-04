using System;
using System.Web;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Directory;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.SagePayServer.Controllers;
using Nop.Plugin.Payments.SagePayServer.Data;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Payments;
using System.Net;
using System.Globalization;
using System.Text;
using Nop.Core.Domain.Payments;
using Nop.Services.Orders;
using System.Web.Mvc;
using Nop.Web.Framework.Controllers;
using Nop.Plugin.Payments.SagePayServer.Services;
using Microsoft.VisualBasic;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Payments.SagePayServer
{
    /// <summary>
    /// SagePayServer payment processor
    /// </summary>
    public class SagePayServerPaymentProcessor : BasePlugin, IPaymentMethod
    {

        #region Fields

        private readonly SagePayServerTransactionObjectContext _context;
        private readonly SagePayServerPaymentSettings _sagePayServerPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly ISagePayServerTransactionService _sagePayServerTransactionService;
        #endregion

        #region Ctor

        public SagePayServerPaymentProcessor(SagePayServerPaymentSettings sagePayServerPaymentSettings,
            ISettingService settingService, ICurrencyService currencyService, IOrderProcessingService orderProcessingService,
            CurrencySettings currencySettings, IWebHelper webHelper, IWorkContext workContext,
            StoreInformationSettings storeInformationSettings, SagePayServerTransactionObjectContext context,
            ISagePayServerTransactionService sagePayServerTransactionService)
        {
            this._context = context;
            this._sagePayServerPaymentSettings = sagePayServerPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._orderProcessingService = orderProcessingService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            this._workContext = workContext;
            this._storeInformationSettings = storeInformationSettings;
            this._sagePayServerTransactionService = sagePayServerTransactionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            var orderGuid = processPaymentRequest.OrderGuid;


            if (orderGuid == Guid.NewGuid())
            {
                result.AddError("SagePay Server transaction code does not exist!");
                return result;
            }

            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(orderGuid.ToString());

            if (transx == null)
            {
                result.AddError(String.Format("SagePay Server transaction code {0} does not exist.", orderGuid.ToString()));
                return result;
            }

            if ((transx.Status == "OK") || (transx.Status == "AUTHENTICATED") || (transx.Status == "REGISTERED"))
            {
                if (_sagePayServerPaymentSettings.TransactType == SagePayServerPaymentSettings.TransactTypeValues.PAYMENT)
                    result.NewPaymentStatus = PaymentStatus.Paid;
                else if (_sagePayServerPaymentSettings.TransactType == SagePayServerPaymentSettings.TransactTypeValues.DEFERRED)
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                else
                    result.NewPaymentStatus = PaymentStatus.Pending;
                result.AuthorizationTransactionId = transx.Id.ToString();
                result.AuthorizationTransactionCode = transx.VPSTxId;
                result.AuthorizationTransactionResult = transx.ToString();
            }
            else
            {
                result.AddError(transx.StatusDetail);
            }


            return result;

        }


        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            var orderGuid = capturePaymentRequest.Order.OrderGuid;

            if (orderGuid == Guid.NewGuid())
            {
                result.AddError("Order Unique identifier does not exist!");
                return result;
            }

            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(orderGuid.ToString());

            if (transx == null)
            {
                result.AddError(String.Format("SagePay Server vendor transaction code {0} does not exist.", orderGuid.ToString()));
                return result;
            }


            var webClient = new WebClient();

            var data = new NVPCodec();

            data.Add("VPSProtocol", SagePayHelper.GetProtocol());
            data.Add("TxType", "RELEASE");
            data.Add("Vendor", _sagePayServerPaymentSettings.VendorName);
            data.Add("VendorTxCode", orderGuid.ToString());
            data.Add("VPSTxId", transx.VPSTxId);
            data.Add("SecurityKey", transx.SecurityKey);
            data.Add("TxAuthNo", transx.TxAuthNo);

            data.Add("ReleaseAmount", capturePaymentRequest.Order.OrderTotal.ToString("F2", CultureInfo.InvariantCulture));

            var postURL = SagePayHelper.GetSageSystemUrl(_sagePayServerPaymentSettings.ConnectTo, "release");

            string strResponse = string.Empty;

            try
            {

                Byte[] responseData = webClient.UploadValues(postURL, data);

                strResponse = Encoding.ASCII.GetString(responseData);


            }
            catch (WebException ex)
            {

                result.AddError(String.Format(
                    @"Your server was unable to release this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}. <br/>
                    The Status Number is: {1}<br/>
                    The Description given is: {2}", postURL, ex.Status, ex.Message));
                return result;
            }

            if (string.IsNullOrWhiteSpace(strResponse))
            {
                result.AddError(String.Format(
                    @"Your server was unable to register this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}.", postURL));
                return result;
            }
            var strStatus = SagePayHelper.FindField("Status", strResponse);
            var strStatusDetail = SagePayHelper.FindField("StatusDetail", strResponse);

            switch (strStatus)
            {
                case "OK":

                    result.NewPaymentStatus = PaymentStatus.Paid;
                    //result.CaptureTransactionId = null;
                    result.CaptureTransactionResult = strStatusDetail;

                    break;

                case "MALFORMED":
                    result.AddError(string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    return result;

                case "INVALID":
                    result.AddError(string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    return result;

                default:
                    result.AddError(string.Format("Error ({0}: {1})", strStatus, strStatusDetail));
                    return result;

            }

            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();

            var orderGuid = refundPaymentRequest.Order.OrderGuid;

            if (orderGuid == Guid.NewGuid())
            {
                result.AddError("Order Unique identifier code does not exist!");
                return result;
            }

            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(orderGuid.ToString());

            if (transx == null)
            {
                result.AddError(String.Format("SagePay Server vendor transaction code {0} does not exist.", orderGuid.ToString()));
                return result;
            }

            var webClient = new WebClient();

            var data = new NVPCodec();

            data.Add("VPSProtocol", SagePayHelper.GetProtocol());
            data.Add("TxType", "REFUND");
            data.Add("Vendor", _sagePayServerPaymentSettings.VendorName);

            var returnGuid = Guid.NewGuid();
            data.Add("VendorTxCode", returnGuid.ToString());

            data.Add("VPSTxId", transx.VPSTxId);
            data.Add("SecurityKey", transx.SecurityKey);
            data.Add("TxAuthNo", transx.TxAuthNo);

            data.Add("Amount", refundPaymentRequest.AmountToRefund.ToString("F2", CultureInfo.InvariantCulture));

            data.Add("Currency", refundPaymentRequest.Order.CustomerCurrencyCode);

            data.Add("Description", "---");

            data.Add("RelatedVPSTxId", transx.VPSTxId);
            data.Add("RelatedVendorTxCode", orderGuid.ToString());
            data.Add("RelatedSecurityKey", transx.SecurityKey);
            data.Add("RelatedTxAuthNo", transx.TxAuthNo);

            var postURL = SagePayHelper.GetSageSystemUrl(_sagePayServerPaymentSettings.ConnectTo, "refund");

            string strResponse = string.Empty;

            try
            {

                Byte[] responseData = webClient.UploadValues(postURL, data);

                strResponse = Encoding.ASCII.GetString(responseData);


            }
            catch (WebException ex)
            {

                result.AddError(String.Format(
                    @"Your server was unable to release this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}. <br/>
                    The Status Number is: {1}<br/>
                    The Description given is: {2}", postURL, ex.Status, ex.Message));
                return result;
            }

            if (string.IsNullOrWhiteSpace(strResponse))
            {
                result.AddError(String.Format(
                    @"Your server was unable to register this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}.", postURL));
                return result;
            }
            var strStatus = SagePayHelper.FindField("Status", strResponse);
            var strStatusDetail = SagePayHelper.FindField("StatusDetail", strResponse);

            switch (strStatus)
            {
                case "OK":

                    result.NewPaymentStatus = PaymentStatus.Refunded;

                    break;

                case "MALFORMED":
                    result.AddError(string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    return result;

                case "INVALID":
                    result.AddError(string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    return result;

                default:
                    result.AddError(string.Format("Error ({0}: {1})", strStatus, strStatusDetail));
                    return result;

            }

            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();

            var orderGuid = voidPaymentRequest.Order.OrderGuid;

            if (orderGuid == Guid.NewGuid())
            {
                result.AddError("Order Unique identifier code does not exist!");
                return result;
            }

            var transx = _sagePayServerTransactionService.GetSagePayServerTransactionByVendorTxCode(orderGuid.ToString());

            if (transx == null)
            {
                result.AddError(String.Format("SagePay Server vendor transaction code {0} does not exist.", orderGuid.ToString()));
                return result;
            }

            var webClient = new WebClient();

            var data = new NVPCodec();

            data.Add("VPSProtocol", SagePayHelper.GetProtocol());
            data.Add("TxType", "VOID");
            data.Add("Vendor", _sagePayServerPaymentSettings.VendorName);

            var voidGuid = Guid.NewGuid();
            data.Add("VendorTxCode", voidGuid.ToString());

            data.Add("VPSTxId", transx.VPSTxId);
            data.Add("SecurityKey", transx.SecurityKey);
            data.Add("TxAuthNo", transx.TxAuthNo);

            var postURL = SagePayHelper.GetSageSystemUrl(_sagePayServerPaymentSettings.ConnectTo, "void");

            string strResponse = string.Empty;

            try
            {

                Byte[] responseData = webClient.UploadValues(postURL, data);

                strResponse = Encoding.ASCII.GetString(responseData);


            }
            catch (WebException ex)
            {

                result.AddError(String.Format(
                    @"Your server was unable to release this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}. <br/>
                    The Status Number is: {1}<br/>
                    The Description given is: {2}", postURL, ex.Status, ex.Message));
                return result;
            }

            if (string.IsNullOrWhiteSpace(strResponse))
            {
                result.AddError(String.Format(
                    @"Your server was unable to register this transaction with Sage Pay.
                    Check that you do not have a firewall restricting the POST and 
                    that your server can correctly resolve the address {0}.", postURL));
                return result;
            }
            var strStatus = SagePayHelper.FindField("Status", strResponse);
            var strStatusDetail = SagePayHelper.FindField("StatusDetail", strResponse);

            switch (strStatus)
            {
                case "OK":

                    result.NewPaymentStatus = PaymentStatus.Voided;

                    break;

                case "MALFORMED":
                    result.AddError(string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    return result;

                case "INVALID":
                    result.AddError(string.Format("Error ({0}: {1}) <br/> {2}", strStatus, strStatusDetail, data.Encode()));
                    return result;

                default:
                    result.AddError(string.Format("Error ({0}: {1})", strStatus, strStatusDetail));
                    return result;

            }

            return result;
        }

        /// <summary>
        /// Process recurring payment. Not supported in Sage Pay
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }


        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee()
        {
            return _sagePayServerPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentSagePayServer";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.SagePayServer.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentSagePayServer";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.SagePayServer.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentSagePayServerController);
        }

        public override void Install()
        {
            var settings = new SagePayServerPaymentSettings()
            {
                TransactType = SagePayServerPaymentSettings.TransactTypeValues.PAYMENT,
                ConnectTo = SagePayServerPaymentSettings.ConnectToValues.SIMULATOR,
                Profile = SagePayServerPaymentSettings.ProfileValues.NORMAL
            };
            _settingService.SaveSetting(settings);

            _context.InstallSchema();

            base.Install();
        }

        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Standard;
            }
        }

        #endregion


    }
}