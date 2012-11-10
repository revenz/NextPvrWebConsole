using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize]
    public class ConfigurationController : Controller
    {
        //
        // GET: /Configuration/

        public ActionResult Index()
        {
            var config = new Models.Configuration();
            var GeneralModel = new Models.ConfigurationModels.GeneralConfiguration();
            GeneralModel.EpgUpdateHour = config.EpgUpdateHour;
            GeneralModel.UpdateDvbEpgDuringLiveTv = config.UpdateDvbEpgDuringLiveTv;
            GeneralModel.LiveTvBufferDirectory = config.LiveTvBufferDirectory;

            ViewBag.GeneralModel = GeneralModel;
            return View();
        }
        
        [HttpPost]
        public ActionResult UpdateGeneral(Models.ConfigurationModels.GeneralConfiguration ModelGeneral)
        {
            if (!ModelState.IsValid)
            {
                List<string> errors = new List<string>();
                foreach (var t in ModelState.Values)
                    errors.AddRange(t.Errors.Where(x => !String.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage));
                throw new HttpException((int)HttpStatusCode.BadRequest, String.Join(Environment.NewLine, errors.ToArray()));
            }

            SaveConfig(ModelGeneral);

            return Json(new { success = true });
        }

        private void SaveConfig(object PartialModel)
        {
            var config = new Models.Configuration();
            foreach(var property in PartialModel.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.SetProperty))
            {
                var configProperty = config.GetType().GetProperty(property.Name);
                if(configProperty != null)
                    configProperty.SetValue(config, property.GetValue(PartialModel, null), null);
            }
            config.Save();
        }

    }
}
