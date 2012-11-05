using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUtility;
using System.Runtime.Serialization;

namespace NextPvrWebConsole.Models
{
    [DataContract]
    public class RecordingGroup
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
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

    [DataContract]
    public class Recording
    {
        NUtility.ScheduledRecording BaseRecording { get; set; }

        #region from ScheduledRecording
        [DataMember]
        public int CaptureSourceOID { get; set; }
        [DataMember]
        public string ChannelName { get; set; }
        [DataMember]
        public int ChannelOID { get; set; }
        [DataMember]
        public DateTime EndTime { get; set; }
        [DataMember]
        public string FailureReason { get; set; }
        [DataMember]
        public string Filename { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int OID { get; set; }
        [DataMember]
        public int PostPadding { get; set; }
        [DataMember]
        public int PrePadding { get; set; }
        [DataMember]
        public DateTime StartTime { get; set; }
        [DataMember]
        public RecordingStatus Status { get; set; }
        #endregion

        #region from EPGEvent
        [DataMember]
        public string Subtitle { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public DateTime OriginalAirDate { get; set; }
        [DataMember]
        public string Quality { get; set; }
        [DataMember]
        public string Rating { get; set; }
        [DataMember]
        public int Season { get; set; }
        [DataMember]
        public int Episode { get; set; }
        [DataMember]
        public List<string> Genres { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Aspect { get; set; }
        [DataMember]
        public string Audio { get; set; }
        [DataMember]
        public string StarRating { get; set; }
        #endregion

        [DataMember]
        public string ChannelIcon { get; set; }
        [DataMember]
        public int ChannelNumber { get; set; }

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

            NUtility.Channel channel = NUtility.Channel.LoadByOID(this.ChannelOID);
            if (channel != null) // can be null if channel is deleted? (i got a null exception here....)
            {
                if (channel.Icon != null)
                    this.ChannelIcon = channel.Icon.ToBase64String();
                this.ChannelNumber = channel.Number;
            }

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

        internal static Recording[] GetUpcoming()
        {
            return NUtility.ScheduledRecording.LoadAll().Where(x => x.Status == RecordingStatus.STATUS_PENDING)
                                                        .OrderBy(x => x.StartTime)
                                                        .Take(5)
                                                        .Select(x => new Recording(x)).ToArray();
        }
    }
}