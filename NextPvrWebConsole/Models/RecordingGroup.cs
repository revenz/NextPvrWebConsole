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

        public static RecordingGroup[] GetAll(int UserOid)
        {
            // get their recording directories
            Dictionary<string, RecordingDirectory> rds = RecordingDirectory.LoadForUser(UserOid, true).ToDictionary(x => x.Path.ToLower());
            List<NUtility.ScheduledRecording> scheduledRecordings = NUtility.ScheduledRecording.LoadAll(); // get in memory
            Dictionary<int, NUtility.RecurringRecording> recurringRecordings = NUtility.RecurringRecording.LoadAll().ToDictionary(x => x.OID); // need this for future recordings which dont have a filename yet\

            SortedDictionary<string, RecordingGroup> results = new SortedDictionary<string, RecordingGroup>();       

            foreach (var sr in scheduledRecordings)
            {
                try
                {
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

        public static RecordingGroup[] GetAll2(int UserOid)
        {
            Dictionary<int, NUtility.RecurringRecording> recurringRecordings = NUtility.RecurringRecording.LoadAll().ToDictionary(x => x.OID); // get in memory
            List<NUtility.ScheduledRecording> data = NUtility.ScheduledRecording.LoadAll(); // get in memory
            Dictionary<string, Models.RecordingDirectory> recordingDirectories = Models.RecordingDirectory.LoadAll().ToDictionary(x => x.Path.ToLower()); // get in memory
            Dictionary<string, Models.RecordingDirectory> recordingDirectoriesNpvrIdIndex = Models.RecordingDirectory.LoadAll().ToDictionary(x => x.RecordingDirectoryId); // get in memory
            SortedDictionary<string, RecordingGroup> results = new SortedDictionary<string, RecordingGroup>();       

            foreach (var sr in data)
            {
                // get recording folder
                RecordingDirectory recordingDirectory = null;
                if (!String.IsNullOrWhiteSpace(sr.Filename))
                {
                    // get from filename
                    System.IO.DirectoryInfo dir = new System.IO.FileInfo(sr.Filename).Directory.Parent;
                    if (dir != null && recordingDirectories.ContainsKey(dir.FullName.ToLower()))
                    {
                        recordingDirectory = recordingDirectories[dir.FullName.ToLower()];
                    }
                }
                if (recordingDirectory == null && sr.RecurrenceOID > 0 && recurringRecordings.ContainsKey(sr.RecurrenceOID))
                {
                    // try get it from the reoccurence oid
                    if (!String.IsNullOrWhiteSpace(recurringRecordings[sr.RecurrenceOID].RecordingDirectoryID) && recordingDirectoriesNpvrIdIndex.ContainsKey(recurringRecordings[sr.RecurrenceOID].RecordingDirectoryID))
                        recordingDirectory = recordingDirectoriesNpvrIdIndex[recurringRecordings[sr.RecurrenceOID].RecordingDirectoryID];
                }

                // check they have accesss ot this shizzle.
                if (new Configuration().EnableUserSupport)
                {
                    if(recordingDirectory == null)
                        continue; // the end game
                    if (recordingDirectory.UserOid != Globals.SHARED_USER_OID && recordingDirectory.UserOid != UserOid)
                        continue; // not allowed this folder
                }

                // ok add it.

                if (!results.ContainsKey(sr.Name))
                    results.Add(sr.Name, new RecordingGroup(sr.Name));

                throw new Exception("Have to fix this, removed recordingDirectory.ShortName.");
                //results[sr.Name].Recordings.Add(new Recording(sr, UserOid) { RecordingDirectory = recordingDirectory == null ? "" : recordingDirectory.ShortName });
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