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

        /// <summary>
        /// Gets or sets the website address of this web application, used when sending links via emails to users (forgot password etc)
        /// </summary>
        public string WebsiteAddress { get; set; }

        /// <summary>
        /// Gets or sets the private secret used when encrypting data sent to the user as plain text (forgot password code links etc)
        /// </summary>
        public string PrivateSecret { get; set; }

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

        #region devices
        public bool UseReverseOrderForLiveTv { get; set; }
        #endregion

        #region SmtpSettings
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public bool SmtpUseSsl { get; set; }
        public string SmtpSender { get; set; }
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

            #region devices
            this.UseReverseOrderForLiveTv = NextPvrConfigHelper.UseReverseOrderForLiveTv;
            #endregion

            #region recordings
            this.PrePadding = NextPvrConfigHelper.PrePadding;
            this.PostPadding = NextPvrConfigHelper.PostPadding;
            this.BlockShutDownWhileRecording = NextPvrConfigHelper.BlockShutDownWhileRecording;
            this.RecurringMatch = NextPvrConfigHelper.RecurringMatch;
            this.AvoidDuplicateRecordings = NextPvrConfigHelper.AvoidDuplicateRecordings;
            #endregion

            #region smtp
            this.SmtpServer = "localhost";
            this.SmtpPort = 21;
            this.SmtpUsername = "";
            this.SmtpPassword = "";
            this.SmtpUseSsl = false;
            this.SmtpSender = "";
            #endregion

            this.WebsiteAddress = "http://localhost";
            this.PrivateSecret = Guid.NewGuid().ToString("N");
            #endregion

            var db = DbHelper.GetDatabase();
            var type = this.GetType();
            foreach (var d in db.Fetch<dynamic>("select * from setting"))
            {
                string name = d.name as string;
                var prop = type.GetProperty(name);
                if (prop == null)
                    continue;
                if (prop.PropertyType == typeof(int))
                    prop.SetValue(this, (int)d.intvalue, null);
                else if (prop.PropertyType.IsEnum)
                    prop.SetValue(this, (int)d.intvalue, null);
                else if(prop.PropertyType == typeof(string))
                    prop.SetValue(this, d.stringvalue, null);
                else if (prop.PropertyType == typeof(double))
                    prop.SetValue(this, d.doublevalue, null);
                else if (prop.PropertyType == typeof(bool))
                    prop.SetValue(this, d.boolvalue, null);
                else if (prop.PropertyType == typeof(DateTime))
                    prop.SetValue(this, d.datetimevalue, null);
            }
        }

        public void Save()
        {
            SaveToDatabase();

            #region general
            NextPvrConfigHelper.EpgUpdateHour = this.EpgUpdateHour;
            NextPvrConfigHelper.UpdateDvbEpgDuringLiveTv = this.UpdateDvbEpgDuringLiveTv;
            NextPvrConfigHelper.LiveTvBufferDirectory = this.LiveTvBufferDirectory;
            #endregion

            #region devices
            NextPvrConfigHelper.UseReverseOrderForLiveTv = this.UseReverseOrderForLiveTv;
            #endregion

            #region recordings
            NextPvrConfigHelper.PrePadding = this.PrePadding;
            NextPvrConfigHelper.PostPadding = this.PostPadding;
            NextPvrConfigHelper.AvoidDuplicateRecordings = this.AvoidDuplicateRecordings;
            NextPvrConfigHelper.BlockShutDownWhileRecording = this.BlockShutDownWhileRecording;
            NextPvrConfigHelper.RecurringMatch = this.RecurringMatch;

            #region recording folders

            // first do shared
            var sharedRecordingDirectories = Models.RecordingDirectory.LoadForUser(Globals.SHARED_USER_OID);
            Models.RecordingDirectory defaultDir = null;
            if (sharedRecordingDirectories.Count > 0)
            {
                // only update if there is at least one directory.
                defaultDir = sharedRecordingDirectories.Where(x => x.IsDefault).FirstOrDefault();
                if (defaultDir == null)
                    defaultDir = sharedRecordingDirectories[0];
                sharedRecordingDirectories.Remove(defaultDir);
            }
            List<KeyValuePair<string, string>> extraRecordingDirs = sharedRecordingDirectories.Select(x => new KeyValuePair<string, string>(x.RecordingDirectoryId, x.Path)).ToList();

            // if user support is turned on, write out user directories
            if (this.EnableUserSupport)
            {
                extraRecordingDirs.AddRange(RecordingDirectory.LoadAll().Where(x => x.UserOid != Globals.SHARED_USER_OID).Select(x => new KeyValuePair<string, string>(x.RecordingDirectoryId, x.Path)));
            }
            if (defaultDir != null)
                NextPvrConfigHelper.DefaultRecordingDirectory = defaultDir.Path;

            // only write out the defaults
            NextPvrConfigHelper.ExtraRecordingDirectories = extraRecordingDirs.ToArray();
            #endregion

            #endregion

            NextPvrConfigHelper.Save();
        }

        private void SaveToDatabase()
        {
            var db = DbHelper.GetDatabase();

            if (this.WebsiteAddress.EndsWith("/"))
                this.WebsiteAddress = this.WebsiteAddress.Substring(0, this.WebsiteAddress.Length - 1);

            db.BeginTransaction(); // wrap this up in a transaction to improve the speed of saving these settings
            try
            {
                var type = this.GetType();
                Action<string> deleteSetting = delegate(string settingName)
                {
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
                db.CompleteTransaction();
            }
            catch (Exception)
            {
                db.AbortTransaction();
            }
        }

        internal static void Write()
        {
            new Configuration().Save();
        }
    }
}