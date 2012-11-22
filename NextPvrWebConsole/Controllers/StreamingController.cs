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
            ViewBag.IsRecording = false;
            return View();
        }

        public ActionResult Recording(int Oid)
        {
            var recording = NUtility.ScheduledRecording.LoadByOID(Oid);
            ViewBag.Title = recording.Name;
            ViewBag.Oid = recording.OID;
            ViewBag.IsRecording = true;
            return View("Index");
        }
    }
}
