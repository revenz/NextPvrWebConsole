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

        public static int PrePadding 
        { 
            get { return settings.GetSetting("/Settigns/Recording/PrePadding", 1); }
            set { settings.SetSetting("/Settigns/Recording/PrePadding", value); }
        }
        public static int PostPadding {
            get { return settings.GetSetting("/Settigns/Recording/PostPadding", 2); }
            set { settings.SetSetting("/Settigns/Recording/PostPadding", value); }
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

        public static KeyValuePair<string, string>[] ExtraRecordingDirectories
        {
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
    }
}