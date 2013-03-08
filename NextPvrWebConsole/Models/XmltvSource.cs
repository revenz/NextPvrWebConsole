using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace NextPvrWebConsole.Models
{
    public class XmltvSource : NextPvrWebConsoleModel
    {
        [PetaPoco.Ignore]
        public string ShortName
        {
            get
            {
                try
                {
                    return new FileInfo(Filename).Name;
                }
                catch (Exception) { return null; }
            }
        }
        [PetaPoco.Column]
        public string Filename { get; set; }
        [PetaPoco.Column]
        public DateTime LastScanTime { get; set; }
        [PetaPoco.Column("channeloids")]
        public string _ChannelOids { get; set; }
        [PetaPoco.Ignore]
        public string[] ChannelOids
        {
            get
            {
                return (_ChannelOids ?? "").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                _ChannelOids = String.Join(";", value ?? new string[] { });
            }
        }

        public static XmltvSource[] ImportFromNextPvr()
        {
            var db = DbHelper.GetDatabase();
            List<string> existing = db.Fetch<string>("select lower([filename]) from xmltvsource");
            string[] sourceFiles = NUtility.Channel.LoadAll().Where(x => x.EPGSource == "XMLTV").Select(x =>
                {
                    return Regex.Match(x.EPGMapping, "(?<=(<file>))[^<]+").Value;
                })
                .Distinct()
                .Where(x => !String.IsNullOrWhiteSpace(x) && !existing.Contains(x.ToLower())).ToArray();

            // insert new files
            List<XmltvSource> newSources = new List<XmltvSource>();
            foreach (string file in sourceFiles)
            {
                XmltvSource source = new XmltvSource();
                source.Filename = file;
                try
                {
                    XDocument doc = XDocument.Load(file);
                    source.ChannelOids = doc.Element("tv").Elements("channel")
                                            .Where(x => x.Element("display-name") != null)
                                            .Select(x =>
                                            {
                                                return x.Element("display-name").Value;
                                            }).ToArray();
                    source.LastScanTime = DateTime.Now;
                }
                catch (Exception) { }
                source.Insert();
                newSources.Add(source);
            }

            return newSources.ToArray();
        }

        public bool Insert()
        {
            var db = DbHelper.GetDatabase();
            return db.Insert(this) != null;
        }

        public static XmltvSource[] LoadAll()
        {
            var db = DbHelper.GetDatabase();
            return db.Fetch<XmltvSource>("select * from xmltvsource").ToArray();
        }
    }
}