using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NextPvrWebConsole.Models;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NextPvrWebConsole.Tests
{
    [TestClass]
    public class NextPvrWebConsoleTest
    {
        public NextPvrWebConsoleTest()
        {
            //new NextPvrWebConsole.Controllers.SetupController().Index(new SetupModel()
            //{
            //    Username = "setupuser",
            //    EmailAddress = "setup@user.test",
            //    Password = "setupuser",
            //    ConfirmPassword = "setupuser"
            //});

            //var config = new NextPvrWebConsole.Models.Configuration();
            //config.UserBaseRecordingDirectory = System.IO.Path.GetTempPath();
            //config.Save();
        }

        protected Models.User User;

        [TestInitialize()]
        public void BaseStartup()
        {
            DbHelper.CreateDatabase(System.IO.Path.GetTempFileName());

            // setup, delete all scheduled recordings, please backup your database before running unit tests!
            var settingsHelper = NUtility.SettingsHelper.GetInstance();
            string npvrDir = settingsHelper.GetDataDirectory();
            string backupDb = System.IO.Path.Combine(npvrDir, "npvr.unittest_backup.db3");
            // backup the db file
            System.IO.File.Copy(System.IO.Path.Combine(npvrDir, settingsHelper.GetDatabaseFilename()), backupDb, true);

            var db = NUtility.DatabaseHelper.GetInstance();
            var conn = db.GetConnection();
            db.CreateCommand(conn, "DELETE FROM RECENTLY_DELETED").ExecuteNonQuery();
            db.CreateCommand(conn, "DELETE FROM SCHEDULED_RECORDING").ExecuteNonQuery();
            db.CreateCommand(conn, "DELETE FROM RECURRING_RECORDING").ExecuteNonQuery();

            db.FreeConnection(conn);

            User = Helpers.UserHelper.CreateTestUser();

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
            System.IO.File.Copy(backupDb, nextPvrDbFile, true);
            System.IO.File.Delete(backupDb);
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
    }
}
