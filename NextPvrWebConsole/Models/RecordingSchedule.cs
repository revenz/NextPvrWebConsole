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


        public enum RecordingType
        {
            Once = 0,
            Season_New_This_Channel = 1,
            Season_All_This_Channel = 2,
            Season_Daily_This_Timeslot = 3,
            Season_Weekly_This_Timeslot = 4,
            Season_Monday_To_Friday_This_Timeslot = 5,
            Season_Weekends_This_Timeslot = 6,
            All_Episodes_All_Channels = 7



        }
    }
}