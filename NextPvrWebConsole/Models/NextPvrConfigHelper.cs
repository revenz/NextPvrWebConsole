using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUtility;
using System.Text.RegularExpressions;
using System.Text;

namespace NextPvrWebConsole.Models
{
    public class NextPvrConfigHelper
    {
        private static SettingsHelper settings = SettingsHelper.GetInstance();

        public static int EpgUpdateHour
        {
            get { return settings.GetSetting("/Settings/General/EPGUpdateHour", 2); }
            set { settings.SetSetting("/Settings/General/EPGUpdateHour", value); }
        }
        public static bool UpdateDvbEpgDuringLiveTv
        {
            get { return settings.GetSetting("/Settings/General/EPGLiveTVUpdates", false); }
            set { settings.SetSetting("/Settings/General/EPGLiveTVUpdates", value); }
        }
        public static string LiveTvBufferDirectory
        {
            get { return settings.GetSetting("/Settings/Recording/LiveTVBufferDirectory", null); }
            set { settings.SetSetting("/Settings/Recording/LiveTVBufferDirectory", value.EndsWith(@"\") ? value : (value + @"\") ); }
        }
        public static int PrePadding 
        { 
            get { return settings.GetSetting("/Settings/Recording/PrePadding", 1); }
            set { settings.SetSetting("/Settings/Recording/PrePadding", value); }
        }
        public static int PostPadding {
            get { return settings.GetSetting("/Settings/Recording/PostPadding", 2); }
            set { settings.SetSetting("/Settings/Recording/PostPadding", value); }
        }
        public static int WebServerPort
        {
            get { return settings.GetSetting("/Settings/WebServer/Port", 8866); }
            set { settings.SetSetting("/Settings/WebServer/Port", value); }
        }
        public static Version NextPvrVersion
        {
            get
            {
                string version = settings.GetSetting("/Settings/Version/CurrentVersion", "0.0.0");
                return Version.Parse(version);
            }
        }

        public static string DefaultRecordingDirectory
        {
            get
            {
                return NUtility.SettingsHelper.GetInstance().GetSetting("/Settings/Recording/RecordingDirectory", null);
            }
            set
            {
                NUtility.SettingsHelper.GetInstance().SetSetting("/Settings/Recording/RecordingDirectory", value);
            }
        }

        public static Dictionary<string, string> GetAllRecordingDirectories()
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            results.Add(null, DefaultRecordingDirectory);
            foreach (var kv in ExtraRecordingDirectories)
                results.Add(kv.Key, kv.Value);
            return results;
        }

        public static Dictionary<string, string> GetAllRecordingDirectories(int UserOid)
        {
            string username = Models.User.GetUsername(UserOid);
            Dictionary<string, string> results = new Dictionary<string, string>();
            results.Add("", DefaultRecordingDirectory);
            foreach (var kv in ExtraRecordingDirectories)
            {
                if(kv.Key.EndsWith("[{0}]".FormatStr(username)))
                    results.Add(kv.Key, kv.Value);
            }
            return results;
        }

        public static KeyValuePair<string, string>[] ExtraRecordingDirectories
        {
            get
            {
                string str = settings.GetSetting("/Settings/Recording/ExtraRecordingDirectories", "");
                List<KeyValuePair<string, string>> results = new List<KeyValuePair<string,string>>();
                foreach (Match match in Regex.Matches(str, "[^~]+~[^~]+~"))
                {
                    string[] parts = match.Value.Split(new string[]{"~"}, StringSplitOptions.RemoveEmptyEntries);
                    results.Add(new KeyValuePair<string, string>(parts[0], parts[1]));
                }
                return results.ToArray();
            }
            set
            {
                // make sure the paths exist
                if (value != null)
                {
                    foreach (KeyValuePair<string, string> dir in value)
                    {
                        try
                        {
                            if (System.IO.Directory.Exists(dir.Value))
                            {
                                if (!System.IO.Directory.CreateDirectory(dir.Value).Exists)
                                    throw new Exception("Failed to create directory: " + dir.Value);
                            }
                        }
                        catch (Exception)
                        {
                            throw new Exception("Failed to create recording directory: " + dir.Value);
                        }
                    }
                }
                settings.SetSetting("/Settings/Recording/ExtraRecordingDirectories", value == null ? "" : String.Join("", (from rd in value select "{0}~{1}~".FormatStr(rd.Key, rd.Value)).ToArray()));
            }
        }

        internal static void Save()
        {
            settings.Save();
        }
    }
}