using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Models
{
    public class EpgListing
    {
        public string Aspect { get; set; }
        public string Audio { get; set; }
        public int ChannelOid { get; set; }
        public string Description { get; set; }
        public DateTime EndTime { get; set; }
        public int Episode { get; set; }
        public bool FirstRun { get; set; }
        public List<string> Genres { get; set; }
        public int Oid { get; set; }
        public DateTime OriginalAirDate { get; set; }
        public string Rating { get; set; }
        public int Season { get; set; }
        public string StarRating { get; set; }
        public DateTime StartTime { get; set; }
        public string Subtitle { get; set; }
        public string Title { get; set; }

        public bool IsRecording { get; set; }
        public bool IsRecurring { get; set; }

        public EpgListing(NUtility.EPGEvent EpgEvent)
        {
            this.Aspect = EpgEvent.Aspect;
            this.Audio = EpgEvent.Audio;
            this.ChannelOid = EpgEvent.ChannelOID;
            this.Description = EpgEvent.Description;
            this.EndTime = EpgEvent.EndTime;
            this.Episode = EpgEvent.Episode;
            this.FirstRun = EpgEvent.FirstRun;
            this.Genres = EpgEvent.Genres;
            this.Oid = EpgEvent.OID;
            this.OriginalAirDate = EpgEvent.OriginalAirDate;
            this.Rating = EpgEvent.Rating;
            this.Season = EpgEvent.Season;
            this.StarRating = EpgEvent.StarRating;
            this.StartTime = EpgEvent.StartTime;
            this.Subtitle = EpgEvent.Subtitle;
            this.Title = EpgEvent.Title;
        }
    }
}
