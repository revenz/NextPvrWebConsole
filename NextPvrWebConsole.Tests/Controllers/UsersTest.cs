using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextPvrWebConsole.Models;

namespace NextPvrWebConsole.Tests.Controllers
{
    [TestClass]
    public class UsersTest
    {
        public UsersTest()
        {
            DbHelper.DbFile = System.IO.Path.GetTempFileName();
        }

        [TestMethod]
        public void UsersTest_CreateDuplicateUser()
        {
            var controller = new NextPvrWebConsole.Controllers.Api.UsersController();
            #region create first user
            Models.UserModel model = new Models.UserModel();
            model.EmailAddress = Helpers.WordGenerator.GetEmailAddress();
            model.Username = model.EmailAddress.Substring(0, model.EmailAddress.IndexOf("@"));
            model.Password = Helpers.WordGenerator.GetSequence(12);
            model.ConfirmPassword = model.Password;
            var user = controller.Post(model);

            Assert.IsNotNull(user);
            #endregion

            // now try and recreate the user
            model.Oid = 0;
            try
            {
                controller.Post(model);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.AreSame(ex.Message, "test");
            }
        }
    }
}
