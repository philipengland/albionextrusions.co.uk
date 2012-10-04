using System.Collections.Generic;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Catalog
{
    public partial class ProductSlideModel : BaseNopEntityModel
    {
        public ProductSlideModel()
        {
            DefaultPictureModel = new PictureModel();
        }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string CategorySeName { get; set; }
        public int CategoryID { get; set; }

        public PictureModel DefaultPictureModel { get; set; }
        public CategoryModel CategoryModel { get; set; }
    }
}