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
        UserSettings = 8,
        Configuration = 16,
        System = 32
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

            bool exists = db.ExecuteScalar<int>("select count(oid) from [user] where (lower(username) = @0 or lower(emailaddress)=@1) and (oid <> @2 or @2 = @3)", this.Username.ToLower(), this.EmailAddress.ToLower(), this.Oid, Globals.SHARED_USER_OID) > 0;
            if (exists)
                throw new Exception("User with that username or email address already exists.");

            db.BeginTransaction();
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
                    throw new Exception("User does not exist.");
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

    [PetaPoco.PrimaryKey("oid")]
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
        public int DefaultRecordingDirectoryOid { get; set; }

        [PetaPoco.Ignore]
        public string DefaultRecordingDirectoryDirectoryId { get; set; }
        
        [PetaPoco.Ignore]
        public string Password { set { this.PasswordHash = BCrypt.HashPassword(value, BCrypt.GenerateSalt()); } }
                
        public static User GetByEmailAddress(string EmailAddress)
        {            
            var db = DbHelper.GetDatabase();
            return InitUser(db.FirstOrDefault<User>("select * from [user] where emailaddress = @0", EmailAddress));
        }

        public static string GetUsername(int Oid)
        {
            var db = DbHelper.GetDatabase();
            return db.FirstOrDefault<string>("select username from [user] where oid = @0", Oid);
        }

        public static User GetByUsername(string Username)
        {
            var db = DbHelper.GetDatabase();
            return InitUser(db.FirstOrDefault<User>("select * from [user] where username = @0", Username));
        }

        private static User InitUser(User User)
        {
            if (User == null)
                return User;
            var rd = RecordingDirectory.Load(User.DefaultRecordingDirectoryOid);
            if(rd != null)
                User.DefaultRecordingDirectoryDirectoryId = rd.RecordingDirectoryId;
            return User;
        }

        public static void SendPasswordResetRequest(string UsernameOrEmailAddress)
        {
            var db = DbHelper.GetDatabase();
            var user = db.FirstOrDefault<User>("select * from [user] where username = @0 or emailaddress = @0", UsernameOrEmailAddress);
            if (user == null)
                throw new Exception("User not found.");

            var config = new Configuration();
            string url = "{0}ResetPassword?code={1}".FormatStr(config.WebsiteAddress, Helpers.Encrypter.Encrypt("{0}:{1}:{2}".FormatStr(user.Username, user.EmailAddress, DateTime.UtcNow.Ticks)));

            Helpers.Emailer.Send(user.EmailAddress, "NextPVR Web Console Password Reset Request", Resources.Files.PasswordResetRequestBody.Replace("{Url}", url));
        }

        public void Save()
        {
            var db = DbHelper.GetDatabase();
            if (this.Oid > 0)
                db.Update(this);
            else
                db.Insert("user", "oid", true, this);
        }

        public static bool ValidateUser(string UsernameOrEmailAddress, string Password)
        {
            var db = DbHelper.GetDatabase();
            var user = db.FirstOrDefault<User>("select * from [user] where username = @0 or emailaddress = @0", UsernameOrEmailAddress);
            if (user == null)
                return false;
            return BCrypt.CheckPassword(Password, user.PasswordHash);
        }

        public bool ChangePassword(string OldPassword, string NewPassword)
        {
            if (!BCrypt.CheckPassword(OldPassword, this.PasswordHash))
                return false;
            return ChangePassword(NewPassword);
        }

        private bool ChangePassword(string NewPassword)
        {
            this.Password = NewPassword;
            var db = DbHelper.GetDatabase();
            return db.Update("user", "oid", new { passwordhash = this.PasswordHash }, this.Oid) > 0;
        }

        public static User CreateUser(string Username, string EmailAddress, string Password, UserRole UserRole, bool Administrator = false, DateTime? LastLoggedInUtc = null)
        {
            var db = DbHelper.GetDatabase();
            if (GetByUsername(Username) != null)
                throw new Exception("User already exists.");
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
                var channelGroups = ChannelGroup.LoadAll(Globals.SHARED_USER_OID);
                foreach(var cg in channelGroups)
                {
                    ChannelGroup cgNew = new ChannelGroup();
                    cgNew.UserOid = user.Oid;
                    cgNew.ParentOid = cg.Oid;
                    cgNew.Enabled = true;
                    cgNew.Name = "";
                    cgNew.OrderOid = cg.OrderOid;

                    db.Insert("channelgroup", "oid", true, cgNew);                    
                }

                // create default recording directory
                RecordingDirectory dir = new RecordingDirectory() { UserOid = user.Oid, Username = user.Username, IsDefault = true, Name = "Default", Path = "" };
                db.Insert("recordingdirectory", "oid", true, dir);

                user.DefaultRecordingDirectoryOid = dir.Oid;
                db.Save(user);

                db.CompleteTransaction();

                return user;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to create user: " + ex.Message + Environment.NewLine + ex.StackTrace);
                db.AbortTransaction();
                return null;
            }
        }

        public static List<User> LoadAll()
        {
            var db = DbHelper.GetDatabase();
            return db.Fetch<User>("select * from user where oid <> " + Globals.SHARED_USER_OID);
        }

        public static void Delete(int UserOid)
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

                db.CompleteTransaction();
            }
            catch (Exception ex)
            {
                db.AbortTransaction();
                throw ex;
            }

        }

        internal static User ValidateResetCode(string Code)
        {
            var config = new Configuration();
            string decrypted = Helpers.Encrypter.Decrypt(Code);
            string[] parts = decrypted.Split(':');
            if(parts.Length != 3)
                throw new Exception("Invalid code.");

            string username = parts[0];
            string email = parts[1];
            long dateticks = 0;
            if(!long.TryParse(parts[2], out dateticks))
                throw new Exception("Invalid code.");

            var user = GetByUsername(username);
            if (user == null || user.EmailAddress != email)
                throw new Exception("Invalid code.");

            DateTime code = new DateTime(dateticks);
            if (code > DateTime.UtcNow) // should never be greater than now, since THIS server must have generated this code in the past.
                throw new Exception("Invalid code.");
            if (code < DateTime.UtcNow.AddDays(-1))
                throw new Exception("Expired code.");
                
            // generate new password and send it to them
            string newPassword = Membership.GeneratePassword(12, 2);
            user.ChangePassword(newPassword);

            Helpers.Emailer.Send(user.EmailAddress, "NextPVR Web Console Password Reset", Resources.Files.PasswordResetBody.Replace("{Username}", user.Username).Replace("{Password}", newPassword).Replace("{Url}", config.WebsiteAddress));

            return user;
        }

        internal static void LoggedIn(string Username)
        {
            var user = GetByUsername(Username);
            user.LastLoggedInUtc = DateTime.UtcNow;
            user.Save();
        }
    }
}