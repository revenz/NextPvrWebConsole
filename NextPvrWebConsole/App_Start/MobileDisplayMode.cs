using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;
using System.Collections.Specialized;

namespace NextPvrWebConsole.App_Start
{
    public class MobileDisplayMode : DefaultDisplayMode
    {
        private readonly string[] _useragenStringPartialIdentifiers = new string[]
        {
            "Android",
            "Mobile",
            "Opera Mobi",
            "Samsung",
            "HTC",
            "Nokia",
            "Ericsson",
            "SonyEricsson",
            "iPhone"
        };

        public MobileDisplayMode()
            : base("Mobile")
        {
            ContextCondition = (context => IsMobile(context.GetOverriddenUserAgent()));
        }

        private bool IsMobile(string useragentString)
        {
            bool isMoblie = _useragenStringPartialIdentifiers.Any(val => useragentString.IndexOf(val, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return isMoblie;
        }
    }
}