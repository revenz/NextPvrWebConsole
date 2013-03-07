using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Globalization;

namespace NextPvrWebConsole.Controllers
{
    public class ChannelIconController : Controller
    {
        [OutputCache(Duration = 7 * 24 * 60 * 60)] // cache for a week, duration is seconds
        public ActionResult Index(int Oid)
        {
            if (!String.IsNullOrEmpty(Request.Headers["If-Modified-Since"]))
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                var lastMod = DateTime.ParseExact(Request.Headers["If-Modified-Since"], "r", provider).ToLocalTime();
                if (lastMod < DateTime.Now.AddDays(-7))
                {
                    Response.StatusCode = 304;
                    Response.StatusDescription = "Not Modified";
                    return Content(String.Empty);
                }
            }
            try
            {
                if (Oid == 0)
                {
                    // special case for "All Channels" channel
                    return base.File(Server.MapPath("~/Content/images/icon_channel_all.png"), "image/png");
                }

                using (var image = Models.Channel.LoadIcon(Oid))
                {
                    var data = image.ToIconSizeBytes();
                    return base.File(data, "image/png");
                }
            }
            catch (Exception) { }
            return HttpNotFound("Channel Logo not found.");
        }

    }
}
