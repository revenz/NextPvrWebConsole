using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public class DriveUsage
    {
        public string Name { get; set; }
        public float Size { get; set; }
        public float RecordingsSize { get; set; }
        public float Used { get; set; }
        public float FreeSpace { get; set; }
    }
}