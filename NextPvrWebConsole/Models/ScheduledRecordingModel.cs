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

    }
}