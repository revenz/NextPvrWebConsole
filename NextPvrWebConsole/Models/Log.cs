using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace NextPvrWebConsole.Models
{
    public class Log
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime DateCreated { get; set; }
        public long Size { get; set; }

        internal static List<Log> LoadAll()
        {
            List<Log> results = new List<Log>();
            string nextPvrLogDir = NUtility.SettingsHelper.GetInstance().GetDataDirectory();
            nextPvrLogDir = Path.Combine(nextPvrLogDir, "Logs");
            foreach (string dir in new string[] { Globals.WebConsoleLoggingDirectory, nextPvrLogDir })
            {
                var dirInfo = new DirectoryInfo(dir);
                if (!dirInfo.Exists)
                    continue;
                foreach (FileInfo file in dirInfo.GetFiles("*.log"))
                {
                    results.Add(new Log()
                    {
                        DateCreated = file.CreationTime,
                        DateModified = file.LastWriteTime,
                        Size = file.Length,
                        Name = file.Name,
                        FullName = file.FullName
                    });
                }
            }
            return results;
        }
    }
}