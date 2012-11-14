using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class DevicesController : ApiController
    {
        // GET api/tuners
        public IEnumerable<Models.Device> Get()
        {
            return Models.Device.GetDevices();
        }
        
        public bool DeleteStream(int Handle)
        {
            if (!Models.Device.StopStream(Handle))
                return false;

            Hubs.NextPvrEventHub.Clients_ShowInfoMessage("Stopped Live Stream: " + Handle); // TODO: Get pretty info about live stream
            return true;
        }
    }
}
