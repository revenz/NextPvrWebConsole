using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize]
    public class GuideController : Controller
    {
        //
        // GET: /Guide/

        public ActionResult Index()
        {
            ViewBag.RecordingDirectories = Models.NextPvrConfigHelper.RecordingDirectories;
            ViewBag.PrePadding = Models.NextPvrConfigHelper.PrePadding;
            ViewBag.PostPadding = Models.NextPvrConfigHelper.PostPadding;
            return View();
        }

    }
}
