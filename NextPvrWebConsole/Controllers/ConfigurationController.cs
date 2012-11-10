using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize]
    public class ConfigurationController : Controller
    {
        //
        // GET: /Configuration/

        public ActionResult Index()
        {
            return View(new Models.Configuration());
        }
        
        [HttpPost]
        public ActionResult UpdateGeneral(Models.ConfigurationModels.GeneralConfiguration ModelGeneral)
        {
            if (!ModelState.IsValid)
            {
                List<string> errors = new List<string>();
                foreach (var t in ModelState.Values)
                    errors.AddRange(t.Errors.Where(x => !String.IsNullOrWhiteSpace(x.ErrorMessage)).Select(x => x.ErrorMessage));
                throw new HttpException((int)HttpStatusCode.BadRequest, String.Join(Environment.NewLine, errors.ToArray()));
            }

            throw new NotImplementedException();
        }

    }
}
