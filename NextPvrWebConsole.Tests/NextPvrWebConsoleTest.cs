using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NextPvrWebConsole.Models;
using System.Web;

namespace NextPvrWebConsole.Tests
{
    public class NextPvrWebConsoleTest
    {
        public NextPvrWebConsoleTest()
        {
#if(DEBUG)
            DbHelper.DbFile = System.IO.Path.GetTempFileName();

            new NextPvrWebConsole.Controllers.SetupController().Index(new SetupModel()
            {
                Username = "setupuser",
                EmailAddress = "setup@user.test",
                Password = "setupuser",
                ConfirmPassword = "setupuser"
            });
#endif
        }

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
