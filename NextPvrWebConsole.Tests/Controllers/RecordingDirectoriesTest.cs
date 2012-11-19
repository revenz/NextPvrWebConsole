using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NextPvrWebConsole.Tests.Controllers
{
    [TestClass]
    public class RecordingDirectoriesTest:NextPvrWebConsoleTest
    {
        [TestMethod]
        public void RecordingDirectoriesTest_CreateRecordingDirectory()
        {
            var user = Helpers.UserHelper.CreateTestUser();
            try
            {
                var controller = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingDirectoriesController>(user);

                string name = Helpers.WordGenerator.GetSequence(5, 12);
                var rds = controller.Get().ToList();
                rds.Add(new Models.RecordingDirectory() { Name = name });
                controller.Post(rds);

                var rds2 = controller.Get().ToList();

                Assert.IsTrue(rds.Count == rds2.Count);
                Assert.IsTrue(rds2.Last().Name == name);
            }
            finally
            {
                Helpers.UserHelper.DeleteUser(user);
            }
        }

        [TestMethod]
        public void RecordingDirectoriesTest_CreateDuplicateRecordingDirectory()
        {
            var user = Helpers.UserHelper.CreateTestUser();
            try
            {
                var controller = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingDirectoriesController>(user);
                string name = Helpers.WordGenerator.GetSequence(5, 12);

                // try create 2 new ones with same name
                var rds = controller.Get().ToList();
                rds.Add(new Models.RecordingDirectory() { Name = name });
                rds.Add(new Models.RecordingDirectory() { Name = name });
                bool failed = false;
                try
                {
                    controller.Post(rds);
                }
                catch (Exception) { failed = true; }
                Assert.IsTrue(failed);

                // try creating one with name, saving, then creating another with the same name
                name = Helpers.WordGenerator.GetSequence(5, 12);
                rds = controller.Get().ToList();
                rds.Add(new Models.RecordingDirectory() { Name = name });
                controller.Post(rds);
                rds = controller.Get().ToList();
                Assert.IsTrue(rds.Last().Name == name);
                rds.Add(new Models.RecordingDirectory() { Name = name });
                failed = false;
                try
                {
                    controller.Post(rds);
                }
                catch (Exception) { failed = true; }
                Assert.IsTrue(failed);

                // try create one with name, saving, then renaming it and creating another with the same name
                name = Helpers.WordGenerator.GetSequence(5, 12);
                rds = controller.Get().ToList();
                rds.Add(new Models.RecordingDirectory() { Name = name });
                controller.Post(rds);
                rds = controller.Get().ToList();
                Assert.IsTrue(rds.Last().Name == name);
                rds.Last().Name = "updated_" + name;
                rds.Add(new Models.RecordingDirectory() { Name = name });
                controller.Post(rds);
                rds = controller.Get().ToList();
                Assert.IsTrue(rds.Last().Name == name);
                Assert.IsTrue(rds[rds.Count - 2].Name == "updated_" + name);
            }
            finally
            {
                Helpers.UserHelper.DeleteUser(user);
            }
        }

        [TestMethod]
        public void RecordingDirectoriesTest_InvalidCharactersTest()
        {
            var user = Helpers.UserHelper.CreateTestUser();
            try
            {
                var controller = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingDirectoriesController>(user);

                foreach(char invalid in new char[]{ ':', '[', ']'}.Union(System.IO.Path.GetInvalidPathChars()))
                {
                    string name = Helpers.WordGenerator.GetSequence(5, 12) + invalid;
                    var rds = controller.Get().ToList();
                    rds.Add(new Models.RecordingDirectory() { Name = name });
                    bool failed = false;
                    try
                    {
                        controller.Post(rds);
                    }
                    catch (Exception) { failed = true; }
                    if (!failed)
                        Assert.Fail("Accepted invalid recording directory character: " + invalid);
                }
            }
            finally
            {
                Helpers.UserHelper.DeleteUser(user);
            }
        }
    }
}
