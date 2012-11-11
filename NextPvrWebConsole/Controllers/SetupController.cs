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

        public ActionResult Index()
        {
            var config = new Models.Configuration();
            if (!config.FirstRun)
                return RedirectToAction("Accounts", "Login");

            return View();
        }

    }
}
