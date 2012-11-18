using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NextPvrWebConsole.Models;

namespace NextPvrWebConsole.Tests.Controllers
{
    [TestClass]
    public class UsersTest : NextPvrWebConsoleTest
    {
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
                Assert.IsTrue(ex.Message.Contains("already exists"));
            }
        }

        [TestMethod]
        public void UsersTest_CreateDuplicateSharedUser()
        {
            var controller = new NextPvrWebConsole.Controllers.Api.UsersController();

            Models.UserModel model = new Models.UserModel();
            model.EmailAddress = Helpers.WordGenerator.GetEmailAddress();
            model.Username = "Shared";
            model.Password = Helpers.WordGenerator.GetSequence(12);
            model.ConfirmPassword = model.Password;
            try
            {
                controller.Post(model);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("already exists"));
            }
        }

        [TestMethod]
        public void UsersTest_CreateUserInvalidEmail()
        {
            var controller = new NextPvrWebConsole.Controllers.Api.UsersController();

            Models.UserModel model = new Models.UserModel();
            model.EmailAddress = Helpers.WordGenerator.GetSequence(10, Helpers.WordGenerator.CharacterSet.LowerCase);
            model.Username = Helpers.WordGenerator.GetSequence(10, Helpers.WordGenerator.CharacterSet.LowerCase);
            model.Password = Helpers.WordGenerator.GetSequence(12);
            model.ConfirmPassword = model.Password;
            try
            {
                var user = controller.Post(model);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message == "Value does not fall within the expected range.");
            }
        }

        [TestMethod]
        public void UsersTest_UpdateUser()
        {
            var controller = new NextPvrWebConsole.Controllers.Api.UsersController();

            string username = Helpers.WordGenerator.GetSequence(10, Helpers.WordGenerator.CharacterSet.LowerCase);
            string originalEmail = Helpers.WordGenerator.GetEmailAddress();
            Models.UserModel model = new Models.UserModel();
            model.EmailAddress = originalEmail;
            model.Username = Helpers.WordGenerator.GetSequence(10, Helpers.WordGenerator.CharacterSet.LowerCase);
            model.Password = Helpers.WordGenerator.GetSequence(12);
            model.ConfirmPassword = model.Password;
            model.UserRole = UserRole.Configuration;
            var user = controller.Post(model);

            model = new UserModel();
            model.Oid = user.Oid;
            model.Username = username;
            model.EmailAddress = "new_" + originalEmail;
            model.Administrator = true;
            var user2 = controller.Post(model);

            Assert.AreEqual(model.Oid, user.Oid);
            Assert.AreEqual(model.EmailAddress, user2.EmailAddress);
            Assert.AreEqual(model.Administrator, user2.Administrator);
        }
    }
}
