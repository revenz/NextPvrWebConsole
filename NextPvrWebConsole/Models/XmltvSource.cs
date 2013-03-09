using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace NextPvrWebConsole.Models
{
    [PetaPoco.PrimaryKey("Oid")]
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
        public int Oid { get; set; }
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
                source.Scan(false);
                source.Save();
                newSources.Add(source);
            }

            return newSources.ToArray();
        }

        public bool Scan(bool Save = true)
        {
            try
            {
                XDocument doc = XDocument.Load(this.Filename);
                this.ChannelOids = doc.Element("tv").Elements("channel")
                                        .Where(x => x.Element("display-name") != null)
                                        .Select(x =>
                                        {
                                            return x.Element("display-name").Value;
                                        }).ToArray();
                this.LastScanTime = DateTime.Now;

                if (Save)
                    this.Save();
                return true;
            }
            catch (Exception) { return false; }
        }

        public bool Save(PetaPoco.Database db = null)
        {
            if(db == null)
                db = DbHelper.GetDatabase();
            if (this._ChannelOids == null)
                this._ChannelOids = "";
            if (this.LastScanTime < System.Data.SqlTypes.SqlDateTime.MinValue.Value)
                this.LastScanTime = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
            if (this.Oid < 1)
                return db.Insert("xmltvsource", "oid", true, this) != null;
            else
                return db.Update(this) > 0;
        }

        public bool Delete(PetaPoco.Database db = null)
        {
            if (db == null)
                db = DbHelper.GetDatabase();
            return db.Execute("delete from xmltvsource where oid=@0", this.Oid) > 0;
        }

        public static XmltvSource[] LoadAll()
        {
            var db = DbHelper.GetDatabase();
            return db.Fetch<XmltvSource>("select * from xmltvsource").ToArray();
        }

        public static XmltvSource LoadByOid(int Oid)
        {
            var db = DbHelper.GetDatabase();
            return db.SingleOrDefault<XmltvSource>(Oid);
        }

        internal static bool Save(XmltvSource[] Sources)
        {
            var db = DbHelper.GetDatabase();
            var toDelete = Models.XmltvSource.LoadAll().Where(x => !Sources.Select(y => y.Oid).Contains(x.Oid)).ToArray();
            try
            {
                db.BeginTransaction();

                foreach (var source in Sources)
                    source.Save(db);

                foreach (var source in toDelete)
                    source.Delete(db);

                db.CompleteTransaction();
                return true;
            }
            catch (Exception)
            {
                db.AbortTransaction();
                return false;
            }
        }
    }
}