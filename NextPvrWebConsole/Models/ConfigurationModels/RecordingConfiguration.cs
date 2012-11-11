using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models.ConfigurationModels
{
    public class RecordingConfiguration
    {
        [Range(0, 89)]
        public int PrePadding { get; set; }
        [Range(0, 89)]
        public int PostPadding { get; set; }

        public bool BlockShutDownWhileRecording { get; set; }
        public RecurringMatchType RecurringMatch { get; set; }

        public bool AvoidDuplicateRecordings { get; set; }

        public List<RecordingDirectory> RecordingDirectories { get; set; }

        public RecordingConfiguration()
        {
            this.RecordingDirectories = new List<RecordingDirectory>();
        }
    }
}