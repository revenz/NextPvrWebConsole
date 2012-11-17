using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace NextPvrWebConsole.Models
{
    [DataContract]
    public class RecordingSchedule
    {
        [DataMember]
        public int Oid { get; set; }

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

        public void Save()
        {
            if (this.Oid > 0)
            {
                // pre existing one
            }
            else
            {
                // new one.
            }
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