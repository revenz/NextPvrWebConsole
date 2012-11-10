using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Validators
{
    public class DirectoryAttribute : RegularExpressionAttribute
    {
        internal static readonly string _Pattern = @"^[A-Za-z]:\\([^""*/:?|<>\\.\x00-\x20]([^""*/:?|<>\\\x00-\x1F]*[^""*/:?|<>\\.\x00-\x20])?\\?)*$";
        
        public DirectoryAttribute()
            : base(_Pattern)
        {
            this.ErrorMessage = "Must be a valid directory.";
        }

        public override bool IsValid(object value)
        {
            if (!base.IsValid(value))
                return false;

            // check the path exists
            bool exists = Directory.Exists(value as string);
            if (exists)
                return true;
            this.ErrorMessage = "Directory '{0}' does not exist".FormatStr(value);
            return false;
        }
    }

    public class DirectoryValidator: DataAnnotationsModelValidator<DirectoryAttribute>
    {
        public DirectoryValidator(ModelMetadata metadata, ControllerContext context, DirectoryAttribute attribute)
        : base(metadata, context, attribute) { }

        public override IEnumerable<ModelClientValidationRule>GetClientValidationRules()
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = "Must be a valid directory.",
                ValidationType = "regex"
            };

            rule.ValidationParameters.Add("pattern", DirectoryAttribute._Pattern);

            return new[] { rule };
        }
    }
}