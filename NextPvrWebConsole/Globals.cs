using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole
{
    public class Globals
    {
        public const int DB_VERSION = 102;
        public const int SHARED_USER_OID = 0;
        public const string SHARED_USER_USERNAME = "Shared";

        public const NextPvrWebConsole.Models.UserRole USER_ROLE_ALL = Models.UserRole.Dashboard | Models.UserRole.Guide | Models.UserRole.Recordings | Models.UserRole.Configuration;
    }
}