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
            this.Password = NShared.SchedulesDirectEPGSource.GetStoredPassword();
            this.Enabled = !String.IsNullOrWhiteSpace(this.Username);
        }

        public bool Save()
        {
            return false;
        }

        public object Scan()
        {
            var source = new NShared.SchedulesDirectEPGSource();
            source.password = this.Password;
            source.username = this.Username;
            var results = source.GetLineups().Select(x =>
            {
                return new
                {
                    Name= x,
                    Oid = x,
                    Channels = source.GetChannelsForLineup(x).Select(y => y.mapping).ToArray()
                };
            });
            return results;
        }
    }
}