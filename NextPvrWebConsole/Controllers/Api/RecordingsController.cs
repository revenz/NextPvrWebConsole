using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    public class RecordingsController : ApiController
    {
        // GET api/recordings
        public IEnumerable<NUtility.ScheduledRecording> Get()
        {
            List<NUtility.ScheduledRecording> data = NUtility.ScheduledRecording.LoadAll();
#if(DEBUG)
            if (data.Count == 0)
            {
                for (int i = 0; i < 10; i++)
                    data.Add(new NUtility.ScheduledRecording() { ChannelOID = 1, Name = "Recording {0}".FormatStr(i), OID = i, StartTime = DateTime.Now.AddMinutes(-180), EndTime = DateTime.Now.AddMinutes(-120) });
            }
#endif
            return data;
        }

        // GET api/recordings/5
        public NUtility.ScheduledRecording Get(int id)
        {
            return NUtility.ScheduledRecording.LoadAll().Where(x => x.EventOID == id).FirstOrDefault();
        }

        // POST api/recordings
        public void Post([FromBody]string value)
        {
        }

        // PUT api/recordings/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/recordings/5
        public void Delete(int id)
        {
        }
    }
}
