using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public class Configuration
    {
        public int PrePadding { get; set; }
        public int PostPadding { get; set; }
        public string DefaultRecordingDirectoryRoot { get; set; }
        public bool EnableUserSupport { get; set; }

        public Configuration()
        {
            #region defaults
            this.EnableUserSupport = true;
            this.DefaultRecordingDirectoryRoot = NextPvrConfigHelper.DefaultRecordingDirectory;
            this.PrePadding = NextPvrConfigHelper.PrePadding;
            this.PostPadding = NextPvrConfigHelper.PostPadding;
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

            NextPvrConfigHelper.PrePadding = PrePadding;
            NextPvrConfigHelper.PostPadding = PostPadding;

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