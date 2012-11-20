using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize(Roles="Dashboard")]
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            var driveinfo = (dynamic)new Api.SystemController().GetDriveStatistics();
            ViewBag.Total = driveinfo.Total;
            ViewBag.Free = driveinfo.Free;
            ViewBag.Recordings = driveinfo.Recordings;
            ViewBag.Used = driveinfo.Used;
            return View();
        }

    }
}
