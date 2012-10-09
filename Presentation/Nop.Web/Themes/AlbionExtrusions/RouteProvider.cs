using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Web.CustomRoutes
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("CategoryWithPagination",
                            "category/{categoryId}/{SeName}",
                            new { controller = "Catalog", action = "CategoryWithPagination", SeName = UrlParameter.Optional },
                            new { categoryId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });

            routes.MapRoute("CategoryTop",
               "categories",
               new { controller = "Catalog", action = "CategoryTopLevel" },
               new[] { "Nop.Web.Controllers" });

            routes.MapRoute("AddAlbionProductToCart",
                "addalbionproducttocart/{productId}",
                new { controller = "ShoppingCart", action = "AddAlbionProductToCart" },
                new { productId = @"\d+" },
                new[] { "Nop.Web.Controllers" });
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
