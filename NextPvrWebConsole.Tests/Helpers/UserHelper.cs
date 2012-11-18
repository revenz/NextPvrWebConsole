using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextPvrWebConsole.Tests.Helpers
{
    class UserHelper
    {
        public static NextPvrWebConsole.Models.User CreateTestUser()
        {
            string username = WordGenerator.GetSequence(12, 15, WordGenerator.CharacterSet.LowerCase);
            string emailAddress = WordGenerator.GetEmailAddress();
            string password = "password";
            NextPvrWebConsole.Models.UserRole userRole = NextPvrWebConsole.Globals.USER_ROLE_ALL;
            bool isAdmin = true;
            return Models.User.CreateUser(username, emailAddress, password, userRole, isAdmin);
        }

        public static void DeleteUser(NextPvrWebConsole.Models.User User)
        {
            Models.User.Delete(User.Oid);
        }
    }
}
