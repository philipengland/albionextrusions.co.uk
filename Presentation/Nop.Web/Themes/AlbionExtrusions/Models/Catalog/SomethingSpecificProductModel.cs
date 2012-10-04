using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial class SomethingSpecificProductModel : BaseNopEntityModel
    {
        public string ProductName { get; set; }
        public string ProductUrl { get; set; }
    }
}