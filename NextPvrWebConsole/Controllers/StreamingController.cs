using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize(Roles="Guide")]
    public class StreamingController : Controller
    {
        public ActionResult Index(int Oid)
        {
            var channel = Models.Channel.LoadAll(this.GetUser().Oid).Where(x => x.Oid == Oid).FirstOrDefault();
            if (channel == null)
                return new HttpNotFoundResult();
            ViewBag.ChannelName = channel.Name;
            ViewBag.ChannelOid = channel.Oid;
            return View();
        }
    }
}
