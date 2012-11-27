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
            ViewBag.DriveStatistics = new Api.SystemController().GetDriveStatistics();
            return View();
        }

    }
}
