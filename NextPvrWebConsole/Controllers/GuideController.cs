using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NextPvrWebConsole.Models;

namespace NextPvrWebConsole.Controllers
{
    [Authorize(Roles="Guide")]
    public class GuideController : Controller
    {
        //
        // GET: /Guide/

        public ActionResult Index()
        {
            var config = new Models.Configuration();
            var user = this.GetUser();
            ViewBag.UserOid = user.Oid;

            ViewBag.RecordingDirectories = Models.RecordingDirectory.LoadForUser(user.Oid, true);
            ViewBag.Groups = Models.ChannelGroup.LoadAll(user.Oid).Where(x => x.Enabled).ToList();
            ViewBag.PrePadding = config.PrePadding;
            ViewBag.PostPadding = config.PostPadding;

            return View();
        }

        public ActionResult Index2()
        {
            var config = new Models.Configuration();
            var user = this.GetUser();
            ViewBag.UserOid = user.Oid;

            ViewBag.RecordingDirectories = Models.RecordingDirectory.LoadForUser(user.Oid, true);
            ViewBag.Groups = Models.ChannelGroup.LoadAll(user.Oid).Where(x => x.Enabled).ToList();
            ViewBag.PrePadding = config.PrePadding;
            ViewBag.PostPadding = config.PostPadding;

            return View();
        }

        public ActionResult Epg(DateTime? Date, string Group)
        {
            var userOid = this.GetUser().Oid;
            var config = new Configuration();
            if (!config.EnableUserSupport)
                userOid = Globals.SHARED_USER_OID;

            ViewBag.GuideStart = Date.Value;
            // round start to midnight today.
            DateTime start = new DateTime(Date.Value.Year, Date.Value.Month, Date.Value.Day, 0, 0, 0);
            start = TimeZone.CurrentTimeZone.ToUniversalTime(start); // convert to utc            
            var data = Models.Channel.LoadForTimePeriod(userOid, Group, start, start.AddDays(1));

            return PartialView("_EpgGrid", data);
        }
    }
}
