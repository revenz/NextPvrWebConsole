using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Threading;

namespace NextPvrWebConsole.Models
{
    public class DbHelper
    {
        private static Mutex dbMutex = new Mutex();
        
        static string _DbFile;
#if(DEBUG)
        public static string DbFile // need this public for unit testing
#else
        static string DbFile
#endif
        {
            private get
            {
                if(_DbFile == null)
                    _DbFile = HttpContext.Current.Server.MapPath("~/App_Data/NextPvrWebConsole.db");
                return _DbFile;
            }
            set { _DbFile = value; }
        }

        static void CreateDatabase(string DbFile)
        {
            SQLiteConnection.CreateFile(DbFile);
            
            using(SQLiteConnection conn = new SQLiteConnection(@"Data Source={0};Version=3;".FormatStr(DbFile))){
                conn.Open();
                foreach (Match match in Regex.Matches(Resources.Files.CreateDatabase_sql, @"(.*?)(([\s]+GO[\s]*)|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase))
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
                using(SQLiteCommand cmd = new SQLiteCommand("INSERT INTO [version](databaseversion) VALUES ({0})".FormatStr(Globals.DB_VERSION), conn))
                {
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        private static bool _Loaded = false;

        public static PetaPoco.Database GetDatabase()
        {
            dbMutex.WaitOne();

            if (!_Loaded)
            {
                if (!System.IO.File.Exists(DbFile))
                    CreateDatabase(DbFile);
                _Loaded = true;
            }

            try
            {
                var db = new PetaPoco.Database(@"Data Source={0};Version=3;".FormatStr(DbFile), "System.Data.SQLite");
#if(DEBUG)
                int version = 0;
                try
                {
                    version = db.ExecuteScalar<int>("select databaseversion from [version]");
                }
                catch (Exception) { }

                if (version != Globals.DB_VERSION)
                {
                    db.Dispose();
                    // recreate db
                    System.IO.File.Delete(DbFile);
                    CreateDatabase(DbFile);
                    db = new PetaPoco.Database(@"Data Source={0};Version=3;".FormatStr(DbFile), "System.Data.SQLite");
                }
#endif
                return db;
            }
            finally
            {
                dbMutex.ReleaseMutex();
            }
        }

        public static void Test() { }
    }
}