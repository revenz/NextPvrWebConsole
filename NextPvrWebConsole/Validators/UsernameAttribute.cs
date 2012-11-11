using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Validators
{
    public class UsernameAttribute : RegularExpressionAttribute
    {
        internal static readonly string _Pattern = @"^[\w\d]+[\w\d \.\-_]+[\w\d]+$"; //allows min length of 3 characters "sub" :)

        public UsernameAttribute()
            : base(_Pattern)
        {
            this.ErrorMessage = "Not a valid username.";
        }
    }

    public class UsernameValidator : DataAnnotationsModelValidator<UsernameAttribute>
    {
        public UsernameValidator(ModelMetadata metadata, ControllerContext context, UsernameAttribute attribute)
            : base(metadata, context, attribute) { }

        public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = "Not a valid username.",
                ValidationType = "regex"
            };

            rule.ValidationParameters.Add("pattern", UsernameAttribute._Pattern);

            return new[] { rule };
        }
    }
}