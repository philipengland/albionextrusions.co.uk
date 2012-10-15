using System;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Models.Boards;
using Nop.Web.Models.Common;

namespace Nop.Web.Extensions
{
    public static class AlbionPagerHtmlExtension
    {
        public static AlbionPager AlbionPager(this HtmlHelper helper, IPageableModel pagination)
        {
            return new AlbionPager(pagination, helper.ViewContext);
        }
        public static AlbionPager AlbionPager(this HtmlHelper helper, string viewDataKey)
        {
            var dataSource = helper.ViewContext.ViewData.Eval(viewDataKey) as IPageableModel;

            if (dataSource == null)
            {
                throw new InvalidOperationException(string.Format("Item in ViewData with key '{0}' is not an IPagination.",
                                                                  viewDataKey));
            }

            return helper.AlbionPager(dataSource);
        }
    }
}
