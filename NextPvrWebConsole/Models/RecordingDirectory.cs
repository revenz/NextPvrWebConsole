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
        public string Oid { get; set; }
        public int UserOid { get; set; }

        public static List<RecordingDirectory> LoadForUser(int UserOid, bool IncludeEveryones = false)
        {
            var config = new Configuration();
            if (config.EnableUserSupport)
            {
                var db = DbHelper.GetDatabase();
                return db.Fetch<RecordingDirectory>("select * from recordingdirectory where useroid = @0" +(IncludeEveryones ? " or useroid is null" : ""), UserOid);
            }
            else
            {
                // return default recording folder(s). // TODO: Add support for more than one default folder
                return new RecordingDirectory[] { new RecordingDirectory() { Oid = String.Empty, Name = "Default", UserOid = 0 } }.ToList();
            }
        }

        public static RecordingDirectory Create(int UserOid, string Name)
        {
            var db = DbHelper.GetDatabase();
            RecordingDirectory directory = new RecordingDirectory() { UserOid = UserOid, Name = Name };
            db.Insert(directory);
            return directory;
        }

    }
}