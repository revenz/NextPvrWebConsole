using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Validators
{
    public class WebsiteAddressAttribute : RegularExpressionAttribute
    {
        internal static readonly string _Pattern = @"^http(s)?://([a-zA-Z0-9\-\.]+\.)*[a-zA-Z0-9\-\.]+(:[0-9]+)?/([a-zA-Z0-9\-]+/)*$";

        public WebsiteAddressAttribute()
            : base(_Pattern)
        {
            this.ErrorMessage = "Must be a valid website address.";
        }
    }

    public class WebsiteAddressValidator : DataAnnotationsModelValidator<WebsiteAddressAttribute>
    {
        public WebsiteAddressValidator(ModelMetadata metadata, ControllerContext context, WebsiteAddressAttribute attribute)
            : base(metadata, context, attribute) { }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = "Must be a valid website address.",
                ValidationType = "regex"
            };

            rule.ValidationParameters.Add("pattern", WebsiteAddressAttribute._Pattern);

            return new[] { rule };
        }
    }
}