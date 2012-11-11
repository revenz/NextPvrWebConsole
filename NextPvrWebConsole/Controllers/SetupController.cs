using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    public class SetupController : Controller
    {
        //
        // GET: /Setup/

        [HttpGet]
        public ActionResult Index()
        {
            var config = new Models.Configuration();
            if (!config.FirstRun)
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public ActionResult Index(Models.SetupModel Model)
        {
            var config = new Models.Configuration();
            if (!config.FirstRun)
                return RedirectToAction("Login", "Account");

            if(!ModelState.IsValid)
                return View();

            // is valid
            
            // load recording directories from NextPVR and import as shared directories.
            string defaultRecordingDirectory = Models.NextPvrConfigHelper.DefaultRecordingDirectory;
            KeyValuePair<string, string>[] extras = Models.NextPvrConfigHelper.ExtraRecordingDirectories;

            if (!String.IsNullOrWhiteSpace(defaultRecordingDirectory))
                new Models.RecordingDirectory() { Name = "Default", UserOid = Globals.SHARED_USER_OID, Path = defaultRecordingDirectory, RecordingDirectoryId = Models.RecordingDirectory.GetRecordingDirectoryId(Globals.SHARED_USER_USERNAME, "Default"), IsDefault = true }.Save();
            foreach (var extra in extras)
                new Models.RecordingDirectory() { Name = extra.Key, UserOid = Globals.SHARED_USER_OID, Path = extra.Value, RecordingDirectoryId = Models.RecordingDirectory.GetRecordingDirectoryId(Globals.SHARED_USER_USERNAME, extra.Key) }.Save();

            // create user, do this after importing folders, otherwise this users new folder with have an oid lower than the origial defaults (not a big deal, just prettier this way)
            var user = Models.User.CreateUser(Model.Username, Model.EmailAddress, Model.Password);
            if (user == null)
                throw new Exception("Failed to create user: " + user.Username);

            WebMatrix.WebData.WebSecurity.Login(Model.Username, Model.Password);

            // turn off first run
            config.FirstRun = false;
            config.Save();

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
