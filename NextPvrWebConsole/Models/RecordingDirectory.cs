using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace NextPvrWebConsole.Models
{
    [PetaPoco.PrimaryKey("Oid")]
    public class RecordingDirectory
    {
        public string Name { get; set; }
        public string Oid { get; set; }
        public int UserOid { get; set; }

        [PetaPoco.Ignore]
        public string FullPath
        {
            get
            {
                return System.IO.Path.Combine(new Configuration().DefaultRecordingDirectoryRoot, User.GetUsername(this.UserOid), Name);
            }
        }

        public static List<RecordingDirectory> LoadForUser(int UserOid, bool IncludeShared = false)
        {
            var config = new Configuration();
            if (config.EnableUserSupport)
            {
                var db = DbHelper.GetDatabase();
                return db.Fetch<RecordingDirectory>("select * from recordingdirectory where useroid = @0" + (IncludeShared ? " or useroid is null" : ""), UserOid);
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