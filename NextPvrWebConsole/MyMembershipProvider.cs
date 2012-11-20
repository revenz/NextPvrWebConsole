using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using WebMatrix.WebData;

namespace NextPvrWebConsole
{
    public class MyMembershipProvider:MembershipProvider
    {

        public override string ApplicationName
        {
            get
            {
                return "NextPvrWebConsole";
            }
            set
            {
            }
        }

        public override bool ValidateUser(string username, string password)
        {
            return MyMembershipUser.ValidateUser(username, password);
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return MyMembershipUser.GetUser(username);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            return MyMembershipUser.ChangePassword(username, oldPassword, newPassword);
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            status = MembershipCreateStatus.UserRejected;
            var result = MyMembershipUser.CreateUser(username, password);
            if (result != null)
                status = MembershipCreateStatus.Success;
            return result;
        }

        #region not implemented yet

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get { throw new NotImplementedException(); }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class MyMembershipUser : MembershipUser
    {
        public Models.User User { get; private set; }

        public MyMembershipUser(Models.User User)
        {
            this.User = User;
        }

        internal static bool ValidateUser(string username, string password)
        {
            return Models.User.ValidateUser(username, password);
        }

        internal static MyMembershipUser GetUser(string username)
        {
            var user = Models.User.GetByEmailAddress(username);
            return user == null ? null : new MyMembershipUser(user);
        }

        internal static bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var user = Models.User.GetByEmailAddress(username);
            return user == null ? false : user.ChangePassword(oldPassword, newPassword);
        }

        internal static MembershipUser CreateUser(string username, string password)
        {
            throw new NotImplementedException(); // cant implement because we require a email address, could make username email and substring user part of that to get username....
        }
    }

    public class MyRoleProvider : RoleProvider
    {
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
        }

        public override string ApplicationName
        {
            get { return "NextPvrWebConsole"; }
            set{ }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        public override string[] GetRolesForUser(string username)
        {
            var user = Models.User.GetByUsername(username);
            if (user == null)
                return new string[] { };
            return user.UserRole.ToString().Split(new[] { ", " }, StringSplitOptions.None);
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }

}