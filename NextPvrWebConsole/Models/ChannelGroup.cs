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
        public bool Enabled { get; set; }
        public int ParentOid { get; set; }
        [PetaPoco.Ignore]
        public int[] ChannelOids { get; set; }
        [PetaPoco.Ignore]
        public bool IsShared { get { return this.ParentOid > 0; } }

        public static ChannelGroup GetById(int Oid)
        {
            var db = DbHelper.GetDatabase();
            return db.FirstOrDefault<ChannelGroup>("select * from channelgroup where oid = @0", Oid);
        }

        public static List<ChannelGroup> LoadAll(int UserOid, bool LoadChannelOids = false)
        {
            var db = DbHelper.GetDatabase();
            var results = db.Fetch<ChannelGroup>(
                UserOid == Globals.SHARED_USER_OID ? 
                "select * from channelgroup where useroid = @0 order by orderoid"  
                :
@"select
    cg1.oid as oid, cg1.orderoid as orderoid, cg1.parentoid as parentoid,
    case when cg1.parentoid > 0 then cg2.name else cg1.name end as name
    , cg1.enabled as enabled
from channelgroup cg1
left outer join channelgroup cg2
on cg1.parentoid = cg2.oid and cg2.useroid = @1
where cg1.useroid = @0

union

select 0 as oid, 9999999 as orderoid, oid, name, 0 as enabled
from channelgroup
where useroid = @1 and oid not in (select parentoid from channelgroup where useroid = @0)


order by orderoid
", UserOid, Globals.SHARED_USER_OID);            
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

        public static ChannelGroup GetByName(int UserOid, string GroupName)
        {
            var db = DbHelper.GetDatabase();
            var channelGroup = db.FirstOrDefault<ChannelGroup>("select * from channelgroup where useroid = @0 and name = @1", UserOid, GroupName);
            if (channelGroup != null)
                return channelGroup;
            // check for a shared group with that name
            return db.FirstOrDefault<ChannelGroup>("select cg1.* from channelgroup cg1 inner join channelgroup cg2 on cg1.oid = cg2.parentoid where cg2.useroid = @0 and cg1.name = @1", UserOid, GroupName);
        }

        public static int[] LoadChannelOids(int UserOid, string GroupName)
        {
            var channelGroup = GetByName(UserOid, GroupName);
            if (channelGroup == null)
                return new int[] { };

            var db = DbHelper.GetDatabase();
            bool userSupport = new Configuration().EnableUserSupport;
            string sql = "select cgc.channeloid from channelgroupchannel cgc inner join channel c on cgc.channeloid = c.oid where c.enabled = 1 and channelgroupoid = @0";
            if (userSupport)
            {
                // need to filter out their disabled channels
                sql = "select cgc.channeloid from channelgroupchannel cgc inner join channel c on cgc.channeloid = c.oid inner join userchannel uc on cgc.channeloid = uc.channeloid where c.enabled = 1 and uc.enabled = 1 and channelgroupoid = @0 and uc.useroid = " + UserOid;
            }
            return db.Fetch<int>(sql, channelGroup.Oid).ToArray();
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
            if (!IsShared)
            {
                this.Name = this.Name.Trim();
                // sanity checks
                if (String.IsNullOrWhiteSpace(this.Name))
                    throw new ArgumentException("Name must not be blank.");
                if (db.FirstOrDefault<int>("select count(*) from channelgroup where useroid = @0 and name = @1 and oid <> @2", this.UserOid, this.Name, this.Oid) > 0)
                    throw new ArgumentException("A group with the name '{0}' already exists.".FormatStr(this.Name));
            }

            if (this.Oid == 0) // new group
            {
                this.OrderOid = db.FirstOrDefault<int>("select ifnull(max(orderoid),0) + 1 from channelgroup where useroid = @0", this.UserOid);
                if (this.IsShared)
                {
                    // special case, insert missing shared, so need to make sure name isnt inserted as it comes from the parent channel group
                    this.Name = ""; 

                }
                db.Insert("channelgroup", "oid", true, this);
                
                if (this.Oid < 1)
                    throw new Exception("Failed to insert channel group.");
            }
            else // update
            {
                // only allow updating of the name here.. maybe add order too?
                if(IsShared)
                    db.Update(this, this.Oid, new string[] { "orderoid", "enabled" });
                else
                    db.Update(this, this.Oid, new string[] { "name", "orderoid" });
            }

            if (ChannelOids != null && !IsShared)
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
            if (ChannelGroups.Where(x => !x.IsShared || UserOid == Globals.SHARED_USER_OID).DuplicatesBy(x => x.Name.ToLower()).Count() > 0)
                throw new ArgumentException("Channel group names must be unique.");
            if (ChannelGroups.Where(x => x.Name.ToLower() == Globals.ALL_CHANNELS_GROUP_NAME.ToLower()).Count() > 0)
                throw new ArgumentException("Cannot create a Channel Group '{0}' as it is reserved.".FormatStr(Globals.ALL_CHANNELS_GROUP_NAME));

            var db = DbHelper.GetDatabase();
            db.BeginTransaction();
            try
            {
                db.Execute("delete from channelgroupchannel where channelgroupoid in (select oid from channelgroup where useroid = {0} and channelgroupoid not in ({1}))".FormatStr(UserOid, String.Join(",", ChannelGroups.Where(x => x.Oid > 0).Select(x => x.Oid.ToString()))));
                db.Execute("delete from channelgroup where useroid = {0} and parentoid < 1 and oid not in ({1})".FormatStr(UserOid, String.Join(",", ChannelGroups.Where(x => x.Oid > 0).Select(x => x.Oid.ToString()))));
                int orderOid = 0;
                foreach (var cg in ChannelGroups)
                {
                    cg.UserOid = UserOid;
                    cg.OrderOid = orderOid++;
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