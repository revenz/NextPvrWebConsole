using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public class ScheduledRecordingModel
    {
        public static List<NUtility.ScheduledRecording> LoadAll(int UserOid, bool IncludeShared = false)
        {
            // step 1, get all allowed recording directories for the user
            Dictionary<string, string> allRecordingDirectories = NextPvrConfigHelper.GetAllRecordingDirectories(UserOid);
            var recordingDirectories = RecordingDirectory.LoadForUser(UserOid, IncludeShared).Select(x=> x.FullPath).ToArray();
            // step 2, get all allowed reoccuring recordings
            var reoccuring = new List<int>();
            foreach(var r in NUtility.RecurringRecording.LoadAll()) {
                if(String.IsNullOrWhiteSpace(r.RecordingDirectoryID)){
                    if(IncludeShared)
                        reoccuring.Add(r.OID);
                } 
                else if(allRecordingDirectories.ContainsKey(r.RecordingDirectoryID))
                {
                    reoccuring.Add(r.OID);
                }
            }
            // step 3, get a list of all recordings
            List<NUtility.ScheduledRecording> recordings = NUtility.ScheduledRecording.LoadAll();
            // step 4, filter all recordings by allowed directories            
            var results = (from r in recordings
                           where reoccuring.Contains(r.RecurrenceOID)
                                 ||
                                 (!String.IsNullOrEmpty(r.Filename) && recordingDirectories.Contains(new System.IO.FileInfo(r.Filename).DirectoryName))
                           select r
                           ).ToList();
            
            return results;
        }
        
        public static NUtility.ScheduledRecording Record(int EpgEventOid, int? PrePadding = null, int? PostPadding = null, string RecordingDirectoryId = null, int NumberToKeep = 0, NUtility.DayMask Days = NUtility.DayMask.ANY, bool OnlyNewEpisodes = false, bool TimeSlot = true)
        {
            var epgevent = NUtility.EPGEvent.LoadByOID(EpgEventOid);
            if(epgevent == null)
                throw new Exception("EPG Event not found.");

            if (String.IsNullOrWhiteSpace(RecordingDirectoryId)) // if not set, use default directory (which is "null")
                RecordingDirectoryId = null;

            if (PrePadding == null)
                PrePadding = NextPvrConfigHelper.PrePadding;
            if (PostPadding == null)
                PostPadding = NextPvrConfigHelper.PostPadding;

            try
            {
                return NShared.RecordingServiceProxy.GetInstance().ScheduleRecording(epgevent, OnlyNewEpisodes, PrePadding.Value, PostPadding.Value, Days, NumberToKeep, NUtility.RecordingQuality.QUALITY_DEFAULT, TimeSlot, RecordingDirectoryId);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to schedule recording.");
            }
        }

        public static NUtility.ScheduledRecording BasicRecord(string Name, int ChannelOId, DateTime Start, DateTime End)
        {
            return NShared.RecordingServiceProxy.GetInstance().ScheduleRecording(Name, ChannelOId, Start, End, NextPvrConfigHelper.PrePadding, NextPvrConfigHelper.PostPadding, NUtility.RecordingQuality.QUALITY_DEFAULT);
        }
    }
}