using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NextPvrWebConsole;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class RecordingsController : ApiController
    {
        // GET api/recordings
        public IEnumerable<Models.RecordingGroup> Get()
        {
            return Models.RecordingGroup.GetAll(this.GetUser().Oid);
        }

        public IEnumerable<Models.Recording> GetUpcoming()
        {
            return Models.Recording.GetUpcoming(this.GetUser().Oid);
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
        public void Delete(int Oid)
        {
            var recording = NUtility.ScheduledRecording.LoadByOID(Oid);
            if(recording == null)
                throw new Exception("Failed to locate recording");

            NUtility.ScheduleHelperFactory.GetScheduleHelper().DeleteRecording(recording);
            Hubs.NextPvrEventHub.Clients_ShowInfoMessage("Deleted recording: " + recording.Name, "Recording Deleted");
        }
    }
}
