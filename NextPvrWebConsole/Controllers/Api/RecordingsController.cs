﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public bool SaveRecording(Models.RecordingSchedule Schedule)
        {
            var user = this.GetUser();
            return Schedule.Save(user.Oid);
        }

        //[HttpPost]
        //public bool UpdateRecurring(Models.RecurringRecording RecurringRecording)
        //{
        //    var user = this.GetUser();
        //    return RecurringRecording.Save(user.Oid);
        //}
        
        // DELETE api/recordings/5
        public bool Delete(int Oid)
        {
            return Models.Recording.DeleteByOid(this.GetUser().Oid,  Oid);
        }

        public bool DeleteRecurring(int Oid)
        {
            return Models.RecurringRecording.DeleteByOid(this.GetUser().Oid, Oid);
        }

        [HttpGet]
        public bool MoveRecordings(string GroupName, string DestinationRecordingDirectoryId)
        {
            return Models.RecordingGroup.Move(this.GetUser().Oid, GroupName, DestinationRecordingDirectoryId);
        }
    }
}
