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
            GeneralModel.EnableUserSupport = config.EnableUserSupport;

            var RecordingModel = new Models.ConfigurationModels.RecordingConfiguration();
            RecordingModel.AvoidDuplicateRecordings = config.AvoidDuplicateRecordings;
            RecordingModel.BlockShutDownWhileRecording = config.BlockShutDownWhileRecording;
            RecordingModel.PostPadding = config.PostPadding;
            RecordingModel.PrePadding = config.PrePadding;
            RecordingModel.RecurringMatch = config.RecurringMatch;
            RecordingModel.RecordingDirectories = Models.RecordingDirectory.LoadForUser(Globals.SHARED_USER_OID);


            var DevicesModel = new Models.ConfigurationModels.DevicesConfiguration();
            DevicesModel.Devices = Models.Device.LoadAll();
            DevicesModel.UseReverseOrderForLiveTv = config.UseReverseOrderForLiveTv;


            ViewBag.GeneralModel = GeneralModel;
            ViewBag.RecordingModel = RecordingModel;
            ViewBag.DevicesModel = DevicesModel;
            ViewBag.ChannelGroups = Models.ChannelGroup.LoadAll(Globals.SHARED_USER_OID, true);
            ViewBag.Channels = Models.Channel.LoadAll(Globals.SHARED_USER_OID, true);
            return View();
        }
        
        [HttpPost]
        public ActionResult UpdateGeneral(Models.ConfigurationModels.GeneralConfiguration ModelGeneral)
        {
            return SaveConfig(ModelGeneral);
        }

        [HttpPost]
        public ActionResult UpdateDevices(Models.ConfigurationModels.DevicesConfiguration ModelDevice)
        {
            if (ModelDevice.Devices != null)
            {
                foreach (var d in ModelDevice.Devices)
                    d.Save();
            }

            return SaveConfig(ModelDevice);
        }

        [HttpPost]
        public ActionResult UpdateRecording(Models.ConfigurationModels.RecordingConfiguration ModelRecording)
        {
            // save recording directories
            if (ModelRecording.RecordingDirectories == null || ModelRecording.RecordingDirectories.Count == 0)
                return Json(new { _error = true, message = "At least one recording directory is required." });
            if (ModelState.IsValid && !Models.RecordingDirectory.SaveForUser(Globals.SHARED_USER_OID, ModelRecording.RecordingDirectories))
                return Json(new { _error = true, message = "Failed to save recording directories." });
            return SaveConfig(ModelRecording);
        }

        [HttpPost]
        public ActionResult UpdateChannelGroups(List<Models.ChannelGroup> ChannelGroups)
        {
            try
            {
                if (Models.ChannelGroup.SaveForUser(Globals.SHARED_USER_OID, ChannelGroups))
                    return Json(new { success = true });
                return Json(new { _error = true, message = "Failed to save channel groups" });
            }
            catch (Exception ex)
            {
                return Json(new { _error = true, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult UpdateChannels(List<Models.Channel> Channels)
        {
            try
            {
                if (Models.Channel.SaveForUser(Globals.SHARED_USER_OID, Channels))
                    return Json(new { success = true });
                return Json(new { _error = true, message = "Failed to save channels" });
            }
            catch (Exception ex)
            {
                return Json(new { _error = true, message = ex.Message });
            }
        }

        private ActionResult SaveConfig(object PartialModel)
        {
            if (!ModelState.IsValid)
            {
                List<string> errors = new List<string>();
                foreach (var t in ModelState.Values)
                    errors.AddRange(t.Errors.Where(x => !String.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage));
                throw new HttpException((int)HttpStatusCode.BadRequest, String.Join(Environment.NewLine, errors.ToArray()));
            }

            var config = new Models.Configuration();
            foreach(var property in PartialModel.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.SetProperty))
            {
                var configProperty = config.GetType().GetProperty(property.Name);
                if(configProperty != null)
                    configProperty.SetValue(config, property.GetValue(PartialModel, null), null);
            }
            config.Save();

            return Json(new { success = true });
        }

    }
}
