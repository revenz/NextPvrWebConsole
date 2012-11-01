using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUtility;
using System.Text.RegularExpressions;

namespace NextPvrWebConsole.Models
{
    public class NextPvrConfigHelper
    {
        private static SettingsHelper settings = SettingsHelper.GetInstance();

        public static int PrePadding { get { return settings.GetSetting("/Settigns/Recording/PrePadding", 1); } }
        public static int PostPadding { get { return settings.GetSetting("/Settigns/Recording/PostPadding", 2); } }


        private static RecordingDirectory[] _RecordingDirectories;
        public static RecordingDirectory[] RecordingDirectories
        {
            get
            {
                if (_RecordingDirectories == null || _RecordingDirectories.Length == 0)
                {
                    List<RecordingDirectory> dirs = new List<RecordingDirectory>();
                    string defaultdir = NUtility.SettingsHelper.GetInstance().GetSetting("/Settings/Recording/RecordingDirectory", string.Empty);
                    if (!String.IsNullOrWhiteSpace(defaultdir))
                        dirs.Add(new RecordingDirectory() { IsDefault = true, Name = "Default", Path = defaultdir });

                    string otherdirs = NUtility.SettingsHelper.GetInstance().GetSetting("/Settings/Recording/ExtraRecordingDirectories", string.Empty);
                    foreach (Match match in Regex.Matches(otherdirs ?? "", "[^~]+~[^~]+"))
                    {
                        RecordingDirectory dir = new RecordingDirectory();
                        string[] parts = match.Value.Split('~');
                        dir.Name = parts[0];
                        dir.Path = parts[1];
                        dirs.Add(dir);
                    }
                    _RecordingDirectories = dirs.ToArray();
                }
                return _RecordingDirectories;
            }
        }
    }
}