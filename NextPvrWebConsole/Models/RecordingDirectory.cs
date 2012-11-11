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
        public int Oid { get; set; }
        public int UserOid { get; set; }
        public string Path { get; set; }
        /// <summary>
        /// Gets or sets teh RecordingDirectoryId used by NextPVR to identify the directory, this should be "{Username} - {Name}"
        /// </summary>
        public string RecordingDirectoryId { get; set; }
        [PetaPoco.ResultColumn]
        public string ShortName { get; set; }

        public static List<RecordingDirectory> LoadForUser(int UserOid, bool IncludeShared = false)
        {
            var db = DbHelper.GetDatabase();
            var config = new Configuration();
            if (config.EnableUserSupport)
            {
                return db.Fetch<RecordingDirectory>("select * from recordingdirectory where useroid = @0 or useroid = @1", UserOid, Globals.SHARED_USER_OID);
            }
            else
            {
                return db.Fetch<RecordingDirectory>("select * from recordingdirectory where useroid = @0", Globals.SHARED_USER_OID);
            }
        }

        public static List<RecordingDirectory> LoadAll()
        {
            var db = DbHelper.GetDatabase();
            return db.Fetch<RecordingDirectory>("select rd.*, username || '\' || rd.name as shortname from recordingdirectory rd inner join user u on rd.useroid = u.oid");
        }

        public static RecordingDirectory LoadByShortName(string ShortName)
        {
            var db = DbHelper.GetDatabase();
            return db.FirstOrDefault<RecordingDirectory>("select rd.*, username || '\' || rd.name as shortname from recordingdirectory rd inner join user u on rd.useroid = u.oid where shortname = @0", ShortName);
        }

        public static RecordingDirectory Create(int UserOid, string Name)
        {
            var db = DbHelper.GetDatabase();
            string username = User.GetUsername(UserOid);
            RecordingDirectory directory = new RecordingDirectory() { UserOid = UserOid, Name = Name, RecordingDirectoryId = GetRecordingDirectoryId(username, Name) };
            db.Insert(directory);
            return directory;
        }

        public static string GetRecordingDirectoryId(string Username, string RecordingDirectoryName)
        {
            return "{0} - {1}".FormatStr(Username, RecordingDirectoryName);
        }


        public void Save()
        {
            var db = DbHelper.GetDatabase();
            if (this.Oid == 0)
                db.Insert("recordingdirectory", "oid", true, this);
        }
    }
}