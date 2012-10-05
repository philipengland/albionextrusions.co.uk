using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;

namespace Nop.Web.Models.Catalog
{
    public partial class CategoryModel : BaseNopEntityModel
    {
        #region Nested Classes

        public partial class SubCategoryModel : BaseNopEntityModel
        {
            public string SubCategoryDescription { get; set; }
        }

        #endregion
    }
}