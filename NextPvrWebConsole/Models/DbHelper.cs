using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace NextPvrWebConsole.Models
{
    public class DbHelper
    {
        private static string DbFile { get; set; }
        static DbHelper()
        {
            DbFile= HttpContext.Current.Server.MapPath("~/App_Data/NextPvrWebConsole.db");
            if (!System.IO.File.Exists(DbFile))
                CreateDatabase(DbFile);
        }

        static void CreateDatabase(string DbFile)
        {
            SQLiteConnection.CreateFile(DbFile);

            string createDb = System.IO.File.ReadAllText(HttpContext.Current.Server.MapPath("~/Models/Database/CreateDatabase.sql"));

            using(SQLiteConnection conn = new SQLiteConnection(@"Data Source={0};Version=3;".FormatStr(DbFile))){
                conn.Open();
                foreach (Match match in Regex.Matches(createDb, @"(.*?)(([\s]+GO[\s]*)|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase))
                {
                    string sql = match.Value.Trim();
                    if (sql.ToUpper().EndsWith("GO"))
                        sql = sql.Substring(0, sql.Length - 2).Trim();
                    if (sql.Length == 0)
                        continue;
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                conn.Close();
            }

            // insert defaults
            var db = DbHelper.GetDatabase();
            db.BeginTransaction();
            try
            {
                // insert channels
                var channels = NUtility.Channel.LoadAll().OrderBy(x => x.Number).Select(x => new Channel() { Oid = x.OID, Name = x.Name, Number = x.Number }).ToArray();
                foreach (var c in channels)
                    db.Insert(c);

                // insert groups
                var groups = NUtility.Channel.GetChannelGroups().Select(x => new ChannelGroup() { Name = x }).ToArray();
                for (int i = 0; i < groups.Length; i++)
                {
                    groups[i].OrderOid = i + 1;
                    db.Insert("channelgroup", "oid", true, groups[i]);
                    foreach (int channelOid in NUtility.Channel.LoadForGroup(groups[i].Name).Select(x => x.OID))
                        db.Execute("insert into [channelgroupchannel](channelgroupoid, channeloid) values (@0, @1)", groups[i].Oid, channelOid);
                }

                db.CompleteTransaction();
            }
            catch (Exception ex) { db.AbortTransaction(); }
        }

        public static PetaPoco.Database GetDatabase()
        {
            return new PetaPoco.Database(@"Data Source={0};Version=3;".FormatStr(DbFile), "System.Data.SQLite");
        }

        public static void Test() { }
    }
}