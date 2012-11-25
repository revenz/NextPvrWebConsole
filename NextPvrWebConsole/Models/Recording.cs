using NUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace NextPvrWebConsole.Models
{
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
        [DataMember]
        public int RecurrenceOid { get; set; }
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
        [DataMember]
        public int EventOid { get; set; }
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
            this.EventOid = BaseRecording.EventOID;
            this.RecurrenceOid = BaseRecording.RecurrenceOID;

            NUtility.EPGEvent epgevent = Helpers.NpvrCoreHelper.EPGEventLoadByOID(BaseRecording.OID);

            var channel = Helpers.NpvrCoreHelper.ChannelLoadByOID(BaseRecording.ChannelOID);
            if (channel != null)
            {
                this.ChannelHasIcon = channel.Icon != null;
                this.ChannelNumber = channel.Number;
            }

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
            return Helpers.NpvrCoreHelper.ScheduledRecordingLoadAll().Where(x => x.Status == RecordingStatus.STATUS_PENDING)
                                                                     .OrderBy(x => x.StartTime)
                                                                     .Take(5)
                                                                     .Select(x => new Recording(x, UserOid)).ToArray();
        }

        public static bool QuickRecord(int UserOid, int Oid)
        {
            var config = new Configuration();
            string recordingDirectoryId = ""; // default
            if (config.EnableUserSupport)
            {
                var rd = RecordingDirectory.LoadForUser(UserOid, false).OrderBy(x => x.IsDefault).FirstOrDefault();
                if (rd != null)
                    recordingDirectoryId = rd.RecordingDirectoryId;
            }

            var epgevent = Helpers.NpvrCoreHelper.EPGEventLoadByOID(Oid);
            if (epgevent == null)
                throw new Exception("Failed to locate EPG Event to record.");

            var instance = NShared.RecordingServiceProxy.GetInstance();
            ScheduledRecording recording = instance.ScheduleRecording(epgevent, config.PrePadding, config.PostPadding, NUtility.RecordingQuality.QUALITY_DEFAULT, recordingDirectoryId);
            return recording != null;
        }

        public static ScheduledRecording Record(int UserOid, Models.RecordingSchedule Schedule)
        {
            var config = new Configuration();
            string recordingDirectoryId = Schedule.RecordingDirectoryId ?? ""; // default
            // make sure they have access to the recording directory
            if (!String.IsNullOrEmpty(recordingDirectoryId))
            {
                if (!RecordingDirectory.LoadForUserAsDictionaryIndexedByDirectoryId(UserOid, true).ContainsKey(recordingDirectoryId))
                    throw new UnauthorizedAccessException();
            }
            

            var epgevent = Helpers.NpvrCoreHelper.EPGEventLoadByOID(Schedule.Oid);
            if (epgevent == null)
                throw new Exception("Failed to locate EPG Event to record.");

            var instance = NShared.RecordingServiceProxy.GetInstance();

            int prePadding = (Schedule.PrePadding ?? (int?)config.PrePadding).Value;
            int postPadding = (Schedule.PostPadding ?? (int?)config.PostPadding).Value;

            bool onlyNew = false;
            DayMask dayMask = DayMask.ANY;
            bool timeslot = true;
            switch (Schedule.Type)
            {
                case RecordingType.Record_Once: // special cast, effectively a "Quick Record" but with a couple more options
                    return instance.ScheduleRecording(epgevent, prePadding, postPadding, NUtility.RecordingQuality.QUALITY_DEFAULT, recordingDirectoryId);
                case RecordingType.Record_Season_New_This_Channel:
                    onlyNew = true;
                    dayMask = DayMask.ANY;
                    timeslot = false;
                    break;
                case RecordingType.Record_Season_All_This_Channel:
                    onlyNew = false;
                    dayMask = DayMask.ANY;
                    timeslot = false;
                    break;
                case RecordingType.Record_Season_Daily_This_Timeslot:
                    onlyNew = false;
                    dayMask = DayMask.ANY;
                    timeslot = true;
                    break;
                case RecordingType.Record_Season_Weekly_This_Timeslot:
                    onlyNew = false;
                    dayMask = dayMask = (DayMask)(1 << ((int)epgevent.StartTime.ToLocalTime().DayOfWeek));
                    timeslot = true;
                    break;
                case RecordingType.Record_Season_Weekdays_This_Timeslot:
                    onlyNew = false;
                    dayMask = DayMask.MONDAY | DayMask.TUESDAY | DayMask.WEDNESDAY | DayMask.THURSDAY | DayMask.FRIDAY;
                    timeslot = true;
                    break;
                case RecordingType.Record_Season_Weekends_This_Timeslot:
                    onlyNew = false;
                    dayMask = DayMask.SATURDAY | DayMask.SUNDAY;
                    timeslot = true;
                    break;
                case RecordingType.Record_Season_All_Episodes_All_Channels: // another special case
                    {
                        string advancedRules = "title like '" + epgevent.Title.Replace("'", "''") + "%'";
                        if (config.RecurringMatch == RecurringMatchType.Exact)
                            advancedRules = "title like '" + epgevent.Title.Replace("'", "''") + "'";
                        return instance.ScheduleRecording(epgevent.Title, 0 /* all channels */, epgevent.StartTime, epgevent.EndTime, prePadding, postPadding, dayMask, Schedule.NumberToKeep, RecordingQuality.QUALITY_DEFAULT, advancedRules, recordingDirectoryId);
                    }     
            }
            return instance.ScheduleRecording(epgevent, onlyNew, prePadding, postPadding, dayMask, Schedule.NumberToKeep, RecordingQuality.QUALITY_DEFAULT, timeslot, recordingDirectoryId);
        }

        public static bool DeleteByOid(int UserOid, int Oid)
        {
            var recording = Helpers.NpvrCoreHelper.ScheduledRecordingLoadByOID(Oid);
            if (recording == null)
                throw new Exception("Failed to locate recording.");

            var config = new Models.Configuration();
            if (config.EnableUserSupport)
            {
                // check they have access to delete this
                bool canDelete = true;
                var recordingDirectories = Models.RecordingDirectory.LoadForUserAsDictionaryIndexedByDirectoryId(UserOid, true);
                if (!String.IsNullOrWhiteSpace(recording.Filename))
                {
                    string path = new System.IO.FileInfo(recording.Filename).Directory.Parent.FullName.ToLower();
                    if (recordingDirectories.ContainsKey(path))
                        canDelete = false;
                }
                else
                {
                    // check for a recurring instance
                    if (recording.RecurrenceOID == 0)
                        canDelete = true; // this means a future recording that will be in the default directory (i.e. the master shared directory everyone can access)
                    else
                    {
                        var recurrenceDirs = Models.RecordingDirectory.LoadForUserAsDictionaryIndexedByDirectoryId(UserOid, true);
                        var recurrence = NUtility.RecurringRecording.LoadByOID(recording.RecurrenceOID);
                        if (!String.IsNullOrWhiteSpace(recurrence.RecordingDirectoryID) && !recurrenceDirs.ContainsKey(recurrence.RecordingDirectoryID))
                            canDelete = false;
                    }
                }

                if (!canDelete)
                    throw new UnauthorizedAccessException();
            }


            Helpers.NpvrCoreHelper.DeleteRecording(recording);
            Hubs.NextPvrEventHub.Clients_ShowInfoMessage("Deleted recording: " + recording.Name, "Recording Deleted");

            return true;
        }
    }
}