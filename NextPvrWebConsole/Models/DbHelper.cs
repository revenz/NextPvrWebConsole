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
        }

        public static PetaPoco.Database GetDatabase()
        {
            return new PetaPoco.Database(@"Data Source={0};Version=3;".FormatStr(DbFile), "System.Data.SQLite");
        }

        public static void Test() { }
    }
}