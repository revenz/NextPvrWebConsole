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
        public enum DirectoryNameMode
        {
            FullPath,
            ShortStrict
        }

        internal static readonly string _PatternFull = @"^[A-Za-z]:\\([^""*/:?|<>\\.\x00-\x20]([^""*/:?|<>\\\x00-\x1F]*[^""*/:?|<>\\.\x00-\x20])?\\?)*$";
        internal static readonly string _PatternShortStrict = @"^([^\[\]""*/:?|<>\\.\x00-\x20]([^\[\]""*/:?|<>\\\x00-\x1F]*[^\[\]""*/:?|<>\\.\x00-\x20])?)*$";


        public DirectoryNameMode Mode { get; set; }

        public DirectoryAttribute(DirectoryNameMode Mode = DirectoryNameMode.FullPath)
            : base(Mode == DirectoryNameMode.FullPath ? _PatternFull : _PatternShortStrict)
        {
            this.Mode = Mode;
            this.ErrorMessage = "Must be a valid directory.";
        }

        public override bool IsValid(object value)
        {
            if (!base.IsValid(value))
                return false;

            if (this.Mode != DirectoryNameMode.FullPath)
                return true;

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

            if(Attribute.Mode == DirectoryAttribute.DirectoryNameMode.FullPath)
                rule.ValidationParameters.Add("pattern", DirectoryAttribute._PatternFull);
            else if (Attribute.Mode == DirectoryAttribute.DirectoryNameMode.ShortStrict)
                rule.ValidationParameters.Add("pattern", DirectoryAttribute._PatternShortStrict);

            return new[] { rule };
        }
    }
}