using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize]
    public class RecordingsController : Controller
    {
        //
        // GET: /Recordings/

        public ActionResult Index()
        {
            return View();
        }

    }
}
