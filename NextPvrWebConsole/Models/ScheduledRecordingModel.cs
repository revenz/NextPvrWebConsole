using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public class ScheduledRecordingModel
    {
        public static List<NUtility.ScheduledRecording> LoadAll()
        {
            return NUtility.ScheduledRecording.LoadAll();
        }
        
        public static NUtility.ScheduledRecording Record(int EpgEventOid, int? PrePadding = null, int? PostPadding = null, string RecordingDirectoryId = null, int NumberToKeep = 0, NUtility.DayMask Days = NUtility.DayMask.ANY, bool OnlyNewEpisodes = false, bool TimeSlot = true)
        {
            var epgevent = NUtility.EPGEvent.LoadByOID(EpgEventOid);
            if(epgevent == null)
                throw new Exception("EPG Event not found.");

            var helper = NUtility.ScheduleHelperFactory.GetScheduleHelper();

            if (String.IsNullOrWhiteSpace(RecordingDirectoryId)) // if not set, use default directory (which is "null")
                RecordingDirectoryId = null;

            if (PrePadding == null)
                PrePadding = NextPvrConfigHelper.PrePadding;
            if (PostPadding == null)
                PostPadding = NextPvrConfigHelper.PostPadding;

            try
            {
                return helper.ScheduleRecording(epgevent, OnlyNewEpisodes, PrePadding.Value, PostPadding.Value, Days, NumberToKeep, NUtility.RecordingQuality.QUALITY_DEFAULT, TimeSlot, RecordingDirectoryId);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to schedule recording.");
            }
        }
    }
}