using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize]
    public class SystemController : Controller
    {
        public ActionResult Index()
        {
            var config = new Models.Configuration();
            var SmtpModel = new Models.ConfigurationModels.SmtpConfiguration();
            SmtpModel.Username = config.SmtpUsername;
            SmtpModel.Password = !String.IsNullOrWhiteSpace(config.SmtpPassword) ? "********" : "";
            SmtpModel.Port = config.SmtpPort;
            SmtpModel.Server = config.SmtpServer;
            SmtpModel.UseSsl = config.SmtpUseSsl;
            ViewBag.SmtpModel = SmtpModel;
            return View();
        }
    }
}
