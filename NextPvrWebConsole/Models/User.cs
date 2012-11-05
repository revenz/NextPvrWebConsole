using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Web.Security;

namespace NextPvrWebConsole.Models
{
    public enum UserRole
    {
        Dashboard = 1,
        Guide = 2,
        Recordings = 4,
        SuperAdmin = 65535
    }
    public class User
    {
        public int Id { get; set; }
        public string EmailAddress { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastLoggedInUtc { get; set; }
        public DateTime DateCreatedUtc { get; set; }
        public UserRole UserRole { get; set; }
        public bool ReadOnly { get; set; }
        [PetaPoco.Ignore]
        public string Password { set { this.PasswordHash = BCrypt.HashPassword(value, BCrypt.GenerateSalt()); } }

        public static User GetByEmailAddress(string EmailAddress)
        {
            var db = DbHelper.GetDatabase();
            return db.FirstOrDefault<User>("select * from [user] where emailaddress = @0", EmailAddress);
        }

        public void Save()
        {
            var db = DbHelper.GetDatabase();
            db.Update(this);
        }

        internal static bool ValidateUser(string EmailAddress, string Password)
        {
            var user = GetByEmailAddress(EmailAddress);
            if (user == null)
                return false;
            return BCrypt.CheckPassword(Password, user.PasswordHash);
        }

        internal bool ChangePassword(string OldPassword, string NewPassword)
        {
            if (!BCrypt.CheckPassword(OldPassword, this.PasswordHash))
                return false;
            this.Password = NewPassword;
            var db = DbHelper.GetDatabase();
            return db.Update("[user]", "id", new { password = this.PasswordHash }, this.Id) > 0;
        }

        internal static User CreateUser(string EmailAddress, string Password)
        {
            User user = new User();
            user.EmailAddress = EmailAddress;
            user.LastLoggedInUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            user.Password = Password;
            user.DateCreatedUtc = DateTime.UtcNow;
            var db = DbHelper.GetDatabase();
            if(db.Insert(user) != null)
                return user;
            return null;
        }
    }
}