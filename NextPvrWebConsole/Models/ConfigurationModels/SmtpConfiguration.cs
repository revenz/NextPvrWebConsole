using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models.ConfigurationModels
{
    public class SmtpConfiguration
    {
        [Required]
        public string Server { get; set; }
        [Range(1, 65535)]
        public int Port { get; set; }

        public string Username { get; set; }        
        public string Password { get; set; }
        public bool UseSsl { get; set; }
    }
}