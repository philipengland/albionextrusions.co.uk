using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Extensions;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Themes;
using Nop.Web.Framework.UI.Captcha;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;
using System.Text;

namespace Nop.Web.Controllers
{
    public partial class CommonController : BaseNopController
    {
        //contact us page
        [NopHttpsRequirement(SslRequirement.No)]
        public ActionResult Contact()
        {
            var model = new ContactModel()
            {
                Email = _workContext.CurrentCustomer.Email,
                FirstName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.FirstName),
                Surname = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastName),
                Telephone = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage
            };
            return View(model);
        }

        [HttpPost, ActionName("Contact")]
        [CaptchaValidator]
        public ActionResult ContactUsSend(ContactModel model, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
            }

            if (ModelState.IsValid)
            {
                string email = model.Email.Trim();
                string firstname = model.FirstName.Trim();
                string surname = model.Surname.Trim();
                string fullName = string.Format("{0} {1}", firstname, surname);
                string telephone = model.Telephone.Trim();
                string orderRef = model.OrderReference.Trim();
                string subject = string.Format(_localizationService.GetResource("ContactUs.EmailSubject"), _storeInformationSettings.StoreName);

                var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
                if (emailAccount == null)
                    emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();

                string from = null;
                string fromName = null;

                StringBuilder body = new StringBuilder();
                body.Append("<html><head><title>Email Enquiry</title></head><body>");
                body.Append("<b>First Name:</b> " + firstname + "<br />");
                body.Append("<b>Surname:</b> " + surname + " <br />");
                body.Append("<b>Email:</b> " + email + " <br />");
                body.Append("<b>Telephone:</b> " + telephone + " <br />");
                body.Append("<b>Order Reference:</b> " + orderRef + " <br />");
                body.Append("<b>Enquiry:</b><br /> " + model.Enquiry);
             //   string body = Core.Html.HtmlHelper.FormatText(model.Enquiry,
                //required for some SMTP servers
                if (_commonSettings.UseSystemEmailForContactUsForm)
                {
                    from = emailAccount.Email;
                    fromName = emailAccount.DisplayName;
                }
                else
                {
                    from = email;
                    fromName = fullName;
                }
                _queuedEmailService.InsertQueuedEmail(new QueuedEmail()
                {
                    From = from,
                    FromName = fromName,
                    To = emailAccount.Email,
                    ToName = emailAccount.DisplayName,
                    Priority = 5,
                    Subject = subject,
                    Body = body.ToString(),
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id
                });

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("ContactUs.YourEnquiryHasBeenSent");
                return View(model);
            }

            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage;
            return View(model);
        }


    }
}
