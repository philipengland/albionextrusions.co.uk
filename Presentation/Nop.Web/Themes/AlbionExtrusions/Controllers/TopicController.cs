using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Localization;
using Nop.Services.Topics;
using Nop.Web.Extensions;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Topics;

namespace Nop.Web.Controllers
{
    public partial class TopicController : BaseNopController
    {
        #region Methods

        [ChildActionOnly]
        //[OutputCache(Duration = 120, VaryByCustom = "WorkingLanguage")]
        public ActionResult TopicText(string systemName)
        {
            var cacheKey = string.Format(ModelCacheEventConsumer.TOPIC_MODEL_KEY, systemName, _workContext.WorkingLanguage.Id);
            var cacheModel = _cacheManager.Get(cacheKey, () => PrepareTopicModel(systemName));

            if (cacheModel == null)
                return Content("");

            return PartialView(cacheModel);
        }

        #endregion
    }
}
