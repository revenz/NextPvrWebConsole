using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Web.Security;
using NextPvrWebConsole.Validators;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace NextPvrWebConsole.Models
{
    [Flags]
    public enum UserRole
    {
        /* NOTE: if adding to this list, update Globals.USER_ROLE_ALL const value */
        Dashboard = 1,
        Guide = 2,
        Recordings = 4,
        Configuration = 8
    }


    public class UserModel
    {
        public int Oid { get; set; }

        [Required]
        [Display(Name = "Username")]
        [Username]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        [Email]
        public string EmailAddress { get; set; }

        public UserRole UserRole { get; set; }

        public bool Administrator { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public User Save()
        {
            var db = DbHelper.GetDatabase();
            db.BeginTransaction();

            bool exists = db.ExecuteScalar<int>("select count(oid) from [user] where (lower(username) = @0 or lower(emailaddress)=@1) and (oid <> @2 or @2 = 0)", this.Username.ToLower(), this.EmailAddress.ToLower(), this.Oid) > 0;
            if (exists)
                throw new Exception("User with that username or email address already exists.");
            try
            {
                User user = new User(){
                    Administrator = this.Administrator,
                    EmailAddress = this.EmailAddress,
                    Oid = this.Oid,
                    Username = this.Username,
                    UserRole = this.UserRole
                };

                if (user.Administrator) // admins should have access to all
                    user.UserRole = Globals.USER_ROLE_ALL;

                if (this.Oid == 0)
                {
                    if (this.Password.Length < 6)
                        throw new ArgumentException(); // shouldnt have been posted back

                    user.DateCreatedUtc = DateTime.UtcNow;
                    user.LastLoggedInUtc = new DateTime(1970,1,1);
                    user.Password = this.Password;
                    db.Insert("user", "oid", true, user);
                }
                else
                {
                    // update existing
                    db.Update("user", "oid", user, user.Oid, new string[] { "EmailAddress", "Administrator", "UserRole" });
                }
                db.CompleteTransaction();

                return User.GetByEmailAddress(this.EmailAddress);
            }
            catch (Exception ex)
            {
                db.AbortTransaction();
                throw ex;
            }
        }
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
        public bool Administrator { get; set; }
        
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

        internal static User CreateUser(string Username, string EmailAddress, string Password, UserRole UserRole, bool Administrator = false, DateTime? LastLoggedInUtc = null)
        {
            var db = DbHelper.GetDatabase();
            db.BeginTransaction();
            try
            {
                User user = new User();
                user.Username = Username;
                user.EmailAddress = EmailAddress;
                if(LastLoggedInUtc == null)
                    LastLoggedInUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                user.LastLoggedInUtc = LastLoggedInUtc.Value;
                user.Password = Password;
                user.Administrator = Administrator;
                user.UserRole = UserRole;
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
            return db.Fetch<User>("select * from user where oid <> " + Globals.SHARED_USER_OID);
        }

        internal static void Delete(int UserOid)
        {
            if (UserOid == Globals.SHARED_USER_OID)
                throw new Exception("You cannot delete the '{0}' user.".FormatStr(Globals.SHARED_USER_USERNAME));
            var db = DbHelper.GetDatabase();
            db.BeginTransaction();
            try
            {
                db.Execute("delete from [userchannel] where useroid = @0", UserOid);
                db.Execute("delete from [channelgroupchannel] where channelgroupoid in (select oid from [channelgroup] where useroid = @0)", UserOid);
                db.Execute("delete from [channelgroup] where useroid = @0", UserOid);
                db.Execute("delete from [recordingdirectory] where useroid = @0", UserOid);
                db.Execute("delete from [user] where oid = @0", UserOid);
            }
            catch (Exception ex)
            {
                db.AbortTransaction();
                throw ex;
            }

        }
    }
}