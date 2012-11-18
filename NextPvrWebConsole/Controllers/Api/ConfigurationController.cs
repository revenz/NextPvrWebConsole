using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class ConfigurationController : NextPvrWebConsoleApiController
    {
        // GET api/configuration
        public Models.Configuration Get()
        {
            return new Models.Configuration();
        }
    }
}
