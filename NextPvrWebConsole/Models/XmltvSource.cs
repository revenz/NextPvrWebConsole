using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace NextPvrWebConsole.Models
{
    [DataContract]
    [PetaPoco.PrimaryKey("Oid")]
    public class XmltvSource : NextPvrWebConsoleModel
    {
        [DataMember]
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
        [DataMember]
        [PetaPoco.Column]
        public int Oid { get; set; }
        [DataMember]
        [PetaPoco.Column]
        public string Filename { get; set; }
        [DataMember]
        [PetaPoco.Column]
        public DateTime LastScanTime { get; set; }
        [PetaPoco.Column("channels")]
        public string _Channels { get; set; }
        [DataMember]
        [PetaPoco.Ignore]
        public XmltvSourceChannel[] Channels
        {
            get
            {
                return (_Channels ?? "").Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(x =>
                       {
                           string[] parts = x.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                           if (parts.Length != 2)
                               return null;
                           return new XmltvSourceChannel() { Oid = parts[0], Name = parts[1] };
                       }).Where(x => x != null).ToArray(); 
            }
            set
            {
                if (value == null)
                    _Channels = "";
                else
                {
                    _Channels = String.Join(";", value.Select(x => { return x.ToString(); }));
                }
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
                this.Channels = doc.Element("tv").Elements("channel")
                                        .Where(x => x.Element("display-name") != null)
                                        .Select(x =>
                                        {
                                            return new XmltvSourceChannel { Oid = x.Attribute("id").Value, Name = x.Element("display-name").Value };
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
            if (this._Channels == null)
                this._Channels = "";
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

    public class XmltvSourceChannel
    {
        public string Oid { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return "{0}|{1}".FormatStr(this.Oid, this.Name);
        }
    }
}