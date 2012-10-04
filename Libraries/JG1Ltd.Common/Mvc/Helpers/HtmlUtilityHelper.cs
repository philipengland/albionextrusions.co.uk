using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace JG1Ltd.Common.Mvc.Helpers
{
    public static class HtmlUtilityHelper
    {
        const string HTML_TAG_PATTERN = "<[a-zA-Z0-9/_-]+?((\".*?\")|([^<\"']+?)|('.*?'))*?>";

        public static string StripHtml(this HtmlHelper helper, string inputString)
        {
            return Regex.Replace
              (inputString, HTML_TAG_PATTERN, string.Empty);
        }

        public static string StripPTags(this HtmlHelper helper, string inputString)
        {
            return inputString.Replace("<p>", "").Replace("</p>", "");
        }
    }
}
