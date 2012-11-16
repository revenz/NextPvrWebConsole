using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUtility;

namespace NextPvrWebConsole.Models
{
    public class RecurringRecording
    {
        public string AdvancedRules { get; set; }
        //public Channel Channel { get; set; }
        public string ChannelName { get; set; }
        public int ChannelOid { get; set; }
        public bool ChannelHasIcon { get; set; }
        public DayMask DayMask { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime EndTime { get; set; }
        public string EpgTitle { get; set; }
        public bool IsManualRecording { get; set; }
        public int Keep { get; set; }
        public string MatchRules { get; set; }
        public string Name { get; set; }
        public int Oid { get; set; }
        public bool OnlyNewEpisodes { get; set; }
        public int PostPadding { get; set; }
        public int PrePadding { get; set; }
        public RecordingQuality Quality { get; set; }
        public string RecordingDirectoryId { get; set; }
        public DateTime StartTime { get; set; }
        public bool Timeslot { get; set; }

        public bool Monday { get { return (this.DayMask & NUtility.DayMask.MONDAY) == NUtility.DayMask.MONDAY; } }
        public bool Tuesday { get { return (this.DayMask & NUtility.DayMask.TUESDAY) == NUtility.DayMask.TUESDAY; } }
        public bool Wednesday { get { return (this.DayMask & NUtility.DayMask.WEDNESDAY) == NUtility.DayMask.WEDNESDAY; } }
        public bool Thursday { get { return (this.DayMask & NUtility.DayMask.THURSDAY) == NUtility.DayMask.THURSDAY; } }
        public bool Friday { get { return (this.DayMask & NUtility.DayMask.FRIDAY) == NUtility.DayMask.FRIDAY; } }
        public bool Saturday { get { return (this.DayMask & NUtility.DayMask.SATURDAY) == NUtility.DayMask.SATURDAY; } }
        public bool Sunday { get { return (this.DayMask & NUtility.DayMask.SUNDAY) == NUtility.DayMask.SUNDAY; } }

        public RecurringRecording() { }
        public RecurringRecording(NUtility.RecurringRecording BaseRecurringRecording)
        {
            
            this.AdvancedRules =BaseRecurringRecording.AdvancedRules;
            this.ChannelName =BaseRecurringRecording.ChannelName;
            this.ChannelOid =BaseRecurringRecording.ChannelOID;
            this.DayMask  = BaseRecurringRecording.DayMask;
            this.EndTime =BaseRecurringRecording.EndTime;
            this.Keep =BaseRecurringRecording.Keep;
            this.MatchRules =BaseRecurringRecording.MatchRules;
            this.Name =BaseRecurringRecording.Name;
            this.Oid =BaseRecurringRecording.OID;
            this.OnlyNewEpisodes =BaseRecurringRecording.OnlyNewEpisodes;
            this.PostPadding =BaseRecurringRecording.PostPadding;
            this.PrePadding =BaseRecurringRecording.PrePadding;
            this.Quality = BaseRecurringRecording.Quality;
            this.RecordingDirectoryId =BaseRecurringRecording.RecordingDirectoryID;
            this.StartTime =BaseRecurringRecording.StartTime;
            this.Timeslot = BaseRecurringRecording.Timeslot;

            this.EpgTitle = BaseRecurringRecording.EPGTitle;

            this.ChannelHasIcon = BaseRecurringRecording.ChannelName == "All Channels" || (BaseRecurringRecording.Channel != null && BaseRecurringRecording.Channel.Icon != null);
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
    }
}