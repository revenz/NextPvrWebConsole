using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    public class DevicesController : ApiController
    {
        // GET api/tuners
        public IEnumerable<Models.Device> Get()
        {
            return Models.Device.GetDevices();
        }

        // GET api/tuners/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/tuners
        public void Post([FromBody]string value)
        {
        }

        // PUT api/tuners/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/tuners/5
        public void Delete(int id)
        {
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
