using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    [PetaPoco.PrimaryKey("Oid")]
    public class ChannelGroup
    {
        public int Oid { get; set; }
        public string Name { get; set; }
        public int OrderOid { get; set; }
        public int UserOid { get; set; }

        public static ChannelGroup GetById(int Oid)
        {
            var db = DbHelper.GetDatabase();
            return db.FirstOrDefault<ChannelGroup>("select * from channelgroup where oid = @0", Oid);
        }

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

        internal void Save(int[] ChannelOids = null)
        {
            var db = DbHelper.GetDatabase();
            this.Name = this.Name.Trim();
            // sanity checks
            if (String.IsNullOrWhiteSpace(this.Name))
                throw new ArgumentException("Name must not be blank.");
            if (db.FirstOrDefault<int>("select count(*) from channelgroup where useroid = @0 and name = @1 and oid <> @2", this.UserOid, this.Name, this.Oid) > 0)
                throw new ArgumentException("A group with the name '{0}' already exists.".FormatStr(this.Name));

            db.BeginTransaction();
            try
            {
                if (this.Oid == 0) // new group
                {
                    this.OrderOid = db.FirstOrDefault<int>("select isnull(max(orderoid), 0) + 1 from channelgroup where useroid = @0", this.UserOid);
                    db.Insert("channelgroup", "oid", true, this);
                    if (this.Oid < 1)
                        throw new Exception("Failed to inert channel group.");
                }
                else // update
                {
                    // only allow updating of the name here.. maybe add order too?
                    db.Update(this, this.Oid, new string[] { "name" });
                }

                if (ChannelOids != null)
                {
                    // insert the channels
                    db.Execute("delete from channelgroupchannel where channelgroupoid = @0", this.Oid);

                    foreach (int channeloid in ChannelOids)
                        db.Execute("insert into channelgroupchannel (channelgroupoid, channeloid) values (@0, @1)", this.Oid, channeloid);
                }

                db.CompleteTransaction();
            }
            catch (Exception ex)
            {
                db.AbortTransaction();
                throw ex;
            }
        }
    }
}