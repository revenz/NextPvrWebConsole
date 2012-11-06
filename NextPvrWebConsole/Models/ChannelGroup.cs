using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public class ChannelGroup
    {
        public int Oid { get; set; }
        public string Name { get; set; }
        public int OrderOid { get; set; }
        public int UserOid { get; set; }

        public static ChannelGroup[] LoadAll(int UserOid)
        {
            var db = DbHelper.GetDatabase();
            return db.Fetch<ChannelGroup>("select * from channelgroup where useroid = @0 order by orderoid", UserOid).ToArray();
        }

        public int[] GetChannels()
        {
            var db = DbHelper.GetDatabase();
            return db.Fetch<int>("select channeloid from channelgroupchannel where channelgroupoid = @0", this.Oid).ToArray();
        }

        public static int[] LoadChannelOids(int UserOid, string GroupName)
        {
            var db = DbHelper.GetDatabase();
            return db.Fetch<int>("select channeloid from channelgroupchannel cgc inner join channelgroup cg on cgc.channelgroupoid = cg.oid where cg.useroid = @0 and cg.name = @1", UserOid, GroupName).ToArray();
        }
    }
}