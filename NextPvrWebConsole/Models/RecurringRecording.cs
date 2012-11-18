using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUtility;

namespace NextPvrWebConsole.Models
{
    public enum RecordingType
    {
        Record_Once = 1,
        Record_Season_New_This_Channel = 2,
        Record_Season_All_This_Channel = 3,
        Record_Season_Daily_This_Timeslot = 4,
        Record_Season_Weekly_This_Timeslot = 5,
        Record_Season_Weekdays_This_Timeslot = 6,
        Record_Season_Weekends_This_Timeslot = 7,
        Record_Season_All_Season_All_Channels = 8
    }

    public class RecurringRecording
    {
        public int Oid { get; set; }
        public string Name { get; set; }

        public string ChannelName { get; set; }
        public int ChannelOid { get; set; }
        public bool ChannelHasIcon { get; set; }

        public RecordingType Type { get; set; }
        public int PostPadding { get; set; }
        public int PrePadding { get; set; }
        public string RecordingDirectoryId { get; set; }
        public int Keep { get; set; }
        
        public DayMask DayMask { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime EndTime { get; set; }

        public string AdvancedRules { get; set; }
        public string EpgTitle { get; set; }
        public bool IsManualRecording { get; set; }
        public string MatchRules { get; set; }
        public bool OnlyNewEpisodes { get; set; }
        public RecordingQuality Quality { get; set; }
        public DateTime StartTime { get; set; }
        public bool Timeslot { get; set; }

        public RecurringRecording() { }
        public RecurringRecording(NUtility.RecurringRecording BaseRecurringRecording)
        {
            this.AdvancedRules = BaseRecurringRecording.AdvancedRules;
            this.ChannelName = BaseRecurringRecording.ChannelName;
            this.ChannelOid = BaseRecurringRecording.ChannelOID;
            this.DayMask = BaseRecurringRecording.DayMask;
            this.EndTime = BaseRecurringRecording.EndTime;
            this.Keep = BaseRecurringRecording.Keep;
            this.MatchRules = BaseRecurringRecording.MatchRules;
            this.Name = BaseRecurringRecording.Name;
            this.Oid = BaseRecurringRecording.OID;
            this.OnlyNewEpisodes = BaseRecurringRecording.OnlyNewEpisodes;
            this.PostPadding = BaseRecurringRecording.PostPadding;
            this.PrePadding = BaseRecurringRecording.PrePadding;
            this.Quality = BaseRecurringRecording.Quality;
            this.RecordingDirectoryId = BaseRecurringRecording.RecordingDirectoryID;
            this.StartTime = BaseRecurringRecording.StartTime;
            this.Timeslot = BaseRecurringRecording.Timeslot;

            this.EpgTitle = BaseRecurringRecording.EPGTitle;

            this.ChannelHasIcon = BaseRecurringRecording.ChannelName == "All Channels" || (BaseRecurringRecording.Channel != null && BaseRecurringRecording.Channel.Icon != null);

            if (OnlyNewEpisodes && !this.Timeslot && DayMask == NUtility.DayMask.ANY && ChannelOid > 0)
                this.Type = RecordingType.Record_Season_New_This_Channel;
            else if (!OnlyNewEpisodes && !this.Timeslot && DayMask == NUtility.DayMask.ANY && ChannelOid > 0)
                this.Type = RecordingType.Record_Season_All_This_Channel;
            else if (this.Timeslot && DayMask == NUtility.DayMask.ANY && ChannelOid > 0)
                this.Type = RecordingType.Record_Season_Daily_This_Timeslot;
            else if (this.Timeslot && ChannelOid > 0 && ((int)this.DayMask & ((int)this.DayMask - 1)) == 0) // make sure only 1 day is set, by checking daymask is a power of 2
                this.Type = RecordingType.Record_Season_Weekly_This_Timeslot;
            else if (this.Timeslot && ChannelOid > 0 && this.DayMask == (NUtility.DayMask.MONDAY | NUtility.DayMask.TUESDAY | NUtility.DayMask.WEDNESDAY | NUtility.DayMask.THURSDAY | NUtility.DayMask.FRIDAY))
                this.Type = RecordingType.Record_Season_Weekdays_This_Timeslot;
            else if (this.Timeslot && ChannelOid > 0 && this.DayMask == (NUtility.DayMask.SATURDAY | NUtility.DayMask.SUNDAY))
                this.Type = RecordingType.Record_Season_Weekends_This_Timeslot;
            else if (!this.Timeslot && this.ChannelOid == 0 && this.DayMask == NUtility.DayMask.ANY && !this.OnlyNewEpisodes)
                this.Type = RecordingType.Record_Season_All_Season_All_Channels;

        }

        public static List<RecurringRecording> LoadAll(int UserOid)
        {
            List<RecurringRecording> results = new List<RecurringRecording>();

            List<NUtility.RecurringRecording> coreRecurringRecordings = NUtility.RecurringRecording.LoadAll();
            RecordingDirectory systemDefault = RecordingDirectory.LoadSystemDefault();

            // Load users recording diretories
            Dictionary<string, RecordingDirectory> rds = RecordingDirectory.LoadForUserAsDictionaryIndexedByDirectoryId(UserOid, true);

            foreach (var crr in coreRecurringRecordings)
            {
                RecordingDirectory recordingDirectory = null;
                if (String.IsNullOrWhiteSpace(crr.RecordingDirectoryID))
                    recordingDirectory = systemDefault;
                else if (rds.ContainsKey(crr.RecordingDirectoryID))
                    recordingDirectory = rds[crr.RecordingDirectoryID];
                else
                    continue;
                results.Add(new RecurringRecording(crr) { RecordingDirectoryId = recordingDirectory.RecordingDirectoryId });
            }


            return results;
        }

        internal bool Save(int UserOid)
        {
            var original = NUtility.RecurringRecording.LoadByOID(this.Oid);
            
            var config = new Models.Configuration();
            if (config.EnableUserSupport)
            {
                var recordingDirectories = RecordingDirectory.LoadForUserAsDictionaryIndexedByDirectoryId(UserOid, true);
                // make sure this user has access to the original 
                if (!String.IsNullOrWhiteSpace(original.RecordingDirectoryID) && !recordingDirectories.ContainsKey(original.RecordingDirectoryID))
                    throw new UnauthorizedAccessException();
            }

            original.PostPadding = this.PostPadding;
            original.PrePadding = this.PrePadding;
            original.RecordingDirectoryID = this.RecordingDirectoryId;
            original.Keep = this.Keep;
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
                        switch (StartTime.DayOfWeek)
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
                case RecordingType.Record_Season_All_Season_All_Channels:
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

        internal static bool DeleteByOid(int UserOid, int Oid)
        {
            var recurrence = NUtility.RecurringRecording.LoadByOID(Oid);
            if (recurrence == null)
                throw new Exception("Failed to locate recurrence.");
            var config = new Configuration();
            if (config.EnableUserSupport && !String.IsNullOrWhiteSpace(recurrence.RecordingDirectoryID)) // if dir is null or empty then its the default shared directory
            {
                var recurrenceDirs = Models.RecordingDirectory.LoadForUserAsDictionaryIndexedByDirectoryId(UserOid, true);
                if (!recurrenceDirs.ContainsKey(recurrence.RecordingDirectoryID))
                    throw new UnauthorizedAccessException();
            }
            var instance = NShared.RecordingServiceProxy.GetInstance();
            instance.CancelRecurring(recurrence.OID);
            return true;
        }

        public static RecordingType GetRecordingType(NUtility.RecurringRecording RecurringRecording)
        {
            // put most specific at top
            if (RecurringRecording.OnlyNewEpisodes == false && RecurringRecording.Timeslot && RecurringRecording.DayMask == (DayMask.MONDAY | DayMask.TUESDAY | DayMask.WEDNESDAY | DayMask.THURSDAY | DayMask.FRIDAY))
                return RecordingType.Record_Season_Weekdays_This_Timeslot;

            if (RecurringRecording.OnlyNewEpisodes == false && RecurringRecording.Timeslot && RecurringRecording.DayMask == (DayMask.SATURDAY | DayMask.SUNDAY))
                return RecordingType.Record_Season_Weekends_This_Timeslot;

            if (RecurringRecording.OnlyNewEpisodes && RecurringRecording.Timeslot && ((((int)RecurringRecording.DayMask) & (((int)RecurringRecording.DayMask) - 1)) == 0))
                return RecordingType.Record_Season_Weekly_This_Timeslot;

            if (RecurringRecording.OnlyNewEpisodes && RecurringRecording.Timeslot == false && RecurringRecording.DayMask == DayMask.ANY && RecurringRecording.ChannelOID == 0)
                return RecordingType.Record_Season_All_Season_All_Channels;

            if (RecurringRecording.OnlyNewEpisodes && RecurringRecording.Timeslot == false && RecurringRecording.DayMask == NUtility.DayMask.ANY)
                return RecordingType.Record_Season_New_This_Channel;
            if (RecurringRecording.OnlyNewEpisodes == false && RecurringRecording.Timeslot == false && RecurringRecording.DayMask == NUtility.DayMask.ANY)
                return RecordingType.Record_Season_All_This_Channel;
            if (RecurringRecording.OnlyNewEpisodes == false && RecurringRecording.Timeslot == true && RecurringRecording.DayMask == NUtility.DayMask.ANY)
                return RecordingType.Record_Season_Daily_This_Timeslot;


            return RecordingType.Record_Once; // unknown assume once


        }
    }
}