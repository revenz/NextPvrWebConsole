using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize(Roles="System")]
    public class LogsController : ApiController
    {
        // GET api/logs
        public IEnumerable<Models.Log> Get()
        {
            return Models.Log.LoadAll();
        }

        [HttpGet]
        public Models.Log Log([FromUri]string Oid, [FromUri]string Name)
        {
            var log = new Api.LogsController().Get().Where(x => x.Oid == Oid || (Oid == "" && x.Name == Name)).FirstOrDefault();
            if (log != null)
            {
                using (var stream = new System.IO.FileStream(log.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(stream)){
                        log.Content = reader.ReadToEnd().Trim();
                        return log;
                    }
                }
            }
            return null;
        }
    }
}
