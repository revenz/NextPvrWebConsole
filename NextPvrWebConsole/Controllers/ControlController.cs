using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    public class ControlController : Controller
    {
        //
        // GET: /Control/

        public ActionResult ChannelSelector()
        {
            return View();
        }

    }
}
