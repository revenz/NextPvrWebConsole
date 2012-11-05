using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public class Configuration
    {
        public int PrePadding { get; set; }
        public int PostPadding { get; set; }
        public RecordingDirectory[] RecordingDirectories { get; set; }

        public Configuration()
        {
            this.PrePadding = NextPvrConfigHelper.PrePadding;
            this.PostPadding = NextPvrConfigHelper.PostPadding;
            this.RecordingDirectories = NextPvrConfigHelper.RecordingDirectories;
        }

        public void Save()
        {
            NextPvrConfigHelper.PrePadding = PrePadding;
            NextPvrConfigHelper.PostPadding = PostPadding;
            NextPvrConfigHelper.RecordingDirectories = RecordingDirectories;
        }
    }
}