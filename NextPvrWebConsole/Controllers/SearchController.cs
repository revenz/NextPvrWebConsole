using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize(Roles = "Guide")]
    public class SearchController : Controller
    {
        public ActionResult Index(string SearchText)
        {
            var config = new Models.Configuration();
            var user = this.GetUser();
            ViewBag.UserOid = user.Oid;
            ViewBag.PrePadding = config.PrePadding;
            ViewBag.PostPadding = config.PostPadding;
            List<Models.SearchResult> results = Models.EpgListing.Search(user.Oid, SearchText);
            return View(results);
        }
    }
}
