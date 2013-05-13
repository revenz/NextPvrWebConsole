using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NextPvrWebConsole.Tests.Controllers
{
    [TestClass]
    public class ChannelGroupsTest : NextPvrWebConsoleTest
    {
        [TestMethod]
        public void ChannelGroupsTest_CreateChannelGroup()
        {
            var controller = base.LoadController<NextPvrWebConsole.Controllers.Api.ChannelGroupsController>(User);

            var groups = controller.Get().ToList();
            int originalCount = groups.Count;
            string name = "a new one" + Helpers.WordGenerator.GetSequence(5, 12);
            groups.Add(new Models.ChannelGroup() { Name = name, Enabled = true });

            Assert.IsTrue(controller.Update(groups));

            var groups2 = controller.Get().ToList();

            Assert.IsTrue(groups2.Count == originalCount + 1);

            Assert.IsTrue(groups2.Last().Name == name);
        }

        [TestMethod]
        public void ChannelGroupsTest_CreateDuplicateChannelGroup()
        {
            var controller = base.LoadController<NextPvrWebConsole.Controllers.Api.ChannelGroupsController>(User);

            // test we cant create 2 items with the same name
            var groups = controller.Get().ToList();
            string name = "a new one" + Helpers.WordGenerator.GetSequence(5, 12);
            groups.Add(new Models.ChannelGroup() { Name = name, Enabled = true });
            groups.Add(new Models.ChannelGroup() { Name = name, Enabled = true });

            bool failed = false;
            try
            {
                Assert.IsFalse(controller.Update(groups));
            }
            catch (Exception ex)
            {
                failed = true;
                Assert.IsTrue(ex.Message == "Channel Group names must be unique.");
            }
            Assert.IsTrue(failed);

            // test we can't create a duplicate of an existing group
            groups = controller.Get().ToList();
            groups.Add(new Models.ChannelGroup() { Name = name, Enabled = true });
            controller.Update(groups);

            groups.Add(new Models.ChannelGroup() { Name = name, Enabled = true });
            failed = false;
            try
            {
                Assert.IsFalse(controller.Update(groups));
            }
            catch (Exception ex)
            {
                failed = true;
                Assert.IsTrue(ex.Message == "Channel Group names must be unique.");
            }
            Assert.IsTrue(failed);

            // test if we can create a duplicate once we renamed the original with same name
            groups = controller.Get().ToList();
            groups.Last().Name = "updated_" + groups.Last().Name;
            groups.Add(new Models.ChannelGroup() { Name = name, Enabled = true });
            controller.Update(groups);
        }

        [TestMethod]
        public void ChannelGroupsTest_GetAnotherUsersChannelGroups()
        {
            var userB = Helpers.UserHelper.CreateTestUser();
            try
            {
                string suffix = Helpers.WordGenerator.GetSequence(5, 12);
                var controllerA = base.LoadController<NextPvrWebConsole.Controllers.Api.ChannelGroupsController>(User);
                var channelsController = base.LoadController<NextPvrWebConsole.Controllers.Api.ChannelController>(User);
                int[] channelOids = channelsController.Get().Take(5).Select(x => x.Oid).ToArray();
                Assert.IsTrue(channelOids.Length > 0); // this must be true, otherwise the test wont work

                controllerA.Update(new Models.ChannelGroup[]
                {
                    new Models.ChannelGroup() {  Name = "usera_" +  suffix, ChannelOids = channelOids }
                }.ToList());
                var createdGroup = controllerA.Get().Last();
                Assert.IsTrue(controllerA.GetChannels(createdGroup.Oid, true).Count() == channelOids.Length);

                Assert.IsTrue(createdGroup.Name == "usera_" + suffix);

                var controllerB = base.LoadController<NextPvrWebConsole.Controllers.Api.ChannelGroupsController>(userB);
                Assert.IsTrue(controllerB.Get().Where(x => x.Oid == createdGroup.Oid).Count() == 0);

                Assert.IsTrue(controllerB.GetChannels(createdGroup.Oid, true).Count() == 0); // shouldnt get any channels for this group as its not theirs.
            }
            finally
            {
                Helpers.UserHelper.DeleteUser(userB);
            };
        }
    }
}
