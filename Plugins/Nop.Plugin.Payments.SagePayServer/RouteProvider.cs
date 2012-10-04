using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.SagePayServer
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.SagePayServer.Configure",
                 "Plugins/PaymentSagePayServer/Configure",
                 new { controller = "PaymentSagePayServer", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.SagePayServer.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.SagePayServer.PaymentInfo",
                 "Plugins/PaymentSagePayServer/PaymentInfo",
                 new { controller = "PaymentSagePayServer", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.SagePayServer.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.SagePayServer.PaymentInfoScripts",
                 "Plugins/PaymentSagePayServer/PaymentInfoScripts",
                 new { controller = "PaymentSagePayServer", action = "PaymentInfoScripts" },
                 new[] { "Nop.Plugin.Payments.SagePayServer.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.SagePayServer.NotificationPage",
                 "Plugins/PaymentSagePayServer/NotificationPage",
                 new { controller = "PaymentSagePayServer", action = "NotificationPage" },
                 new[] { "Nop.Plugin.Payments.SagePayServer.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.SagePayServer.ResponsePage",
                 "Plugins/PaymentSagePayServer/ResponsePage",
                 new { controller = "PaymentSagePayServer", action = "ResponsePage" },
                 new[] { "Nop.Plugin.Payments.SagePayServer.Controllers" }
            );


        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
