using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Validators
{
    public class EmailAttribute : RegularExpressionAttribute
    {
        internal static readonly string _Pattern = @"^([a-zA-Z0-9]+([\.+_-][a-zA-Z0-9]+)*)@(([a-zA-Z0-9]+((\.|[-]{1,2})[a-zA-Z0-9]+)*)\.[a-zA-Z]{2,6})$";

        public EmailAttribute()
            : base(_Pattern)
        {
            this.ErrorMessage = "Must be a valid email address.";
        }
    }

    public class EmailValidator : DataAnnotationsModelValidator<EmailAttribute>
    {
        public EmailValidator(ModelMetadata metadata, ControllerContext context, EmailAttribute attribute)
            : base(metadata, context, attribute) { }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = "Must be a valid email address.",
                ValidationType = "regex"
            };

            rule.ValidationParameters.Add("pattern", EmailAttribute._Pattern);

            return new[] { rule };
        }
    }
}