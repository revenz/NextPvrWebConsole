using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

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
            return MyMembershipUser.CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
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

        internal static MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            status = MembershipCreateStatus.UserRejected;
            var user = Models.User.CreateUser(username, password);
            if (user != null)
                status = MembershipCreateStatus.Success;
            return new MyMembershipUser(user);
        }
    }
}