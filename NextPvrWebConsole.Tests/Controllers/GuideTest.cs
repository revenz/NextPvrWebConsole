using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace NextPvrWebConsole.Tests.Controllers
{
    [TestClass]
    public class GuideTest : NextPvrWebConsoleTest
    {
        [TestMethod]
        public void GuideTest_SpeedTest()
        {
            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);
            string groupName = Models.ChannelGroup.LoadAll(User.Oid, false).First().Name;
            Stopwatch timer = new Stopwatch();
            timer.Start();
            var listings = guide.Get(DateTime.Now, groupName);
            timer.Stop();
            Assert.IsTrue(timer.Elapsed.TotalSeconds < 2);
        }
    }
}
