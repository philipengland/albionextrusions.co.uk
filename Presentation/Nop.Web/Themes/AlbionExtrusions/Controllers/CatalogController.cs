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
            IList<Category> categories = _categoryService.GetAllCategoriesDisplayedOnHomePage().ToList();
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
                    productModel.ProductUrl = String.Format("/category/{0}/{1}", productCategory.CategoryId, productCategory.Category.Name);
                    somethingSpecific.Products.Add(productModel);
                }

                if(somethingSpecific.Products.Any())
                    somethingSpecifics.Add(somethingSpecific);

                if (somethingSpecifics.Count == 6)
                    break;
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

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult CategoryTopLevel()
        {
            var listModel = _categoryService.GetAllCategories()
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


            IList<CategoryModel> categories = new List<CategoryModel>();
            foreach (CategoryModel model in listModel)
            {
                Category category = _categoryService.GetCategoryById(model.Id);
                if (category.ParentCategoryId == 0)
                    categories.Add(model);
            }

            return View(categories);
        }

        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult CategoryWithPagination(int categoryId, CatalogPagingFilteringModel command)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
                return RedirectToRoute("HomePage");

            //Check whether the current user has a "Manage catalog" permission
            //It allows him to preview a category before publishing
            if (!category.Published && !_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return RedirectToRoute("HomePage");

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.LastContinueShoppingPage, _webHelper.GetThisPageUrl(false));

            if (command.PageNumber <= 0) command.PageNumber = 1;

            var model = category.ToModel();




            //sorting
            model.PagingFilteringContext.AllowProductSorting = _catalogSettings.AllowProductSorting;
            if (model.PagingFilteringContext.AllowProductSorting)
            {
                foreach (ProductSortingEnum enumValue in Enum.GetValues(typeof(ProductSortingEnum)))
                {
                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "orderby=" + ((int)enumValue).ToString(), null);

                    var sortValue = enumValue.GetLocalizedEnum(_localizationService, _workContext);
                    model.PagingFilteringContext.AvailableSortOptions.Add(new SelectListItem()
                    {
                        Text = sortValue,
                        Value = sortUrl,
                        Selected = enumValue == (ProductSortingEnum)command.OrderBy
                    });
                }
            }



            //view mode
            model.PagingFilteringContext.AllowProductViewModeChanging = _catalogSettings.AllowProductViewModeChanging;
            var viewMode = !string.IsNullOrEmpty(command.ViewMode)
                ? command.ViewMode
                : _catalogSettings.DefaultViewMode;
            if (model.PagingFilteringContext.AllowProductViewModeChanging)
            {
                var currentPageUrl = _webHelper.GetThisPageUrl(true);
                //grid
                model.PagingFilteringContext.AvailableViewModes.Add(new SelectListItem()
                {
                    Text = _localizationService.GetResource("Categories.ViewMode.Grid"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=grid", null),
                    Selected = viewMode == "grid"
                });
                //list
                model.PagingFilteringContext.AvailableViewModes.Add(new SelectListItem()
                {
                    Text = _localizationService.GetResource("Categories.ViewMode.List"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode=list", null),
                    Selected = viewMode == "list"
                });
            }

            //page size
            model.PagingFilteringContext.AllowCustomersToSelectPageSize = false;
            if (category.AllowCustomersToSelectPageSize && category.PageSizeOptions != null)
            {
                var pageSizes = category.PageSizeOptions.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                    {
                        int temp = 0;

                        if (int.TryParse(pageSizes.FirstOrDefault(), out temp))
                        {
                            if (temp > 0)
                            {
                                command.PageSize = temp;
                            }
                        }
                    }

                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.ModifyQueryString(currentPageUrl, "pagesize={0}", null);
                    sortUrl = _webHelper.RemoveQueryString(sortUrl, "pagenumber");

                    foreach (var pageSize in pageSizes)
                    {
                        int temp = 0;
                        if (!int.TryParse(pageSize, out temp))
                        {
                            continue;
                        }
                        if (temp <= 0)
                        {
                            continue;
                        }

                        model.PagingFilteringContext.PageSizeOptions.Add(new SelectListItem()
                        {
                            Text = pageSize,
                            Value = String.Format(sortUrl, pageSize),
                            Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }

                    if (model.PagingFilteringContext.PageSizeOptions.Any())
                    {
                        model.PagingFilteringContext.PageSizeOptions = model.PagingFilteringContext.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();
                        model.PagingFilteringContext.AllowCustomersToSelectPageSize = true;

                        if (command.PageSize <= 0)
                        {
                            command.PageSize = int.Parse(model.PagingFilteringContext.PageSizeOptions.FirstOrDefault().Text);
                        }
                    }
                }
            }
            else
            {
                //customer is not allowed to select a page size
                command.PageSize = category.PageSize;
            }

            if (command.PageSize <= 0) command.PageSize = category.PageSize;


            //price ranges
            model.PagingFilteringContext.PriceRangeFilter.LoadPriceRangeFilters(category.PriceRanges, _webHelper, _priceFormatter);
            var selectedPriceRange = model.PagingFilteringContext.PriceRangeFilter.GetSelectedPriceRange(_webHelper, category.PriceRanges);
            decimal? minPriceConverted = null;
            decimal? maxPriceConverted = null;
            if (selectedPriceRange != null)
            {
                if (selectedPriceRange.From.HasValue)
                    minPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.From.Value, _workContext.WorkingCurrency);

                if (selectedPriceRange.To.HasValue)
                    maxPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(selectedPriceRange.To.Value, _workContext.WorkingCurrency);
            }





            //category breadcrumb
            model.DisplayCategoryBreadcrumb = _catalogSettings.CategoryBreadcrumbEnabled;
            if (model.DisplayCategoryBreadcrumb)
            {
                foreach (var catBr in GetCategoryBreadCrumb(category))
                {
                    model.CategoryBreadcrumb.Add(new CategoryModel()
                    {
                        Id = catBr.Id,
                        Name = catBr.GetLocalized(x => x.Name),
                        SeName = catBr.GetSeName()
                    });
                }
            }




            //subcategories
            model.SubCategories = _categoryService
                .GetAllCategoriesByParentCategoryId(categoryId)
                .Select(x =>
                {
                    var subCatName = x.GetLocalized(y => y.Name);
                    var subCatModel = new CategoryModel.SubCategoryModel()
                    {
                        Id = x.Id,
                        Name = subCatName,
                        SeName = x.GetSeName(),
                        SubCategoryDescription = x.Description
                    };

                    //prepare picture model
                    int pictureSize = _mediaSettings.CategoryThumbPictureSize;
                    var categoryPictureCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_PICTURE_MODEL_KEY, x.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured());
                    subCatModel.PictureModel = _cacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var pictureModel = new PictureModel()
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(x.PictureId),
                            ImageUrl = _pictureService.GetPictureUrl(x.PictureId, pictureSize),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), subCatName),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), subCatName)
                        };
                        return pictureModel;
                    });

                    return subCatModel;
                })
                .ToList();

            model.PagingFilteringContext.TotalItems = model.SubCategories.Count;

            //      model.SubCategories = model.SubCategories.Skip(command.PageIndex * command.PageSize).Take(command.PageSize).ToList();

            IPagedList<CategoryModel.SubCategoryModel> pagedCategories = new PagedList<CategoryModel.SubCategoryModel>(model.SubCategories, command.PageNumber - 1, command.PageSize);

            model.PagingFilteringContext.LoadPagedList(pagedCategories);

            model.SubCategories = model.SubCategories.Skip(command.PageIndex * command.PageSize).Take(command.PageSize).ToList();

            if (model.SubCategories.Count == 0)
            {
                //featured products
                //Question: should we use '_catalogSettings.ShowProductsFromSubcategories' setting for displaying featured products?
                if (!_catalogSettings.IgnoreFeaturedProducts && _categoryService.GetTotalNumberOfFeaturedProducts(categoryId) > 0)
                {
                    //We use the fast GetTotalNumberOfFeaturedProducts before invoking of the slow SearchProducts
                    //to ensure that we have at least one featured product
                    IList<int> filterableSpecificationAttributeOptionIdsFeatured = null;
                    var featuredProducts = _productService.SearchProducts(category.Id,
                        0, true, null, null, 0, null, false, false,
                        _workContext.WorkingLanguage.Id, null,
                        ProductSortingEnum.Position, 0, int.MaxValue,
                        false, out filterableSpecificationAttributeOptionIdsFeatured);
                    model.FeaturedProducts = PrepareProductOverviewModels(featuredProducts).ToList();
                }


                var categoryIds = new List<int>();
                categoryIds.Add(category.Id);
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    //include subcategories
                    categoryIds.AddRange(GetChildCategoryIds(category.Id));
                }
                //products
                IList<int> alreadyFilteredSpecOptionIds = model.PagingFilteringContext.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper);
                IList<int> filterableSpecificationAttributeOptionIds = null;
                var products = _productService.SearchProducts(categoryIds, 0,
                    _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
                    minPriceConverted, maxPriceConverted,
                    0, string.Empty, false, false, _workContext.WorkingLanguage.Id, alreadyFilteredSpecOptionIds,
                    (ProductSortingEnum)command.OrderBy, command.PageNumber - 1, command.PageSize,
                    true, out filterableSpecificationAttributeOptionIds);
                model.Products = PrepareProductOverviewModels(products).ToList();

                model.PagingFilteringContext.LoadPagedList(products);
                model.PagingFilteringContext.ViewMode = viewMode;

                //specs
                model.PagingFilteringContext.SpecificationFilter.PrepareSpecsFilters(alreadyFilteredSpecOptionIds,
                    filterableSpecificationAttributeOptionIds,
                    _specificationAttributeService, _webHelper, _workContext);
            }

            //template
            var templateCacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_TEMPLATE_MODEL_KEY, category.CategoryTemplateId);
            var templateViewPath = _cacheManager.Get(templateCacheKey, () =>
            {
                var template = _categoryTemplateService.GetCategoryTemplateById(category.CategoryTemplateId);
                if (template == null)
                    template = _categoryTemplateService.GetAllCategoryTemplates().FirstOrDefault();
                return template.ViewPath;
            });

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewCategory", _localizationService.GetResource("ActivityLog.PublicStore.ViewCategory"), category.Name);

            return View(templateViewPath, model);
        }

    }
}
