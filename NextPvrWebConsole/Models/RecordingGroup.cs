using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUtility;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

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
            Dictionary<string, RecordingDirectory> rds = RecordingDirectory.LoadForUserAsDictionaryIndexedByPath(UserOid, true);
            List<NUtility.ScheduledRecording> scheduledRecordings = Helpers.NpvrCoreHelper.ScheduledRecordingLoadAll(); // get in memory
            Dictionary<int, NUtility.RecurringRecording> recurringRecordings = NUtility.RecurringRecording.LoadAll().ToDictionary(x => x.OID); // need this for future recordings which dont have a filename yet\

            RecordingDirectory systemDefault = RecordingDirectory.LoadSystemDefault();

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
                    if (!String.IsNullOrEmpty(sr.Filename) && !Regex.IsMatch(sr.Filename, @"^\[[^\]]+\]$"))
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
                        if (!String.IsNullOrWhiteSpace(directoryId)) // the directoryid isn't set that means its going into NextPVRs default recording directory (which is a Shared one, so we allow it).
                        {
                            rd = rds.Values.Where(x => x.RecordingDirectoryId == directoryId).FirstOrDefault();
                            if (rd == null)
                                continue;
                        }
                        else
                        {
                            rd = systemDefault;
                            if (rd == null || rd.Path == null || !rds.ContainsKey(rd.Path.EndsWith(@"\") ? rd.Path.Substring(0, rd.Path.Length - 1).ToLower() : rd.Path.ToLower()))
                                continue;
                        }
                    }
                    else if (sr.RecurrenceOID == 0) // one off recording in the future
                    {
                        if (!String.IsNullOrEmpty(sr.Filename)) // if this isnt set it will be saved in the default path
                        {
                            // is set for a recording path, so check the security
                            rd = rds.Values.Where(x => x.RecordingDirectoryId == sr.Filename).FirstOrDefault();
                            if(rd == null)
                                continue; // a once off recording they dont have access to.
                        }
                    }
                    else
                    {
                        continue;
                    }

                    if (!results.ContainsKey(sr.Name))
                        results.Add(sr.Name, new RecordingGroup(sr.Name));

                    results[sr.Name].Recordings.Add(new Recording(sr, UserOid) { RecordingDirectoryId = rd == null ? "" : rd.RecordingDirectoryId });
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            return results.Values.ToArray();
        }

        internal static bool Move(int UserOid, string GroupName, string DestinationRecordingDirectoryId)
        {
            // get the recording group
            var RecordingGroup = Get(UserOid, false, true).Where(x => x.Name == GroupName).FirstOrDefault();
            if(RecordingGroup == null)
                throw new Exception("Recording Group '{0}' not found.".FormatStr(GroupName));

            // make sure they have access ot the destination recording group.
            var directory = RecordingDirectory.LoadForUser(UserOid, true).Where(x => x.RecordingDirectoryId == DestinationRecordingDirectoryId).FirstOrDefault();
            if (directory == null)
                throw new Exception("Failed to locate destination Recording Directory.");

            // need to iterate through all recordings in group
            List<int> recurrenceOids = new List<int>();
            foreach(var recording in RecordingGroup.Recordings)
            {
                // push those into a "moving" table (stored in db, so if app is restarted queue can be restored)
                // a worker thread will then handle the moving progress.
                // lets just try using the inbuilt "Archive" feature...
                Helpers.NpvrCoreHelper.ArchiveRecording(recording.OID, directory.RecordingDirectoryId.Substring(1, directory.RecordingDirectoryId.Length - 2));

                // update recurrences to use the new destination recording directory for future recurrences
                if (recording.RecurrenceOid > 0 && !recurrenceOids.Contains(recording.RecurrenceOid))
                {
                    var recurrence = Helpers.NpvrCoreHelper.RecurringRecordingLoadByOID(recording.RecurrenceOid);
                    if (recurrence != null)
                    {
                        recurrence.RecordingDirectoryID = DestinationRecordingDirectoryId;
                        recurrence.Save();
                    }

                    recurrenceOids.Add(recording.RecurrenceOid);
                }
            }

            return true;
        }
    }

}