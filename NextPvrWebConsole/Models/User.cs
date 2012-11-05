using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}