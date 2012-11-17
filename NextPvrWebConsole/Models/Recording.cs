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

            NUtility.EPGEvent epgevent = NUtility.EPGEvent.LoadByOID(BaseRecording.OID);

            var channel = NUtility.Channel.LoadByOID(BaseRecording.ChannelOID);
            this.ChannelHasIcon = channel != null && channel.Icon != null;

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

        // this code is from NEWA, didnt help.
        private static IScheduleHelper scheduleHelper
        {
            get
            {
                if (ScheduleHelperFactory.GetScheduleHelper() == null)
                {
                    //ScheduleHelperFactory.SetScheduleHelper(RecordingServiceProxy.GetServiceInstance());

                    NShared.RecordingServiceProxy.ForceRemote();
                    ScheduleHelperFactory.SetScheduleHelper(NShared.RecordingServiceProxy.GetInstance());
                }
                IScheduleHelper helper = ScheduleHelperFactory.GetScheduleHelper();
                return helper;
            }
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

            var epgevent = NUtility.EPGEvent.LoadByOID(Oid);

            ScheduleHelperFactory.SetScheduleHelper(NShared.RecordingServiceProxy.GetInstance());
            ScheduledRecording recording = ScheduleHelperFactory.GetScheduleHelper().ScheduleRecording(epgevent, config.PrePadding, config.PostPadding, NUtility.RecordingQuality.QUALITY_DEFAULT, recordingDirectoryId);
            //return scheduleHelper.ScheduleRecording(epgevent, config.PrePadding, config.PostPadding, NUtility.RecordingQuality.QUALITY_DEFAULT, recordingDirectoryId) != null;
            return recording != null;
        }

        public static bool DeleteByOid(int UserOid, int Oid)
        {
            var recording = NUtility.ScheduledRecording.LoadByOID(Oid);
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
                        canDelete = false; // should this even happen???
                    else
                    {
                        var recurrenceDirs = Models.RecordingDirectory.LoadForUserAsDictionaryIndexedByDirectoryId(UserOid, true);
                        var recurrence = NUtility.RecurringRecording.LoadByOID(recording.RecurrenceOID);
                        if (!String.IsNullOrWhiteSpace(recurrence.RecordingDirectoryID) && !recurrenceDirs.ContainsKey(recurrence.RecordingDirectoryID))
                            canDelete = false;
                    }
                }

                if (!canDelete)
                    throw new AccessViolationException();
            }


            var instance = NShared.RecordingServiceProxy.GetInstance();
            instance.DeleteRecording(recording);
            Hubs.NextPvrEventHub.Clients_ShowInfoMessage("Deleted recording: " + recording.Name, "Recording Deleted");

            return true;
        }
    }
}