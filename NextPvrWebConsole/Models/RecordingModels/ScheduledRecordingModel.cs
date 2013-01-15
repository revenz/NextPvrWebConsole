using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public class ScheduledRecordingModel : NextPvrWebConsoleModel
    {
        public static List<NUtility.ScheduledRecording> LoadAll(int UserOid, bool IncludeShared = false)
        {
            // step 1, get all allowed recording directories for the user
            Dictionary<string, string> allRecordingDirectories = NextPvrConfigHelper.GetAllRecordingDirectories(UserOid);
            var recordingDirectories = RecordingDirectory.LoadForUser(UserOid, IncludeShared).Select(x=> x.Path).ToArray();
            // step 2, get all allowed reoccuring recordings
            var reoccuring = new List<int>();
            foreach (var r in NUtility.RecurringRecording.LoadAll())
            {
                if (String.IsNullOrWhiteSpace(r.RecordingDirectoryID))
                {
                    if (IncludeShared)
                        reoccuring.Add(r.OID);
                }
                else if (allRecordingDirectories.ContainsKey(r.RecordingDirectoryID))
                {
                    reoccuring.Add(r.OID);
                }
            }
            // step 3, get a list of all recordings
            List<NUtility.ScheduledRecording> recordings = Helpers.NpvrCoreHelper.ScheduledRecordingLoadAll();
            // step 4, filter all recordings by allowed directories            
            var results = (from r in recordings
                           where reoccuring.Contains(r.RecurrenceOID)
                                 ||
                                 (!String.IsNullOrEmpty(r.Filename) && recordingDirectories.Contains(new System.IO.FileInfo(r.Filename).DirectoryName))
                           select r
                           ).ToList();
            
            return results;
        }
    }
}