using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using NUtility;

namespace NextPvrWebConsole.Models
{
    [DataContract]
    public class RecordingSchedule : NextPvrWebConsoleModel
    {
        [DataMember]
        public int? PrePadding { get; set; }

        [DataMember]
        public int? PostPadding { get; set; }

        [DataMember]
        public string RecordingDirectoryId { get; set; }

        [DataMember]
        public int NumberToKeep { get; set; }

        [DataMember]
        public RecordingType Type { get; set; }

        [DataMember]
        public int RecordingOid { get; set; }

        [DataMember]
        public int RecurrenceOid { get; set; }

        [DataMember]
        public int EpgEventOid { get; set; }

        public bool Save(int UserOid)
        {
            if (this.RecurrenceOid > 0)
            {
                // pre existing one
                return Update(UserOid);
            }
            else if(this.EpgEventOid > 0)
            { 
                // new one.
                return Create(UserOid);
            }
            return false;
        }

        public static RecordingSchedule QuickRecord(int UserOid, int EpgEventOid)
        {
            var config = new Configuration();
            RecordingDirectory rd = null;
            if (config.EnableUserSupport && config.UserRecordingDirectoriesEnabled)
                rd = RecordingDirectory.LoadUserDefault(UserOid);

            if (rd == null)
                rd = RecordingDirectory.LoadSystemDefault();

            var schedule = new Models.RecordingSchedule()
            {
                NumberToKeep = 0,
                EpgEventOid = EpgEventOid,
                PostPadding = config.PostPadding,
                PrePadding = config.PrePadding,
                RecordingDirectoryId = rd == null ? "" : rd.RecordingDirectoryId,
                Type = RecordingType.Record_Once
            };
            if (schedule.Save(UserOid))
                return schedule;
            return null;
        }

        private bool Create(int UserOid)
        {
            var config = new Configuration();
            string recordingDirectoryId = this.RecordingDirectoryId ?? ""; // default
            // make sure they have access to the recording directory
            if (!String.IsNullOrEmpty(recordingDirectoryId))
            {
                if (!RecordingDirectory.LoadForUserAsDictionaryIndexedByDirectoryId(UserOid, true).ContainsKey(recordingDirectoryId))
                    throw new UnauthorizedAccessException();
            }

            var epgevent = Helpers.NpvrCoreHelper.EPGEventLoadByOID(this.EpgEventOid);
            if (epgevent == null)
                throw new Exception("Failed to locate EPG Event to record.");

            var instance = NShared.RecordingServiceProxy.GetInstance();

            int prePadding = (this.PrePadding ?? (int?)config.PrePadding).Value;
            int postPadding = (this.PostPadding ?? (int?)config.PostPadding).Value;

            bool onlyNew = false;
            DayMask dayMask = DayMask.ANY;
            bool timeslot = true;
            switch (this.Type)
            {
                case RecordingType.Record_Once: // special cast, effectively a "Quick Record" but with a couple more options
                    {
                        var result = instance.ScheduleRecording(epgevent, prePadding, postPadding, NUtility.RecordingQuality.QUALITY_DEFAULT, recordingDirectoryId);
                        if (result == null)
                            return false;
                        this.RecordingOid = result.OID;
                        this.RecurrenceOid = result.RecurrenceOID;
                    }
                    return true;
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
                        var result = instance.ScheduleRecording(epgevent.Title, 0 /* all channels */, epgevent.StartTime, epgevent.EndTime, prePadding, postPadding, dayMask, this.NumberToKeep, RecordingQuality.QUALITY_DEFAULT, advancedRules, recordingDirectoryId);
                        if (result == null)
                            return false;
                        this.RecordingOid = result.OID;
                        this.RecurrenceOid = result.RecurrenceOID;
                        return true;
                    }
                default:
                    return false; // unknown type.
            }
            
            var recording = instance.ScheduleRecording(epgevent, onlyNew, prePadding, postPadding, dayMask, this.NumberToKeep, RecordingQuality.QUALITY_DEFAULT, timeslot, recordingDirectoryId);
            if (recording == null)
                return false;
            this.RecordingOid = recording.OID;
            this.RecurrenceOid = recording.RecurrenceOID;
            return true;
        }

