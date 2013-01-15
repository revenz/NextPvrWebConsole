using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;

namespace NextPvrWebConsole.Models
{
    public class EpgRecordingData : NextPvrWebConsoleModel
    {
        public int Keep { get; set; }
        public int PrePadding { get; set; }
        public int PostPadding { get; set; }
        public string RecordingDirectoryId { get; set; }
        public bool IsRecurring { get; set; }
        public RecordingType RecordingType { get; set; }
        public int RecordingOid { get; set; }
        public int RecurrenceOid { get; set; }

        public static EpgRecordingData LoadForEpgEventOid(int UserOid, int EpgEventOid)
        {
            var allowed = LoadAllowedRecordings(UserOid);
            if (allowed.ContainsKey(EpgEventOid))
                return allowed[EpgEventOid];
            return null;
        }

        public static Dictionary<int, EpgRecordingData> LoadAllowedRecordings(int UserOid)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            int count = 0;
            Logger.Log("AllowedRecordings[{0}]: {1}",  count++, timer.Elapsed);

            // -12 hours from start to make sure we get data that starts earlier than start, but finishes after start            
            var AllowedRecordingDirectoriesIndexedByDirectoryId = Helpers.Cacher.RetrieveOrStore<Dictionary<string, Models.RecordingDirectory>>
                ("AllowedRecordings[" + UserOid +"].AllowedRecordingDirectoriesIndexedByDirectoryId", new TimeSpan(0, 0, 10), delegate { return RecordingDirectory.LoadForUserAsDictionaryIndexedByDirectoryId(UserOid, true); });

            var AllowedRecordingDirectoriesIndexedByPath = Helpers.Cacher.RetrieveOrStore<Dictionary<string, Models.RecordingDirectory>>
                ("AllowedRecordings[" + UserOid + "].AllowedRecordingDirectoriesIndexedByPath", new TimeSpan(0, 0, 10), delegate { return RecordingDirectory.LoadForUserAsDictionaryIndexedByPath(UserOid, true); });

            var Recordings = Helpers.Cacher.RetrieveOrStore<List<NUtility.ScheduledRecording>>("AllowedRecordings[" + UserOid + "].Recordings", new TimeSpan(0, 0, 10), delegate { return Helpers.NpvrCoreHelper.ScheduledRecordingLoadAll(); });

            var RecurringRecordings = Helpers.Cacher.RetrieveOrStore<List<RecurringRecording>>("AllowedRecordings[" + UserOid + "].RecurringRecordings", new TimeSpan(0, 0, 10), delegate { return RecurringRecording.LoadAll(UserOid); });

            Dictionary<int, EpgRecordingData> allowedRecordings = new Dictionary<int, EpgRecordingData>();
            Logger.Log("AllowedRecordings[{0}]: {1}", count++, timer.Elapsed);

            foreach (var r in Recordings)
            {
                EpgRecordingData d = null;
                if (r.RecurrenceOID > 0)
                {
                    var recurrence = NUtility.RecurringRecording.LoadByOID(r.RecurrenceOID);
                    if (recurrence != null) // incase the recurrence was deleted
                    {
                        d = new EpgRecordingData()
                        {
                            Keep = recurrence.Keep,
                            PrePadding = recurrence.PrePadding,
                            PostPadding = recurrence.PostPadding,
                            RecordingDirectoryId = recurrence.RecordingDirectoryID,
                            IsRecurring = true,
                            RecurrenceOid = r.RecurrenceOID,
                            RecordingType = RecurringRecording.GetRecordingType(recurrence),
                            RecordingOid = r.OID
                        };
                    };
                }

                if (d == null)
                {
                    d = new EpgRecordingData()
                    {
                        Keep = 0, // once off recording,
                        PrePadding = r.PrePadding,
                        PostPadding = r.PostPadding,
                        IsRecurring = false,
                        RecordingDirectoryId = r.Filename,
                        RecordingType = RecordingType.Record_Once,
                        RecordingOid = r.OID
                    };
                }
                if (d == null || (!String.IsNullOrWhiteSpace(d.RecordingDirectoryId) && !AllowedRecordingDirectoriesIndexedByDirectoryId.ContainsKey(d.RecordingDirectoryId)))
                {
                    // check to see if recording and has fullname in directoryid
                    try
                    {
                        System.IO.FileInfo fi = new System.IO.FileInfo(r.Filename);
                        if (!AllowedRecordingDirectoriesIndexedByPath.ContainsKey(fi.Directory.Parent.FullName.ToLower()))
                            continue; // not allowed for the current user
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                if (!allowedRecordings.ContainsKey(r.EventOID))
                    allowedRecordings.Add(r.EventOID, d);
            }
            Logger.Log("AllowedRecordings[{0}]: {1}", count++, timer.Elapsed);
            return allowedRecordings;
        }
    }
}