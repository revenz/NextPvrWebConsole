using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize(Roles="System")]
    public class SystemController : Controller
    {
        private const string DummyPassword = "********";

        public ActionResult Index()
        {
            var config = new Models.Configuration();
            var SmtpModel = new Models.ConfigurationModels.SmtpConfiguration();
            SmtpModel.Username = config.SmtpUsername;
            SmtpModel.Password = !String.IsNullOrWhiteSpace(config.SmtpPassword) ? DummyPassword : "";
            SmtpModel.Port = config.SmtpPort;
            SmtpModel.Server = config.SmtpServer;
            SmtpModel.UseSsl = config.SmtpUseSsl;
            SmtpModel.Sender = config.SmtpSender;
            ViewBag.SmtpModel = SmtpModel;

            var GeneralModel = new Models.ConfigurationModels.GeneralSystem();
            GeneralModel.WebsiteAddress = config.WebsiteAddress;
            ViewBag.GeneralModel = GeneralModel;

            return View();
        }

        public ActionResult UpdateSmtpSettings(Models.ConfigurationModels.SmtpConfiguration Model)
        {
            if (!ModelState.IsValid)
                throw new ArgumentException();

            var config = new Models.Configuration();
            config.SmtpServer = Model.Server;
            config.SmtpPort = Model.Port;
            config.SmtpUsername = Model.Username;
            if(Model.Password != DummyPassword) // only update it if its not the dummy
                config.SmtpPassword = Helpers.Encrypter.Encrypt(Model.Password, Helpers.Encrypter.GetCpuId()); // TODO encrypt this
            config.SmtpUseSsl = Model.UseSsl;
            config.SmtpSender = Model.Sender;
            config.Save();

            return Json(new { success = true });
        }

        public ActionResult UpdateGeneralSettings(Models.ConfigurationModels.GeneralSystem GeneralModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new ArgumentException();

                var config = new Models.Configuration();
                config.WebsiteAddress = GeneralModel.WebsiteAddress;
                config.Save();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { _error = true, message = ex.Message });
            }
        }

        public ActionResult Log(string Oid)
        {
            // security, make sure the filename is in the list of log files
            var log = new Api.LogsController().Get().Where(x => x.Oid == Oid).FirstOrDefault();
            if (log != null)
            {
                using (var stream = new System.IO.FileStream(log.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                        return Content(reader.ReadToEnd());
                }
            }
            return Content("Log file not found.");
        }
    }
}
