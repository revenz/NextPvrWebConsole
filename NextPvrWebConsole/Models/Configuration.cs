using NextPvrWebConsole.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public enum RecurringMatchType
    {
        Exact = 0, /* exact match */
        Start = 1 /* match start of title */
    }
    public class Configuration
    {
        /// <summary>
        /// Gets or sets if this is the first run of the Webconsole, used when display the initial configuration page
        /// </summary>
        public bool FirstRun { get; set; }

        public string DefaultRecordingDirectoryRoot { get; set; }
        public bool EnableUserSupport { get; set; }

        #region general
        [Range(0, 23)]
        public int EpgUpdateHour { get; set; }
        public bool UpdateDvbEpgDuringLiveTv { get; set; }
        [Required]
        [Directory]
        public string LiveTvBufferDirectory { get; set; }
        #endregion

        #region recordings
        public int PrePadding { get; set; }
        public int PostPadding { get; set; }
        public bool BlockShutDownWhileRecording { get; set; }
        public RecurringMatchType RecurringMatch { get; set; }
        public bool AvoidDuplicateRecordings { get; set; }
        #endregion

        public Configuration()
        {
            #region defaults
            this.FirstRun = true;

            this.EnableUserSupport = true;
            this.DefaultRecordingDirectoryRoot = NextPvrConfigHelper.DefaultRecordingDirectory;

            #region general
            this.EpgUpdateHour = NextPvrConfigHelper.EpgUpdateHour;
            this.UpdateDvbEpgDuringLiveTv = NextPvrConfigHelper.UpdateDvbEpgDuringLiveTv;
            this.LiveTvBufferDirectory = NextPvrConfigHelper.LiveTvBufferDirectory;
            #endregion

            #region recordings
            this.PrePadding = NextPvrConfigHelper.PrePadding;
            this.PostPadding = NextPvrConfigHelper.PostPadding;
            this.BlockShutDownWhileRecording = NextPvrConfigHelper.BlockShutDownWhileRecording;
            this.RecurringMatch = NextPvrConfigHelper.RecurringMatch;
            this.AvoidDuplicateRecordings = NextPvrConfigHelper.AvoidDuplicateRecordings;
            #endregion
            #endregion

            var db = DbHelper.GetDatabase();
            var type = this.GetType();
            bool found = false;
            foreach (var d in db.Fetch<dynamic>("select * from setting"))
            {
                found = true;
                string name = d.name as string;
                var prop = type.GetProperty(name);
                if (prop == null)
                    continue;
                var proptype = prop.GetType();
                if (proptype == typeof(int))
                    prop.SetValue(this, d.intvalue, null);
                else if(proptype == typeof(string))
                    prop.SetValue(this, d.stringvalue, null);
                else if (proptype == typeof(double))
                    prop.SetValue(this, d.doublevalue, null);
                else if (proptype == typeof(bool))
                    prop.SetValue(this, d.boolvalue, null);
                else if (proptype == typeof(DateTime))
                    prop.SetValue(this, d.datetimevalue, null);
            }
            if(!found)
                Save();
        }

        public void Save()
        {
            SaveToDatabase();

            #region general
            NextPvrConfigHelper.EpgUpdateHour = this.EpgUpdateHour;
            NextPvrConfigHelper.UpdateDvbEpgDuringLiveTv = this.UpdateDvbEpgDuringLiveTv;
            NextPvrConfigHelper.LiveTvBufferDirectory = this.LiveTvBufferDirectory;
            #endregion

            #region recordings
            NextPvrConfigHelper.PrePadding = this.PrePadding;
            NextPvrConfigHelper.PostPadding = this.PostPadding;
            NextPvrConfigHelper.AvoidDuplicateRecordings = this.AvoidDuplicateRecordings;
            NextPvrConfigHelper.BlockShutDownWhileRecording = this.BlockShutDownWhileRecording;
            NextPvrConfigHelper.RecurringMatch = this.RecurringMatch;
            /* dont do this until its working, testing against a live system after all....
            NextPvrConfigHelper.DefaultRecordingDirectory = System.IO.Path.Combine(this.DefaultRecordingDirectoryRoot, "Everyone");

            List<KeyValuePair<string, string>> recordingDirectories = new List<KeyValuePair<string, string>>();
            if (EnableUserSupport)
            {
                var users = Models.User.LoadAll();
                foreach (var user in users)
                {
                    var userDirectories = RecordingDirectory.LoadForUser(user.Oid);
                    if (userDirectories.Count == 0)
                        userDirectories.Add(RecordingDirectory.Create(user.Oid, "Default")); // no directories for the user, create the default one

                    recordingDirectories.AddRange(userDirectories.Select(x => new KeyValuePair<string, string>("{0}-{1}".FormatStr(user.Username, x.Name),
                                                                                                               System.IO.Path.Combine(this.DefaultRecordingDirectoryRoot, user.Username, x.Name))));
                }
            }

            NextPvrConfigHelper.ExtraRecordingDirectories = recordingDirectories.ToArray();
             * */
            #endregion

            NextPvrConfigHelper.Save();
        }

        private void SaveToDatabase()
        {
            var db = DbHelper.GetDatabase();

            var type = this.GetType();
            Action<string> deleteSetting = delegate(string settingName){
                db.Execute("delete from setting where name = @0", settingName);
            };
            foreach (var prop in type.GetProperties(System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var proptype = prop.PropertyType;
                if (proptype == typeof(int))
                {
                    deleteSetting(prop.Name);
                    db.Execute("insert into setting(name, intvalue) values (@0, @1)", prop.Name, prop.GetValue(this, null));
                }
                else if (proptype.IsEnum)
                {
                    deleteSetting(prop.Name);
                    db.Execute("insert into setting(name, intvalue) values (@0, @1)", prop.Name, (int)prop.GetValue(this, null));
                }
                else if (proptype == typeof(string))
                {
                    deleteSetting(prop.Name);
                    db.Execute("insert into setting(name, stringvalue) values (@0, @1)", prop.Name, prop.GetValue(this, null));
                }
                else if (proptype == typeof(double))
                {
                    deleteSetting(prop.Name);
                    db.Execute("insert into setting(name, doublevalue) values (@0, @1)", prop.Name, prop.GetValue(this, null));
                }
                else if (proptype == typeof(bool))
                {
                    deleteSetting(prop.Name);
                    db.Execute("insert into setting(name, boolvalue) values (@0, @1)", prop.Name, prop.GetValue(this, null));
                }
                else if (proptype == typeof(DateTime))
                {
                    deleteSetting(prop.Name);
                    db.Execute("insert into setting(name, datetimevalue) values (@0, @1)", prop.Name, prop.GetValue(this, null));
                }
            }
        }
    }
}