        private bool Update(int UserOid)
        {
            var original = NUtility.RecurringRecording.LoadByOID(this.RecurrenceOid);

            var config = new Models.Configuration();
            if (config.EnableUserSupport)
            {
                var recordingDirectories = RecordingDirectory.LoadForUserAsDictionaryIndexedByDirectoryId(UserOid, true);
                // make sure this user has access to the original 
                if (!String.IsNullOrWhiteSpace(original.RecordingDirectoryID) && !recordingDirectories.ContainsKey(original.RecordingDirectoryID))
                    throw new UnauthorizedAccessException();
            }

            original.PrePadding = (this.PrePadding ?? (int?)config.PrePadding).Value;
            original.PostPadding = (this.PostPadding ?? (int?)config.PostPadding).Value;
            original.RecordingDirectoryID = this.RecordingDirectoryId;
            original.Keep = this.NumberToKeep;
            switch (this.Type)
            {
                case RecordingType.Record_Season_New_This_Channel:
                    {
                        original.OnlyNewEpisodes = true;
                        original.Timeslot = false;
                        original.DayMask = NUtility.DayMask.ANY;
                    }
                    break;
                case RecordingType.Record_Season_All_This_Channel:
                    {
                        original.OnlyNewEpisodes = false;
                        original.Timeslot = false;
                        original.DayMask = NUtility.DayMask.ANY;
                    }
                    break;
                case RecordingType.Record_Season_Daily_This_Timeslot:
                    {
                        original.OnlyNewEpisodes = false;
                        original.Timeslot = true;
                        original.DayMask = NUtility.DayMask.ANY;
                    }
                    break;
                case RecordingType.Record_Season_Weekly_This_Timeslot:
                    {
                        original.OnlyNewEpisodes = false;
                        original.Timeslot = true;
                        // need to work out day its on...
                        switch (original.StartDate.DayOfWeek)
                        {
                            case DayOfWeek.Sunday: original.DayMask = NUtility.DayMask.SUNDAY; break;
                            case DayOfWeek.Monday: original.DayMask = NUtility.DayMask.MONDAY; break;
                            case DayOfWeek.Tuesday: original.DayMask = NUtility.DayMask.TUESDAY; break;
                            case DayOfWeek.Wednesday: original.DayMask = NUtility.DayMask.WEDNESDAY; break;
                            case DayOfWeek.Thursday: original.DayMask = NUtility.DayMask.THURSDAY; break;
                            case DayOfWeek.Friday: original.DayMask = NUtility.DayMask.FRIDAY; break;
                            case DayOfWeek.Saturday: original.DayMask = NUtility.DayMask.SATURDAY; break;
                        }
                    }
                    break;
                case RecordingType.Record_Season_Weekdays_This_Timeslot:
                    {
                        original.OnlyNewEpisodes = false;
                        original.Timeslot = true;
                        original.DayMask = NUtility.DayMask.MONDAY | NUtility.DayMask.TUESDAY | NUtility.DayMask.WEDNESDAY | NUtility.DayMask.THURSDAY | NUtility.DayMask.FRIDAY;
                    }
                    break;
                case RecordingType.Record_Season_Weekends_This_Timeslot:
                    {
                        original.OnlyNewEpisodes = false;
                        original.Timeslot = true;
                        original.DayMask = NUtility.DayMask.SATURDAY | NUtility.DayMask.SUNDAY;
                    }
                    break;
                case RecordingType.Record_Season_All_Episodes_All_Channels:
                    {
                        original.OnlyNewEpisodes = false;
                        original.Timeslot = false;
                        original.DayMask = NUtility.DayMask.ANY;
                        original.Channel = null;
                    }
                    break;
            }

            original.Save();
            return true;
        }
        
        public static bool CancelRecording(int Oid)
        {
            var recording = NUtility.ScheduledRecording.LoadByOID(Oid);
            if (recording == null)
                return false;
            NShared.RecordingServiceProxy.GetInstance().CancelRecording(recording);            
            return true;
        }
    }
}