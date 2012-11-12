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
        [PetaPoco.Ignore]
        public int[] ChannelOids { get; set; }

        public static ChannelGroup GetById(int Oid)
        {
            var db = DbHelper.GetDatabase();
            return db.FirstOrDefault<ChannelGroup>("select * from channelgroup where oid = @0", Oid);
        }

        public static List<ChannelGroup> LoadAll(int UserOid, bool LoadChannelOids = false)
        {
            var db = DbHelper.GetDatabase();
            var results = db.Fetch<ChannelGroup>("select * from channelgroup where useroid = @0 order by orderoid", UserOid);
            if (LoadChannelOids)
            {
                foreach (var r in results)
                    r.ChannelOids = r.GetChannels();
            }
            return results;
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

        public static int[] LoadChannelOids(int UserOid, int Oid)
        {
            var db = DbHelper.GetDatabase();
            return db.Fetch<int>("select channeloid from channelgroupchannel cgc inner join channelgroup cg on cgc.channelgroupoid = cg.oid where cg.useroid = @0 and cg.oid = @1", UserOid, Oid).ToArray();
        }

        internal void Save(int[] ChannelOids = null)
        {
            var db = DbHelper.GetDatabase();
            this.Name = this.Name.Trim();

            db.BeginTransaction();
            try
            {
                Save(db, ChannelOids);

                db.CompleteTransaction();
            }
            catch (Exception ex)
            {
                db.AbortTransaction();
                throw ex;
            }
        }

        private void Save(PetaPoco.Database db, int[] ChannelOids = null)
        {
            this.Name = this.Name.Trim();
            // sanity checks
            if (String.IsNullOrWhiteSpace(this.Name))
                throw new ArgumentException("Name must not be blank.");
            if (db.FirstOrDefault<int>("select count(*) from channelgroup where useroid = @0 and name = @1 and oid <> @2", this.UserOid, this.Name, this.Oid) > 0)
                throw new ArgumentException("A group with the name '{0}' already exists.".FormatStr(this.Name));

            if (this.Oid == 0) // new group
            {
                this.OrderOid = db.FirstOrDefault<int>("select ifnull(max(orderoid),0) + 1 from channelgroup where useroid = @0", this.UserOid);
                db.Insert("channelgroup", "oid", true, this);
                if (this.Oid < 1)
                    throw new Exception("Failed to inert channel group.");
            }
            else // update
            {
                // only allow updating of the name here.. maybe add order too?
                db.Update(this, this.Oid, new string[] { "name", "orderoid" });
            }

            if (ChannelOids != null)
            {
                // insert the channels
                db.Execute("delete from channelgroupchannel where channelgroupoid = @0", this.Oid);

                foreach (int channeloid in ChannelOids)
                    db.Execute("insert into channelgroupchannel (channelgroupoid, channeloid) values (@0, @1)", this.Oid, channeloid);
            }

        }

        internal static bool Delete(int Oid, int UserOid)
        {
            var db = DbHelper.GetDatabase();

            var group = GetById(Oid);
            if(group == null)
                return true; // nothing to delete
            if (group.UserOid != UserOid)
                throw new UnauthorizedAccessException(); // can't delete someone elses group

            db.BeginTransaction();
            try
            {
                db.Execute("delete from channelgroupchannel where channelgroupoid = @0", group.Oid);
                db.Delete(group);

                db.CompleteTransaction();
                return true;
            }
            catch (Exception)
            {
                db.AbortTransaction();
                return false;
            }
        }

        internal static bool SaveForUser(int UserOid, List<ChannelGroup> ChannelGroups)
        {
            if (ChannelGroups.DuplicatesBy(x => x.Name.ToLower()).Count() > 0)
                throw new Exception("Channel group names must be unique.");

            var db = DbHelper.GetDatabase();
            db.BeginTransaction();
            try
            {
                db.Execute("delete from channelgroupchannel where channelgroupoid in (select oid from channelgroup where useroid = {0} and channelgroupoid not in ({1}))".FormatStr(UserOid, String.Join(",", ChannelGroups.Where(x => x.Oid > 0).Select(x => x.Oid.ToString()))));
                db.Execute("delete from channelgroup where useroid = {0} and oid not in ({1})".FormatStr(UserOid, String.Join(",", ChannelGroups.Where(x => x.Oid > 0).Select(x => x.Oid.ToString()))));
                foreach (var cg in ChannelGroups)
                {
                    cg.Save(db, cg.ChannelOids ?? new int[] { });
                }

                db.CompleteTransaction();
                return true;
            }
            catch (Exception ex)
            {
                db.AbortTransaction();
                return false;
            }
        }
    }
}