using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public class ChannelGroup
    {
        public string Name { get; set; }

        public static ChannelGroup[] LoadAll()
        {
            return (from c in NUtility.Channel.GetChannelGroups()
                    where !String.IsNullOrWhiteSpace(c)
                    select new ChannelGroup() { Name = c }).ToArray();
        }
    }
}