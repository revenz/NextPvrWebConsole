using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;

namespace NextPvrWebConsole.Models
{
    [DataContract]
	public class Channel
    {
        [DataMember]
        public string EPGMapping { get; set; }
        [DataMember]
        public string EPGSource { get; set; }
        [DataMember]
        public List<string> Groups { get; set; }
        [DataMember]
        public string Icon { get; set; }
        //public List<ChannelMapping> Mappings { get; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Number { get; set; }
        [DataMember]
        public int OID { get; set; }

        [DataMember]
        public List<NUtility.EPGEvent> Listings { get; set; }

        private Channel()
        {
        }

        public static List<Channel> LoadForTimePeriod(DateTime Start, DateTime End)
        {
            // -12 hours from start to make sure we get data that starts earlier than start, but finishes after start
            var data = NUtility.EPGEvent.GetListingsForTimePeriod(Start.AddHours(-12), End);
            List<Channel> results = new List<Channel>();
            foreach (var key in data.Keys)
            {
                results.Add(new Channel() 
                {
                    Name = key.Name,
                    Number = key.Number,
                    OID = key.OID,
                    EPGMapping = key.EPGMapping,
                    EPGSource = key.EPGSource,
                    Groups = key.Groups,
                    Icon = key.Icon != null ? key.Icon.ToBase64String() : null,

                    Listings = data[key].Where(x => x.EndTime > Start).ToList()
                });
            }
            return results;
        }
	}
}