using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize(Roles="Configuration")]
    public class ConfigurationController : NextPvrWebConsoleApiController
    {
        // GET api/configuration
        public Models.Configuration Get()
        {
            return new Models.Configuration();
        }

        [HttpGet]
        public IEnumerable<Models.Device> Devices()
        {
            return Models.Device.LoadAll();
        }

        [HttpGet]
        public bool EmptyEpg()
        {
            Helpers.NpvrCoreHelper.EmptyEpg();
            return true;
        }

        [HttpGet]
        public bool UpdateEpg()
        {
            Helpers.NpvrCoreHelper.UpdateEpg();
            return true;
        }
    }
}
