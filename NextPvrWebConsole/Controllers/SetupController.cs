using NextPvrWebConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
#if(DEBUG)
            System.Threading.Thread.Sleep(2000);
#endif
            var config = new Models.Configuration();
            if (!config.FirstRun)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View();

            // is valid

            // load recording directories from NextPVR and import as shared directories.
            #region import recording directories
            string defaultRecordingDirectory = Models.NextPvrConfigHelper.DefaultRecordingDirectory;
            KeyValuePair<string, string>[] extras = Models.NextPvrConfigHelper.ExtraRecordingDirectories;
            

            if (!String.IsNullOrWhiteSpace(defaultRecordingDirectory))
                new Models.RecordingDirectory() { Name = "Default", UserOid = Globals.SHARED_USER_OID, Path = defaultRecordingDirectory, Username = Globals.SHARED_USER_USERNAME, IsDefault = true }.Save();
            foreach (var extra in extras)
            {
                // make sure there aren't any directories from NextPVRWebConsole
                string name = extra.Key;
                if (name.StartsWith("[Shared - "))
                    name = "[" + name.Substring("[Shared - ".Length);
                if (Regex.IsMatch(name, @"^\[[^\]\-]+\-[^\]]+\]$"))
                    continue; // must be a user directory, so skip it
                if (name.StartsWith("[") && name.EndsWith("]"))
                    name = name.Substring(1, name.Length - 2);
                new Models.RecordingDirectory() { Name = name, UserOid = Globals.SHARED_USER_OID, Path = extra.Value, Username = Globals.SHARED_USER_USERNAME }.Save();
            }
            #endregion

            var db = DbHelper.GetDatabase();
            #region import channels and groups
            db.BeginTransaction();
            try
            {
                // insert channels
                var channels = NUtility.Channel.LoadAll().OrderBy(x => x.Number).Select(x => new Models.Channel() { Oid = x.OID, Name = x.Name, Number = x.Number, Enabled = true }).ToArray();
                foreach (var c in channels)
                    db.Insert("channel", "oid", false, c);

                // insert groups
                var groups = NUtility.Channel.GetChannelGroups().Select(x => new Models.ChannelGroup() { Name = x, UserOid = Globals.SHARED_USER_OID }).ToArray();
                for (int i = 0; i < groups.Length; i++)
                {
                    groups[i].OrderOid = i + 1;
                    db.Insert("channelgroup", "oid", true, groups[i]);
                    foreach (int channelOid in NUtility.Channel.LoadForGroup(groups[i].Name).Select(x => x.OID))
                        db.Execute("insert into [channelgroupchannel](channelgroupoid, channeloid) values (@0, @1)", groups[i].Oid, channelOid);
                }

                db.CompleteTransaction();
            }
            catch (Exception ex) { db.AbortTransaction(); throw ex; }
            #endregion

            // create user, do this after importing folders, otherwise this users new folder with have an oid lower than the origial defaults (not a big deal, just prettier this way)
            var user = Models.User.CreateUser(Model.Username, Model.EmailAddress, Model.Password, Globals.USER_ROLE_ALL, true, DateTime.UtcNow);
            if (user == null)
                throw new Exception("Failed to create user: " + Model.Username);
            try
            {
                // this can fail during a unit test
                System.Web.Security.FormsAuthentication.SetAuthCookie(Model.Username, false);
            }
            catch (Exception) { }

            if (HttpContext != null && HttpContext.Request != null)
                config.WebsiteAddress = HttpContext.Request.Url.ToString().Substring(0, HttpContext.Request.Url.ToString().LastIndexOf("/") + 1);

            // turn off first run
            config.FirstRun = false;
            config.Save();
            db.CompleteTransaction();

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
