using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class LogsController : ApiController
    {
        // GET api/logs
        public IEnumerable<Models.Log> Get()
        {
            return Models.Log.LoadAll();
        }

        public string GetContent(string Filename)
        {
            // security, make sure the filename is in the list of log files
            if (Get().Where(x => x.FullName.ToLower() == Filename.ToLower()).Count() < 1)
                return "Not found.";

            var file = new System.IO.FileInfo(Filename);
            if(!file.Exists)
                return "Not found.";
            return System.IO.File.ReadAllText(Filename);
        }
    }
}
