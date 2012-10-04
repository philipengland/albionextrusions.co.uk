using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial class SomethingSpecificModel : BaseNopEntityModel
    {
        public SomethingSpecificModel()
        {
            Products = new List<SomethingSpecificProductModel>();
        }
        public string CategoryName { get; set; }
        public IList<SomethingSpecificProductModel> Products { get; set; }
    }
}