using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NextPvrWebConsole;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize(Roles = "Recordings")]
    public class RecordingsController : NextPvrWebConsoleApiController
    {
        // GET api/recordings
        public IEnumerable<Models.RecordingGroup> Get()
        {
            return Models.RecordingGroup.Get(this.GetUser().Oid, IncludeAll: true);
        }

        [HttpGet]
        public IEnumerable<Models.RecordingGroup> Available()
        {
            return Models.RecordingGroup.Get(this.GetUser().Oid, IncludeAvailable: true);
        }

        [HttpGet]
        public IEnumerable<Models.Recording> Pending()
        {
            return Models.RecordingGroup.Get(this.GetUser().Oid, IncludePending: true).SelectMany(x => x.Recordings).OrderBy(x => x.StartTime);
        }

        [HttpGet]
        public IEnumerable<Models.RecurringRecording> Recurring()
        {
            return Models.RecurringRecording.LoadAll(this.GetUser().Oid).OrderBy(x => x.Name);
        }

        public IEnumerable<Models.Recording> GetUpcoming()
        {
            return Models.Recording.GetUpcoming(this.GetUser().Oid);
        }

        // GET api/recordings/5
        public NUtility.ScheduledRecording Get(int id)
        {
            return Helpers.NpvrCoreHelper.ScheduledRecordingLoadAll().Where(x => x.EventOID == id).FirstOrDefault();
        }

        [HttpPost]
        public bool UpdateRecurring(Models.RecurringRecording RecurringRecording)
        {
            var user = this.GetUser();
            return RecurringRecording.Save(user.Oid);
        }
        
        // DELETE api/recordings/5
        public bool Delete(int Oid)
        {
            return Models.Recording.DeleteByOid(this.GetUser().Oid,  Oid);
        }

        public bool DeleteRecurring(int Oid)
        {
            return Models.RecurringRecording.DeleteByOid(this.GetUser().Oid, Oid);
        }

        public bool MoveRecordings(string GroupName, string DestinationRecordingDirectoryId)
        {
            // need to iterate through all recordings in group
            // push those into a "moving" table (stored in db, so if app is restarted queue can be restored)
            // a worker thread will then handle the moving progress.
            // before the move, when the recordings are added to the table, it should look for recurrence oids and if found
            // update recurrences to use the new destination recording directory for future recurrences
            return true;
        }
    }
}
