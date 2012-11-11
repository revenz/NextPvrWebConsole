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
        public int Oid { get; set; }
        public string Username { get; set; }
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

        public static string GetUsername(int Oid)
        {
            var db = DbHelper.GetDatabase();
            return db.FirstOrDefault<string>("select username from [user] where oid = @0", Oid);
        }

        public static User GetByUsername(string Username)
        {
            var db = DbHelper.GetDatabase();
            return db.FirstOrDefault<User>("select * from [user] where username = @0", Username);
        }

        public void Save()
        {
            var db = DbHelper.GetDatabase();
            db.Update(this);
        }

        internal static bool ValidateUser(string UsernameOrEmailAddress, string Password)
        {
            var db = DbHelper.GetDatabase();
            var user = db.FirstOrDefault<User>("select * from [user] where username = @0 or emailaddress = @0", UsernameOrEmailAddress);
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
            return db.Update("[user]", "oid", new { password = this.PasswordHash }, this.Oid) > 0;
        }

        internal static User CreateUser(string Username, string EmailAddress, string Password)
        {
            var db = DbHelper.GetDatabase();
            db.BeginTransaction();
            try
            {
                User user = new User();
                user.Username = Username;
                user.EmailAddress = EmailAddress;
                user.LastLoggedInUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                user.Password = Password;
                user.DateCreatedUtc = DateTime.UtcNow;
                if (db.Insert("user", "oid", true, user) == null)
                    throw new Exception("Failed to create user.");

                // insert default channels.
                db.Execute("insert into [userchannel](useroid, channeloid,[enabled],number) select @0, oid, [enabled], number from channel", user.Oid);

                // insert default groups
                var channelGroups = ChannelGroup.LoadAll(0);
                foreach(var cg in channelGroups)
                {
                    int originalOid = cg.Oid;
                    cg.UserOid = user.Oid;
                    cg.Oid = 0;
                    db.Insert("channelgroup", "oid", true, cg);
                    
                    // insert default channels into default groups
                    foreach(var c in db.Fetch<int>("select channeloid from [channelgroupchannel] where channelgroupoid = @0", originalOid))
                        db.Execute("insert into [channelgroupchannel](channelgroupoid, channeloid) values (@0, @1)", cg.Oid, c);
                }

                // create default recording directory
                db.Execute("insert into [recordingdirectory](useroid, name, isdefault, path) values (@0, @1, 1, '')", user.Oid, "Default");

                db.CompleteTransaction();

                return user;
            }
            catch (Exception ex)
            {
                db.AbortTransaction();
                return null;
            }
        }

        internal static List<User> LoadAll()
        {
            var db = DbHelper.GetDatabase();
            return db.Fetch<User>("select * from user where oid > 0");
        }
    }
}