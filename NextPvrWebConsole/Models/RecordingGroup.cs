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
        
        public static RecordingGroup[] Get(int UserOid, bool IncludePending = false, bool IncludeAvailable = false, bool IncludeFailed = false, bool IncludeAll = false)
        {
            // get their recording directories
            Dictionary<string, RecordingDirectory> rds = RecordingDirectory.LoadForUser(UserOid, true).ToDictionary(x => x.Path.EndsWith(@"\") ? x.Path.Substring(0, x.Path.Length - 1).ToLower() : x.Path.ToLower());
            List<NUtility.ScheduledRecording> scheduledRecordings = NUtility.ScheduledRecording.LoadAll(); // get in memory
            Dictionary<int, NUtility.RecurringRecording> recurringRecordings = NUtility.RecurringRecording.LoadAll().ToDictionary(x => x.OID); // need this for future recordings which dont have a filename yet\

            SortedDictionary<string, RecordingGroup> results = new SortedDictionary<string, RecordingGroup>();       

            foreach (var sr in scheduledRecordings)
            {
                try
                {
                    if (!IncludeAll)
                    {
                        if (!IncludeAvailable && (sr.Status == RecordingStatus.STATUS_COMPLETED || sr.Status == RecordingStatus.STATUS_COMPLETED_WITH_ERROR))
                            continue;
                        if (!IncludePending && (sr.Status == RecordingStatus.STATUS_PENDING))
                            continue;
                        if (!IncludeFailed && !String.IsNullOrEmpty(sr.FailureReason))
                            continue;
                    }
                    if (sr.Status == RecordingStatus.STATUS_DELETED)
                        continue; // no point showing deleted...


                    RecordingDirectory rd = null;
                    if (!String.IsNullOrEmpty(sr.Filename))
                    {
                        // recordings are in {Directory}\{Show Name}\{Showname}.{ext} so we need the parent directory of the recordign
                        string path = new System.IO.FileInfo(sr.Filename).Directory.Parent.FullName.ToLower();
                        if (!rds.ContainsKey(path))
                            continue; // they dont have access to this recording
                        rd = rds[path];
                    }
                    else if (sr.RecurrenceOID > 0 && recurringRecordings.ContainsKey(sr.RecurrenceOID))
                    {
                        string directoryId = recurringRecordings[sr.RecurrenceOID].RecordingDirectoryID;
                        rd = rds.Values.Where(x => x.RecordingDirectoryId == directoryId).FirstOrDefault();
                        if (rd == null)
                            continue;
                    }
                    else
                    {
                        continue;
                    }

                    if (!results.ContainsKey(sr.Name))
                        results.Add(sr.Name, new RecordingGroup(sr.Name));

                    results[sr.Name].Recordings.Add(new Recording(sr, UserOid) { RecordingDirectoryId = rd.RecordingDirectoryId });
                }
                catch (Exception)
                {
                    continue;
                }
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
        public bool ChannelHasIcon { get; set; }
        [DataMember]
        public int ChannelNumber { get; set; }
        [DataMember]
        public string RecordingDirectoryId { get; set; }

        public Recording(NUtility.ScheduledRecording BaseRecording, int UserOid)
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

            var channel = NUtility.Channel.LoadByOID(BaseRecording.ChannelOID);
            this.ChannelHasIcon = channel != null && channel.Icon != null;

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

        internal static Recording[] GetUpcoming(int UserOid)
        {
            return NUtility.ScheduledRecording.LoadAll().Where(x => x.Status == RecordingStatus.STATUS_PENDING)
                                                        .OrderBy(x => x.StartTime)
                                                        .Take(5)
                                                        .Select(x => new Recording(x, UserOid)).ToArray();
        }
    }
}