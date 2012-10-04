using System.Collections.Generic;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Catalog
{
    public partial class HomePageSlidesModel : BaseNopModel
    {
        public HomePageSlidesModel()
        {
            Products = new List<ProductSlideModel>();
        }

        public IList<ProductSlideModel> Products { get; set; }
    }
}