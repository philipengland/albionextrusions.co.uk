using System.Web.Mvc;
using FluentValidation.Attributes;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Validators.Common;

namespace Nop.Web.Models.Common
{
    [Validator(typeof(ContactValidator))]
    public partial class ContactModel : BaseNopModel
    {
        [AllowHtml]
        [NopResourceDisplayName("ContactUs.Email")]
        public string Email { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ContactUs.Enquiry")]
        public string Enquiry { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ContactUs.FirstName")]
        public string FirstName { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ContactUs.Surname")]
        public string Surname { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ContactUs.Telephone")]
        public string Telephone { get; set; }

        [AllowHtml]
        [NopResourceDisplayName("ContactUs.OrderReference")]
        public string OrderReference { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}