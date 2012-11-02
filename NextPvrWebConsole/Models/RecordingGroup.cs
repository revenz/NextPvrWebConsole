using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUtility;

namespace NextPvrWebConsole.Models
{
    public class RecordingGroup
    {
        public string Name { get; set; }
        public List<Recording> Recordings { get; set; }

        public RecordingGroup(string Name)
        {
            this.Name = Name;
            this.Recordings = new List<Recording>();
        }

        public static RecordingGroup[] GetAll()
        {
            SortedDictionary<string, RecordingGroup> results = new SortedDictionary<string, RecordingGroup>();
            List<NUtility.ScheduledRecording> data = NUtility.ScheduledRecording.LoadAll();
            foreach (ScheduledRecording sr in data)
            {
                if (!results.ContainsKey(sr.Name))
                    results.Add(sr.Name, new RecordingGroup(sr.Name));
                results[sr.Name].Recordings.Add(new Recording(sr));
            }

            return results.Values.ToArray();
        }
    }

    public class Recording
    {
        NUtility.ScheduledRecording BaseRecording { get; set; }

        #region from ScheduledRecording
        public int CaptureSourceOID { get; set; }
        public string ChannelName { get; set; }
        public int ChannelOID { get; set; }
        public DateTime EndTime { get; set; }
        public string FailureReason { get; set; }
        public string Filename { get; set; }
        public string Name { get; set; }
        public int OID { get; set; }
        public int PostPadding { get; set; }
        public int PrePadding { get; set; }
        public DateTime StartTime { get; set; }
        public RecordingStatus Status { get; set; }

        #endregion

        #region from EPGEvent
        public string Subtitle { get; set; }
        public string Title { get; set; }
        public DateTime OriginalAirDate { get; set; }
        public string Quality { get; set; }
        public string Rating { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public List<string> Genres { get; set; }
        public string Description { get; set; }
        public string Aspect { get; set; }
        public string Audio { get; set; }
        public string StarRating { get; set; }
        #endregion

        public Recording(NUtility.ScheduledRecording BaseRecording)
        {
            this.BaseRecording = BaseRecording;

            this.Name = BaseRecording.Name;
            this.CaptureSourceOID = BaseRecording.CaptureSourceOID;
            this.ChannelName = BaseRecording.ChannelName;
            this.ChannelOID = BaseRecording.ChannelOID;
            this.EndTime = BaseRecording.EndTime;
            this.FailureReason = BaseRecording.FailureReason;
            this.Filename = BaseRecording.Filename;
            this.OID = BaseRecording.OID;
            this.PostPadding = BaseRecording.PostPadding;
            this.PrePadding = BaseRecording.PrePadding;
            this.StartTime = BaseRecording.StartTime;
            this.Status = BaseRecording.Status;

            NUtility.EPGEvent epgevent = NUtility.EPGEvent.LoadByOID(BaseRecording.OID);

            if (epgevent != null)
            {
                this.Subtitle = epgevent.Subtitle;
                this.Title = epgevent.Title;
                this.OriginalAirDate = epgevent.OriginalAirDate;
                this.Quality = epgevent.Quality;
                this.Rating = epgevent.Rating;
                this.Season = epgevent.Season;
                this.Episode = epgevent.Episode;
                this.Genres = epgevent.Genres;
                this.Description = epgevent.Description;
                this.Aspect = epgevent.Aspect;
                this.Audio = epgevent.Audio;
                this.StarRating = epgevent.StarRating;
            }
        }
    }
}