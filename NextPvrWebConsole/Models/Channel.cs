using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace NextPvrWebConsole.Models
{
    [DataContract]
	public class Channel
    {
        [DataMember]
        public int OID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Number { get; set; }
        [DataMember]
        [PetaPoco.Ignore]
        public string Icon { get; set; }
        //public List<ChannelMapping> Mappings { get; }

        [PetaPoco.Ignore]
        [DataMember]
        public List<NUtility.EPGEvent> Listings { get; set; }

        public Channel()
        {
        }

        public static List<Channel> LoadForTimePeriod(int UserOid, string GroupName, DateTime Start, DateTime End)
        {
            int[] channelOids = ChannelGroup.LoadChannelOids(UserOid, GroupName);

            // -12 hours from start to make sure we get data that starts earlier than start, but finishes after start
            var data = NUtility.EPGEvent.GetListingsForTimePeriod(Start.AddHours(-12), End);

            List<Channel> results = new List<Channel>();
            foreach (var key in data.Keys.Where(x => channelOids.Contains(x.OID)))
            {
                results.Add(new Channel() 
                {
                    Name = key.Name,
                    Number = key.Number,
                    OID = key.OID,
                    Icon = key.Icon != null ? key.Icon.ToBase64String() : null,

                    Listings = data[key].Where(x => x.EndTime > Start).ToList()
                });
            }
            return results;
        }

	}
}