using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NextPvrWebConsole.Models;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize(Roles="Guide")]
    public class GuideController : NextPvrWebConsoleApiController
    {
        // GET api/guide
        public IEnumerable<Channel> Get(DateTime Date, string Group)
        {
            var userOid = this.GetUser().Oid;
            var config = new Configuration();
            if (!config.EnableUserSupport)
                userOid = Globals.SHARED_USER_OID;

            // round start to midnight today.
            DateTime start = new DateTime(Date.Year, Date.Month, Date.Day, 0, 0, 0);
            start = TimeZone.CurrentTimeZone.ToUniversalTime(start); // convert to utc            
            return Models.Channel.LoadForTimePeriod(userOid, Group, start, start.AddDays(1));
        }

        // POST api/quickrecord
        [HttpPost]
        public RecordingSchedule QuickRecord(int Oid)
        {
            return RecordingSchedule.QuickRecord(this.GetUser().Oid, Oid);
        }

        //[HttpPost]
        //public NUtility.ScheduledRecording Record(RecordingSchedule RecordingSchedule)        
        //{
        //    return Models.Recording.Record(this.GetUser().Oid, RecordingSchedule);
        //}

        [HttpGet]
        public Models.EpgListing EpgListing(int Oid)
        {
            var user = this.GetUser();
            var epgEvent = NUtility.EPGEvent.LoadByOID(Oid);
            var channel = Models.Channel.Load(epgEvent.ChannelOID, user.Oid);
            var config = new Models.Configuration();

            var eventEpgRecodingData = Models.EpgRecordingData.LoadForEpgEventOid(user.Oid, Oid);
            
            return new Models.EpgListing(epgEvent)
            { 
                ChannelName = channel.Name, 
                ChannelNumber = channel.Number,
                ChannelHasIcon = channel.HasIcon,
                PrePadding = eventEpgRecodingData == null ? config.PrePadding : eventEpgRecodingData.PrePadding,
                PostPadding = eventEpgRecodingData == null ? config.PostPadding : eventEpgRecodingData.PostPadding,
                RecordingDirectoryId = eventEpgRecodingData == null ? user.DefaultRecordingDirectoryDirectoryId : eventEpgRecodingData.RecordingDirectoryId,
                IsRecurring = eventEpgRecodingData != null && eventEpgRecodingData.IsRecurring,
                IsRecording = eventEpgRecodingData != null && eventEpgRecodingData.RecordingOid > 0,
                RecordingOid = eventEpgRecodingData != null ? eventEpgRecodingData.RecordingOid : 0,
                RecurrenceOid = eventEpgRecodingData != null ? eventEpgRecodingData.RecurrenceOid : 0,
                RecordingType = eventEpgRecodingData != null ? eventEpgRecodingData.RecordingType : (RecordingType)0
            };
        }
    }
}

