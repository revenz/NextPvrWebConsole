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
        internal static readonly string _Pattern = @"^(?:(?:[^@,""\[\]\x5c\x00-\x20\x7f-\xff\.]|\x5c(?=[@,""\[\]\x5c\x00-\x20\x7f-\xff]))(?:[^@,""\[\]\x5c\x00-\x20\x7f-\xff\.]|(?<=\x5c)[@,""\[\]\x5c\x00-\x20\x7f-\xff]|\x5c(?=[@,""\[\]\x5c\x00-\x20\x7f-\xff])|\.(?=[^\.])){1,62}(?:[^@,""\[\]\x5c\x00-\x20\x7f-\xff\.]|(?<=\x5c)[@,""\[\]\x5c\x00-\x20\x7f-\xff])|""(?:[^""]|(?<=\x5c)""){1,62}"")@(?:(?:[a-z0-9][a-z0-9-]{1,61}[a-z0-9]\.?)+\.[a-z]{2,6}|\[(?:[0-1]?\d?\d|2[0-4]\d|25[0-5])(?:\.(?:[0-1]?\d?\d|2[0-4]\d|25[0-5])){3}\])$";

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