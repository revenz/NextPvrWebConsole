using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize(Roles="UserSettings")]
    public class UserSettingsController : Controller
    {
        //
        // GET: /UserSettings/

        public ActionResult Index()
        {
            var config = new Models.Configuration();
            ViewBag.UserRecordingDirectoriesEnabled = config.UserRecordingDirectoriesEnabled;
            return View();
        }

    }
}
