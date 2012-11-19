using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace NextPvrWebConsole.Tests.Controllers
{
    [TestClass]
    public class SecurityTest
    {
        [TestMethod]
        public void SecurityTest_ValidateApiRequiresLogin()
        {
            // get all api controllers
            var types = Assembly.GetAssembly(typeof(NextPvrWebConsole.Controllers.Api.NextPvrWebConsoleApiController)).GetTypes().Where(t => String.Equals(t.Namespace, "NextPvrWebConsole.Controllers.Api", StringComparison.Ordinal)).ToArray();
            Assert.IsTrue(types.Length > 0);
            
            // check every API controller has the authorize attribute
            foreach (var t in types)
            {
                if (!typeof(System.Web.Http.ApiController).IsAssignableFrom(t))
                    continue;
                if (t == typeof(NextPvrWebConsole.Controllers.Api.NextPvrWebConsoleApiController))
                    continue; // no need to check base type
                if (t == typeof(NextPvrWebConsole.Controllers.Api.ServiceController))
                    continue; // this is the special "/service" endpoint used by clients like xbmc, so no need to check this for [Authorize]
                bool found = t.GetCustomAttributes(typeof(System.Web.Http.AuthorizeAttribute), false).Length > 0;
                if (!found)
                    Assert.Fail("Failed to find [Authorize] on: " + t.Name);
            }
        }
    }
}
