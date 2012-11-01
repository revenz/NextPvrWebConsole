using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace NextPvrWebConsole.Models
{
    public class RecordingDirectory
    {
        public string Name { get; set; }
        public string Id { get { return Name; } }
        public string Path { get; set; }
        public bool IsDefault { get; set; }
    }
}