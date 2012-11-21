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
            List<Models.SearchResult> results = Models.EpgListing.Search(this.GetUser().Oid, SearchText);
            return View(results);
        }
    }
}
