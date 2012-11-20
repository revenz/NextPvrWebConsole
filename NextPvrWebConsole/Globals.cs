using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole
{
    public class Globals
    {
        public const int DB_VERSION = 113;
        public const int SHARED_USER_OID = 1;
        public const string SHARED_USER_USERNAME = "Shared";

        public const NextPvrWebConsole.Models.UserRole USER_ROLE_ALL = Models.UserRole.Dashboard | Models.UserRole.Guide | Models.UserRole.Recordings | Models.UserRole.Configuration;

        public static Version NextPvrWebConsoleVersion = new Version("1.0.0.0"); // todo get version
        public static Version NextPvrVersion = new Version("1.0.0.0"); // todo get NextPVR version
        public static string WebConsolePhysicalPath = null;
        public static string WebConsoleLoggingDirectory = null;
    }
}