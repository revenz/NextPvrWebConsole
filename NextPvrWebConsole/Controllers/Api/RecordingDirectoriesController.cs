using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class RecordingDirectoriesController : ApiController
    {
        // GET api/recordingdirectories
        public IEnumerable<Models.RecordingDirectory> Get()
        {
            return Models.RecordingDirectory.LoadForUser(this.GetUser().Oid);
        }
    }
}
