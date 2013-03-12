using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public class SchedulesDirectModel
    {
        public bool Enabled { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public SchedulesDirectModel()
        {
            this.Username = NShared.SchedulesDirectEPGSource.GetStoredUsername();
            this.Enabled = !String.IsNullOrWhiteSpace(this.Username);
            if(this.Enabled)
                this.Password = "        ";
        }

        public bool Save()
        {
            string username = this.Enabled ? this.Username : null;
            string password = this.Enabled && this.Password != "        " ? this.Password : null; 
            new NShared.SchedulesDirectEPGSource().UpdateCache(username, password);
            return true;
        }

        public object Scan()
        {
            var source = new NShared.SchedulesDirectEPGSource();
            source.password = this.Password;
            source.username = this.Username;
            if (String.IsNullOrWhiteSpace(source.username))
                source.username = NShared.SchedulesDirectEPGSource.GetStoredPassword();

            var results = source.GetLineups().Select(x =>
            {
                return new
                {
                    Name = x,
                    Oid = x,
                    Channels = source.GetChannelsForLineup(x).Select(y => new { Oid = y.description, Name = y.description }).ToArray()
                };
            });
            return results;
        }
    }
}