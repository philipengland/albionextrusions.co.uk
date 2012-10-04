using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Web.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.UI.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Services.Logging;
namespace Nop.Web.Controllers
{
    public partial class CatalogController : BaseNopController
    {
        [ChildActionOnly]
        public ActionResult HomePageSlides()
        {
          //  ProductDetailsModel
            IList<Product> products = _productService.GetAllProductsDisplayedOnHomePage();

            HomePageSlidesModel model = new HomePageSlidesModel();
            foreach (Product product in products)
            {
                ProductSlideModel productModel = new ProductSlideModel()
                    {
                        Name = product.Name,
                        CategorySeName = product.SeName,
                        ShortDescription = product.ShortDescription,
                    };

                if (product.ProductCategories.Count > 0 && product.ProductPictures.Count > 0)
                {
                    ProductCategory category = product.ProductCategories.FirstOrDefault();
                    productModel.CategoryID = category.CategoryId;
                    productModel.CategorySeName = category.Category.SeName;

                    ProductPicture productPicture = product.ProductPictures.Last();
                    productModel.DefaultPictureModel = new PictureModel()
                    {
                        ImageUrl = _pictureService.GetPictureUrl(productPicture.PictureId, 710),
                        FullSizeImageUrl = _pictureService.GetPictureUrl(productPicture.PictureId),
                        Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), product.Name),
                        AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), product.Name),
                    };

                    model.Products.Add(productModel);
                }
            }

            return PartialView(model);
        }

        [ChildActionOnly]
        //[OutputCache(Duration = 120, VaryByCustom = "WorkingLanguage")]
        public ActionResult SomethingSpecific(int count)
        {
            IList<Category> categories = _categoryService.GetAllCategoriesDisplayedOnHomePage().Take(count).ToList();
            IList<SomethingSpecificModel> somethingSpecifics = new List<SomethingSpecificModel>();
            foreach (Category category in categories)
            {
                SomethingSpecificModel somethingSpecific = new SomethingSpecificModel();
                somethingSpecific.CategoryName = category.Name;

                var productCategories = _categoryService.GetProductCategoriesByCategoryId(category.Id, false);
                IList<Product> products = new List<Product>();
                foreach (ProductCategory productCategory in productCategories)
                {
                    SomethingSpecificProductModel productModel = new SomethingSpecificProductModel();
                    productModel.ProductName = productCategory.Product.Name;
                    productModel.ProductUrl = String.Format("/c/{0}/{1}", productCategory.CategoryId, productCategory.Category.Name);
                    somethingSpecific.Products.Add(productModel);
                }

                somethingSpecifics.Add(somethingSpecific);
            }

            return PartialView(somethingSpecifics);
        }

        [ChildActionOnly]
        //[OutputCache(Duration = 120, VaryByCustom = "WorkingLanguage")]
        public ActionResult HomepageFeaturedCategories(int count)
        {
            var listModel = _categoryService.GetAllCategoriesDisplayedOnHomePage().Take(count)
                .Select(x =>
                {
                    var catModel = x.ToModel();

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured());
                    catModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var pictureModel = new PictureModel()
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(x.PictureId),
                            ImageUrl = _pictureService.GetPictureUrl(x.PictureId, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                        };
                        return pictureModel;
                    });

                    return catModel;
                })
                .ToList();

            return PartialView(listModel);
        }
    }
}
