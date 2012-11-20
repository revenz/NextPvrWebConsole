using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;

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
    }
}
