using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Validators
{
    public class Validator
    {
        public static bool IsValid(object ItemToValidate, params string[] PropertiesToIgnore)
        {
            System.ComponentModel.DataAnnotations.ValidationContext context = new System.ComponentModel.DataAnnotations.ValidationContext(ItemToValidate, null, null);
            List<ValidationResult> results = new List<ValidationResult>();
            bool valid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(ItemToValidate, context, results, true);
            if (!valid && PropertiesToIgnore.Length > 0)
            {
                valid = true;
                foreach (var r in results)
                {
                    foreach (string memberName in r.MemberNames)
                    {
                        if (PropertiesToIgnore.Contains(memberName))
                            continue;
                        return false;
                    }
                }
            }
            return valid;
        }
    }
}