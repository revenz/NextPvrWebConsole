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
        public bool IsDefault { get; set; }
        [PetaPoco.ResultColumn]
        public string Username { get; set; }
        /// <summary>
        /// Gets or sets teh RecordingDirectoryId used by NextPVR to identify the directory, this should be "{Username} - {Name}"
        /// </summary>
        [PetaPoco.Ignore]
        public string RecordingDirectoryId
        {
            get
            {
                if (String.IsNullOrWhiteSpace(Username))
                    throw new Exception("No username set for recording directory.");
                return GetRecordingDirectoryId(Username, this.Name);
            }
        }
        //[PetaPoco.ResultColumn]
        //public string ShortName { get; set; }

        public static List<RecordingDirectory> LoadForUser(int UserOid, bool IncludeShared = false)
        {
            var db = DbHelper.GetDatabase();
            var config = new Configuration();
            if (config.EnableUserSupport)
            {
                return SetUserPaths(db.Fetch<RecordingDirectory>("select rd.*, username from recordingdirectory rd inner join [user] u on rd.useroid = u.oid where useroid = @0 or useroid = @1", UserOid, Globals.SHARED_USER_OID));

            }
            else
            {
                return db.Fetch<RecordingDirectory>("select rd.*, username from recordingdirectory rd inner join user u on rd.useroid = u.oid where useroid = @0", Globals.SHARED_USER_OID);
            }
        }

        public static List<RecordingDirectory> LoadAll()
        { 
            var db = DbHelper.GetDatabase();
            return SetUserPaths(db.Fetch<RecordingDirectory>("select rd.*, username, username || '\' || rd.name as shortname from recordingdirectory rd inner join user u on rd.useroid = u.oid"));
        }

        private static List<RecordingDirectory> SetUserPaths(List<RecordingDirectory> Directories)
        {
            var config = new Configuration();
            foreach (var dir in Directories)
            {
                if (dir.UserOid != Globals.SHARED_USER_OID)
                    dir.Path = System.IO.Path.Combine(config.DefaultRecordingDirectoryRoot, dir.Username, dir.Name);
            }
            return Directories;
        }

        public static RecordingDirectory LoadByShortName(string ShortName)
        {
            var db = DbHelper.GetDatabase();
            return db.FirstOrDefault<RecordingDirectory>("select rd.*, username, username || '\' || rd.name as shortname from recordingdirectory rd inner join user u on rd.useroid = u.oid where shortname = @0", ShortName);
        }

        public static RecordingDirectory Create(int UserOid, string Name)
        {
            var db = DbHelper.GetDatabase();
            string username = User.GetUsername(UserOid);
            RecordingDirectory directory = new RecordingDirectory() { UserOid = UserOid, Name = Name, Username = username };
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

        /// <summary>
        /// Saves the recording directories for a user, will delete any that aren't listed
        /// </summary>
        /// <param name="UserOid">the OID of the User</param>
        /// <param name="RecordingDirectories">the list of recording directories to save</param>
        internal static bool SaveForUser(int UserOid, List<RecordingDirectory> RecordingDirectories)
        {
            var db = DbHelper.GetDatabase();
            db.BeginTransaction();
            try
            {
                // delete the old
                string rdOids = String.Join(",", (from rd in RecordingDirectories where rd.Oid > 0 select rd.Oid.ToString()).ToArray());
                db.Execute("delete from recordingdirectory where useroid = {0} {1}".FormatStr(UserOid, String.IsNullOrWhiteSpace(rdOids) ? "" : "and oid not in ({0})".FormatStr(rdOids)));

                // save the rest
                foreach (var rd in RecordingDirectories)
                {
                    if (rd.Oid < 1) // new one
                        db.Insert("recordingdirectory", "oid", true, rd);
                    else // update an old one
                        db.Update(rd);
                }

                db.CompleteTransaction();
                return true;
            }
            catch (Exception ex)
            {
                db.AbortTransaction();
                return false;
            }
        }
    }
}