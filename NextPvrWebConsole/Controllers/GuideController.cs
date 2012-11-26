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

        public ActionResult Epg(DateTime? Date, string Group)
        {
            ViewBag.GuideStart = Date.Value;
            var data = new Api.GuideController().Get(Date.Value, Group);
            return PartialView("_EpgGrid", data);
        }
    }
}
