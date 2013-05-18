using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NextPvrWebConsole.Models;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;

namespace NextPvrWebConsole.Tests
{
    [TestClass]
    public class NextPvrWebConsoleTest
    {
        private bool PopulateEpg { get; set; }
        public NextPvrWebConsoleTest(bool PopulateEpg = false)
        {
            this.PopulateEpg = PopulateEpg;
        }

        protected Models.User User;

        protected PetaPoco.Database NpvrDb
        {
            get;
            set;
        }

        [TestInitialize()]
        public void BaseStartup()
        {
            // setup, delete all scheduled recordings, please backup your database before running unit tests!
            var settingsHelper = NUtility.SettingsHelper.GetInstance();
            string npvrDir = settingsHelper.GetDataDirectory();
            string backupDb = System.IO.Path.Combine(npvrDir, "npvr.unittest_backup.db3");

            this.NpvrDb = new PetaPoco.Database(@"Data Source={0};Version=3;".FormatStr(System.IO.Path.Combine(npvrDir, "npvr.db3")), "System.Data.SQLite");

            DbHelper.CreateDatabase(System.IO.Path.GetTempFileName());

            SetupDummyDatabase();

            // backup the db file
            System.IO.File.Copy(System.IO.Path.Combine(npvrDir, settingsHelper.GetDatabaseFilename()), backupDb, true);

            var db = NUtility.DatabaseHelper.GetInstance();
            var conn = db.GetConnection();
            db.CreateCommand(conn, "DELETE FROM RECENTLY_DELETED").ExecuteNonQuery();
            db.CreateCommand(conn, "DELETE FROM SCHEDULED_RECORDING").ExecuteNonQuery();
            db.CreateCommand(conn, "DELETE FROM RECURRING_RECORDING").ExecuteNonQuery();

            db.FreeConnection(conn);
            

            Startup();
        }

        [TestCleanup()]
        public void BaseCleanup()
        {
            Cleanup();

            if(User != null)
                Helpers.UserHelper.DeleteUser(User);

            var settingsHelper = NUtility.SettingsHelper.GetInstance();
            string npvrDir = settingsHelper.GetDataDirectory();
            string backupDb = System.IO.Path.Combine(npvrDir, "npvr.unittest_backup.db3");
            string nextPvrDbFile = System.IO.Path.Combine(npvrDir, settingsHelper.GetDatabaseFilename());
            // restore the db file
            if (System.IO.File.Exists(backupDb) && System.IO.File.Exists(nextPvrDbFile))
            {
                System.IO.File.Copy(backupDb, nextPvrDbFile, true);
                System.IO.File.Delete(backupDb);
            }
        }

        public virtual void Startup() { }
        public virtual void Cleanup() { }

        protected T LoadController<T>(User User)
        {
            var controller = typeof(T).GetConstructor(new Type[] { }).Invoke(new object[] { }) as NextPvrWebConsole.Controllers.Api.NextPvrWebConsoleApiController;
            if (controller == null)
                throw new Exception("Not a NextPvrWebConsoleApiController");

            controller.GetUserFunction = new Func<User>(delegate
            {
                return User;
            });
            return (T)Convert.ChangeType(controller, typeof(T));
        }


        private void SetupDummyDatabase()
        {
            long captureSourceOid = (long)NpvrDb.FirstOrDefault<long>("select oid from CAPTURE_SOURCE");
            if (captureSourceOid < 1)
            {
                NpvrDb.Execute("insert into CAPTURE_SOURCE(name, recorder_plugin_class, present, enabled, priority) values ('dummy', 'NShared.DigitalRecorder', 1, 1, 1)");
                captureSourceOid = (long)NpvrDb.FirstOrDefault<long>("select oid from CAPTURE_SOURCE");
            }

            Assert.IsTrue(captureSourceOid > 0, "Capture source found.");


            User = Helpers.UserHelper.CreateTestUser();

            if (PopulateEpg)
            {
                var db = DbHelper.GetDatabase();

                db.Execute("insert into recordingdirectory(useroid, name, path, isdefault) values (@0, 'Default', @1, 1)", Globals.SHARED_USER_OID, "");

                for (int i = 0; i < 2; i++)
                {
                    Channel c = new Channel();
                    c.Oid = i + 1;
                    c.Number = i + 1;
                    c.Name = "chan " + (i + 1);
                    c.Enabled = true;

                    db.Insert("channel", "oid", false, c);

                    try
                    {
                        NpvrDb.Execute("insert into CHANNEL VALUES (@0, @1, @2, 'None', '')", c.Oid, c.Name, c.Number);
                    }
                    catch (Exception) { }
                    try
                    {
                        NpvrDb.Execute("insert into CHANNEL_MAPPING VALUES (@0, @1, 0, '<tuning><type>DVB-T</type><locator><frequency>538000</frequency><bandwidth>8</bandwidth></locator><service_id>1201</service_id><tsid>25</tsid><onid>8746</onid><service_type>25</service_type></tuning>', 1, 25)", c.Oid, captureSourceOid);
                    }
                    catch (Exception) { }

                    db.Execute("insert into userchannel values (@0, @1, @2, @3)", User.Oid, c.Oid, c.Number, true);

                    DateTime date = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 0, 0, 0).AddHours(-12);
                    while (date < DateTime.UtcNow.AddDays(9))
                    {
                        NpvrDb.Execute("insert into EPG_EVENT(title, subtitle, description, start_time, end_time, channel_oid, unique_id, rating, season, episode) values (@0, '', '', @1, @2, @3, '', 0, 0, 0)",
                                       "show_" + date.ToString("HH_mm"), date, date.AddHours(3), c.Oid);
                        date = date.AddHours(6);
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    ChannelGroup cg = new ChannelGroup();
                    cg.Name = "cg " + (i + 1);
                    cg.UserOid = Globals.SHARED_USER_OID;
                    cg.OrderOid = i + 1;
                    cg.Enabled = true;
                    cg.ChannelOids = new int[] { 1, 2 };
                    cg.Save(cg.ChannelOids);
                }
            }
        }
    }
}
