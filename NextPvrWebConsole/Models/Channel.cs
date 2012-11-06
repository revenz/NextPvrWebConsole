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
        public int Oid { get; set; }
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
                    Oid = key.OID,
                    Icon = key.Icon != null ? key.Icon.ToBase64String() : null,

                    Listings = data[key].Where(x => x.EndTime > Start).ToList()
                });
            }
            return results;
        }


        internal static Channel[] LoadAll(int UserOid)
        {
            var db = DbHelper.GetDatabase();
            return db.Fetch<Channel>("select c.* from channel c inner join userchannel uc on c.oid = uc.channeloid where uc.useroid = @0", UserOid).ToArray();
        }

        internal static Channel[] LoadChannelsForGroup(int UserOid, string GroupName)
        {
            var db = DbHelper.GetDatabase();
            return db.Fetch<Channel>("select c.* from channelgroup cg inner join channelgroupchannel cgc on cg.oid = cgc.channelgroupoid inner join channel c on cgc.channeloid = c.oid inner join userchannel uc on c.oid = uc.channeloid where uc.useroid = @0 and cg.name = @1", UserOid, GroupName).ToArray();
        }

        internal static Channel Load(int ChannelOid, int UserOid)
        {
            var db = DbHelper.GetDatabase();
            return db.FirstOrDefault<Channel>("select c.* from channel c innser join userchannel uc on c.oid = uc.channeloid where c.oid = @0 and uc.useroid = @1", ChannelOid, UserOid);
        }

        internal static Channel Load(int ChannelOid)
        {
            var db = DbHelper.GetDatabase();
            return db.FirstOrDefault<Channel>("select * from channel c where oid = @0", ChannelOid);

        }
    }
}