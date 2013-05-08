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
        internal static string DbFile
        {
            get
            {
                if(_DbFile == null)
                    _DbFile = HttpContext.Current.Server.MapPath("~/App_Data/NextPvrWebConsole.db");
                return _DbFile;
            }
            set { _DbFile = value; }
        }

        internal static void CreateDatabase(string DbFile)
        {
            string path = new System.IO.FileInfo(DbFile).DirectoryName;
            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

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

            DbHelper.DbFile = DbFile;
        }

        public static PetaPoco.Database GetDatabase(bool ValidateDatabase = true)
        {
            dbMutex.WaitOne();

            if (!System.IO.File.Exists(DbFile))
                CreateDatabase(DbFile);

            try
            {
                var db = new PetaPoco.Database(@"Data Source={0};Version=3;".FormatStr(DbFile), "System.Data.SQLite");
                if (ValidateDatabase)
                {
                    int version = 0;
                    try
                    {
                        version = db.ExecuteScalar<int>("select databaseversion from [version]");
#if(DEBUG)
                        if (version != Globals.DB_VERSION)
                        {
                            db.Dispose();
                            // recreate db
                            System.IO.File.Delete(DbFile);
                            CreateDatabase(DbFile);
                            db = new PetaPoco.Database(@"Data Source={0};Version=3;".FormatStr(DbFile), "System.Data.SQLite");
                        }
#endif
                    }
                    catch (Exception)
                    {
                        throw new SetupException();
                    }
                }

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