using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NextPvrWebConsole.Validators;
using System.ComponentModel.DataAnnotations;

namespace NextPvrWebConsole.Models.ConfigurationModels
{
    public class GeneralSystem
    {
        [Required]
        [WebsiteAddress]
        public string WebsiteAddress { get; set; }
    }
}