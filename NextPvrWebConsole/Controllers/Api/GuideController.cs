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
        public IEnumerable<dynamic> Get(DateTime Date, string Group)
        {
            var userOid = this.GetUser().Oid;
            var config = new Configuration();
            if (!config.EnableUserSupport)
                userOid = Globals.SHARED_USER_OID;

            // round start to midnight today.
            DateTime start = new DateTime(Date.Year, Date.Month, Date.Day, 0, 0, 0);
            start = TimeZone.CurrentTimeZone.ToUniversalTime(start); // convert to utc            
            var data = Models.Channel.LoadForTimePeriod(userOid, Group, start, start.AddDays(1));
            return data.Select(x => new
            {
                Oid = x.Oid,
                Name = x.Name,
                Number = x.Number,
                Enabled = x.Enabled,
                HasIcon = x.HasIcon,
                Listings = x.Listings.Select(y => new {
                    EndTime = y.EndTime,
                    IsRecording = y.IsRecording,
                    IsRecurring = y.IsRecurring,
                    Oid = y.Oid,
                    RecordingOid = y.RecordingOid,
                    StartTime = y.StartTime,
                    Title = y.Title
                })
            });
        }

        // POST api/quickrecord
        [HttpPost]
        public bool QuickRecord(int Oid)
        {
            return Models.Recording.QuickRecord(this.GetUser().Oid, Oid);
        }

        [HttpPost]
        public NUtility.ScheduledRecording Record(RecordingSchedule RecordingSchedule)        
        {
            return Models.Recording.Record(this.GetUser().Oid, RecordingSchedule);
        }

        [HttpGet]
        public Models.EpgListing EpgListing(int Oid)
        {
            var epgEvent = NUtility.EPGEvent.LoadByOID(Oid);
            var channel = Models.Channel.Load(epgEvent.ChannelOID, this.GetUser().Oid);
            return new Models.EpgListing(epgEvent) { ChannelName = channel.Name, ChannelNumber = channel.Number };
        }
    }
}

