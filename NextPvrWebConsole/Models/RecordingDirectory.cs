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
        /// <summary>
        /// Gets or sets teh RecordingDirectoryId used by NextPVR to identify the directory, this should be "{Username} - {Name}"
        /// </summary>
        public string RecordingDirectoryId { get; set; }
        [PetaPoco.ResultColumn]
        public string ShortName { get; set; }

        private string _FullPath;
        [PetaPoco.Ignore]
        public string FullPath
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_FullPath))
                {
                    if (!String.IsNullOrWhiteSpace(this.ShortName))
                        _FullPath = System.IO.Path.Combine(new Configuration().DefaultRecordingDirectoryRoot, this.ShortName);
                    else
                        _FullPath = System.IO.Path.Combine(new Configuration().DefaultRecordingDirectoryRoot, User.GetUsername(this.UserOid), Name);
                }
                return _FullPath;
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

    }
}