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
            var user = this.GetUser();
            ViewBag.UserOid = user.Oid;
            return View();
        }

    }
}
