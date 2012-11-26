using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class DevicesController : NextPvrWebConsoleApiController
    {
        // GET api/tuners
        public IEnumerable<Models.Device> Get()
        {
            return Models.Device.GetDevices();
        }
        
        [HttpDelete]
        public bool DeleteStream(int Handle)
        {
            Models.Device.StopStream(Handle);
            return true;
        }
    }
}
