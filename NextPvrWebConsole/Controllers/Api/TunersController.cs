using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    public class TunersController : ApiController
    {
        // GET api/tuners
        public IEnumerable<NShared.Visible.CaptureSource> Get()
        {
            //string status = NUtility.ScheduleHelperFactory.GetScheduleHelper().GetServerStatus();
            return NShared.Visible.CaptureSource.LoadAll();
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
    }
}
