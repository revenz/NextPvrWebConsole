using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize(Roles="Configuration")]
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
            GeneralModel.UserBaseRecordingDirectory = config.UserBaseRecordingDirectory;

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
            try
            {
                // check if user recording base recording directory is a shared recording directory
                if (!String.IsNullOrWhiteSpace(ModelGeneral.UserBaseRecordingDirectory))
                {
                    string basePath = ModelGeneral.UserBaseRecordingDirectory.ToLower();
                    if (basePath.EndsWith(@"\"))
                        basePath = basePath.Substring(0, basePath.Length - 1);
                    var rds = Models.RecordingDirectory.LoadForUser(Globals.SHARED_USER_OID);
                    foreach (var rd in rds)
                    {
                        string rdPath = rd.Path.ToLower();
                        if (rdPath.EndsWith(@"\"))
                            rdPath = rdPath.Substring(0, rdPath.Length - 1);

                        if (basePath.StartsWith(rdPath))
                            throw new Exception("User Recording Directory must not be in a Shared Recording Directory.");
                    }
                }

                return SaveConfig(ModelGeneral);
            }
            catch (Exception ex)
            {
                return Json(new { _error = true, message = ex.Message });
            }
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
            try
            {
                // save recording directories
                if (ModelRecording.RecordingDirectories == null || ModelRecording.RecordingDirectories.Count == 0)
                    throw new Exception("At least one recording directory is required.");

                // check base user recording directory isnt in a shared directory
                var config = new Models.Configuration();
                if (!String.IsNullOrWhiteSpace(config.UserBaseRecordingDirectory))
                {
                    string basePath = config.UserBaseRecordingDirectory.ToLower();
                    if (basePath.EndsWith(@"\"))
                        basePath = basePath.Substring(0, basePath.Length - 1);
                    foreach (var rd in ModelRecording.RecordingDirectories)
                    {
                        string rdPath = rd.Path.ToLower();
                        if (rdPath.EndsWith(@"\"))
                            rdPath = rdPath.Substring(0, rdPath.Length - 1);

                        if (basePath.StartsWith(rdPath))
                            throw new Exception("Cannot crete a Shared Recording Directory that contains the User Recording Directory.");
                    }
                }

                if (ModelState.IsValid && !Models.RecordingDirectory.SaveForUser(Globals.SHARED_USER_OID, ModelRecording.RecordingDirectories))
                    throw new Exception("Failed to save recording directories.");
                return SaveConfig(ModelRecording);
            }
            catch (Exception ex)
            {
                return Json(new { _error = true, message = ex.Message });
            }
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

        public ActionResult Scan(int Oid)
        {
            HttpContext.Response.Write("Scan..." + Environment.NewLine);
            HttpContext.Response.Flush();
            NShared.Visible.CaptureSource cs = Helpers.NpvrCoreHelper.CaptureSourceLoadAll().Where(x => x.OID == Oid).FirstOrDefault();
            if (cs == null)
                return null;

            var paramters = cs.GetChannelScannerParameters().ToDictionary(x => x.Key, x => (object)x.Value);
            foreach (var p in paramters)
            {
                List<object> o = cs.GetChannelScannerParameterOptions(p.Key);
            }
            string reason = null;
            var scannner = cs.GetChannelScanner(paramters, out reason);
            scannner.StartScan(out reason);
            HttpContext.Response.Write("Starting scan..." + Environment.NewLine);
            HttpContext.Response.Flush();

            while (!scannner.IsScanComplete())
            {
                string status = scannner.GetStatusDescription();
                HttpContext.Response.Write("    " + status + Environment.NewLine);
                HttpContext.Response.Flush();
                System.Threading.Thread.Sleep(1000);
            }
            HttpContext.Response.Write("Scan complete.");
            return null;
        }
    }
}
