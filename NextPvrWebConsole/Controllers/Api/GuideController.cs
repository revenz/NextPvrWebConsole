using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NextPvrWebConsole.Models;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class GuideController : ApiController
    {
        // GET api/guide
        public IEnumerable<Models.Channel> Get(DateTime Date)
        {
            // round start to midnight today.
            DateTime start = new DateTime(Date.Year, Date.Month, Date.Day, 0, 0, 0);
            start = TimeZone.CurrentTimeZone.ToUniversalTime(start); // convert to utc
            return Models.Channel.LoadForTimePeriod(start, start.AddDays(1));
        }

        // POST api/quickrecord
        [HttpPost]
        public NUtility.ScheduledRecording QuickRecord(int Oid)
        {
            return Models.ScheduledRecordingModel.Record(Oid);
        }

        [HttpPost]
        public NUtility.ScheduledRecording Record(RecordingSchedule Recording)        
        {
            NUtility.DayMask Days = NUtility.DayMask.ANY;
            bool OnlyNewEpisodes = false;
            bool TimeSlot = true;
            return Models.ScheduledRecordingModel.Record(Recording.Oid, Recording.PrePadding, Recording.PostPadding, Recording.RecordingDirectoryId, Recording.NumberToKeep, Days, OnlyNewEpisodes, TimeSlot);
        }
    }
}

