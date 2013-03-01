using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Runtime.Serialization;

namespace NextPvrWebConsole.Models
{
    [DataContract]
    public class Log
    {
        [DataMember]
        public string Name { get; set; }
        // no data member for this, so not to expose full directory
        public string FullName { get; set; }
        [DataMember]
        public DateTime DateModified { get; set; }
        [DataMember]
        public DateTime DateCreated { get; set; }
        [DataMember]
        public long Size { get; set; }
        [DataMember]
        public string Oid { get; set; }
        [DataMember]
        public string Content { get; set; }

        internal static List<Log> LoadAll()
        {
            List<Log> results = new List<Log>();
            string nextPvrLogDir = NUtility.SettingsHelper.GetInstance().GetDataDirectory();
            nextPvrLogDir = Path.Combine(nextPvrLogDir, "Logs");
            int dirId = 0;
            foreach (string dir in new string[] { Globals.WebConsoleLoggingDirectory, nextPvrLogDir })
            {
                var dirInfo = new DirectoryInfo(dir);
                if (!dirInfo.Exists)
                    continue;
                foreach (FileInfo file in dirInfo.GetFiles("*.log*"))
                {
                    results.Add(new Log()
                    {
                        DateCreated = file.CreationTime,
                        DateModified = file.LastWriteTime,
                        Size = file.Length,
                        Name = file.Name,
                        FullName = file.FullName,
                        Oid = "{0}:{1}".FormatStr(dirId, file.Name).ToLower()
                    });
                }
                dirId++;

            }
            return results.OrderByDescending(x => x.DateModified).ToList();
        }
    }
}