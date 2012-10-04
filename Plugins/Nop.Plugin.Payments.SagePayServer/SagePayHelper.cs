
using System;
using System.Text;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Directory;
using System.Web;

namespace Nop.Plugin.Payments.SagePayServer
{
    /// <summary>
    /// Represents paypal helper
    /// </summary>
    public class SagePayHelper
    {


        /// <summary>
        /// Gets Sage Pay URL
        /// </summary>
        /// <returns></returns>
        public static string GetSageSystemUrl(string connectTo, string strType)
        {
            string strSystemURL = String.Empty;

            if (string.Compare(connectTo, "LIVE", true) == 0)
            {
                switch (strType)
                {
                    case "abort":
                        strSystemURL = "https://live.sagepay.com/gateway/service/abort.vsp";
                        break;
                    case "authorise":
                        strSystemURL = "https://live.sagepay.com/gateway/service/authorise.vsp";
                        break;
                    case "cancel":
                        strSystemURL = "https://live.sagepay.com/gateway/service/cancel.vsp";
                        break;
                    case "purchase":
                        strSystemURL = "https://live.sagepay.com/gateway/service/vspserver-register.vsp";
                        break;
                    case "refund":
                        strSystemURL = "https://live.sagepay.com/gateway/service/refund.vsp";
                        break;
                    case "release":
                        strSystemURL = "https://live.sagepay.com/gateway/service/release.vsp";
                        break;
                    case "repeat":
                        strSystemURL = "https://live.sagepay.com/gateway/service/repeat.vsp";
                        break;
                    case "void":
                        strSystemURL = "https://live.sagepay.com/gateway/service/void.vsp";
                        break;
                    case "3dcallback":
                        strSystemURL = "https://live.sagepay.com/gateway/service/direct3dcallback.vsp";
                        break;
                    case "showpost":
                        strSystemURL = "https://live.sagepay.com/showpost/showpost.asp";
                        break;
                    default:
                        break;
                }
            }
            else if (string.Compare(connectTo, "TEST", true) == 0)
            {
                switch (strType)
                {
                    case "abort":
                        strSystemURL = "https://test.sagepay.com/gateway/service/abort.vsp";
                        break;
                    case "authorise":
                        strSystemURL = "https://test.sagepay.com/gateway/service/authorise.vsp";
                        break;
                    case "cancel":
                        strSystemURL = "https://test.sagepay.com/gateway/service/cancel.vsp";
                        break;
                    case "purchase":
                        strSystemURL = "https://test.sagepay.com/gateway/service/vspserver-register.vsp";
                        break;
                    case "refund":
                        strSystemURL = "https://test.sagepay.com/gateway/service/refund.vsp";
                        break;
                    case "release":
                        strSystemURL = "https://test.sagepay.com/gateway/service/release.vsp";
                        break;
                    case "repeat":
                        strSystemURL = "https://test.sagepay.com/gateway/service/repeat.vsp";
                        break;
                    case "void":
                        strSystemURL = "https://test.sagepay.com/gateway/service/void.vsp";
                        break;
                    case "3dcallback":
                        strSystemURL = "https://test.sagepay.com/gateway/service/direct3dcallback.vsp";
                        break;
                    case "showpost":
                        strSystemURL = "https://test.sagepay.com/showpost/showpost.asp";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (strType)
                {
                    case "abort":
                        strSystemURL = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorAbortTx";
                        break;
                    case "authorise":
                        strSystemURL = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorAuthoriseTx";
                        break;
                    case "cancel":
                        strSystemURL = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorCancelTx";
                        break;
                    case "purchase":
                        strSystemURL = "https://test.sagepay.com/simulator/VSPServerGateway.asp?Service=VendorRegisterTx";
                        break;
                    case "refund":
                        strSystemURL = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorRefundTx";
                        break;
                    case "release":
                        strSystemURL = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorReleaseTx";
                        break;
                    case "repeat":
                        strSystemURL = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorRepeatTx";
                        break;
                    case "void":
                        strSystemURL = "https://test.sagepay.com/simulator/vspserverGateway.asp?Service=VendorVoidTx";
                        break;
                    case "3dcallback":
                        strSystemURL = "https://test.sagepay.com/simulator/vspserverCallback.asp";
                        break;
                    case "showpost":
                        strSystemURL = "https://test.sagepay.com/showpost/showpost.asp";
                        break;
                    default:
                        break;
                }
            }
            return strSystemURL;
        }


        public static string GetProtocol()
        {
            return "2.23";
        }

        public static string URLEncode(string strString)
        {
            return HttpUtility.UrlEncode(strString, System.Text.Encoding.GetEncoding("ISO-8859-15"));
        }


        public static string FindField(string field, string strResponse)
        {
            string[] delimiters = new string[1];
            delimiters[0] = Environment.NewLine;
            string[] fieldList = strResponse.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in fieldList)
            {
                if (s.StartsWith(field + "=", StringComparison.CurrentCultureIgnoreCase))
                {
                    return s.Substring(field.Length + 1);
                }
            }
            return "";
        }
    }
}

