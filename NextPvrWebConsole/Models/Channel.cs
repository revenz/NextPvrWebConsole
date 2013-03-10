﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace NextPvrWebConsole.Models
{
    [DataContract]
    [PetaPoco.PrimaryKey("Oid")]
	public class Channel
    {
        [DataMember]
        public int Oid { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Number { get; set; }
        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        [PetaPoco.Ignore]
        public bool HasIcon { get; set; }

        [PetaPoco.Ignore]
        [DataMember]
        public List<EpgListing> Listings { get; set; }

        [DataMember]
        [PetaPoco.Ignore]
        public string EpgSource { get; set; }
        [DataMember]
        [PetaPoco.Ignore]
        public string XmlTvChannel { get; set; }

        public Channel()
        {
        }

        public static List<Channel> LoadForTimePeriod(int UserOid, string GroupName, DateTime Start, DateTime End)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Logger.Log("Channel load for time period.");
            Dictionary<int, Channel> channels = (String.IsNullOrWhiteSpace(GroupName) ? Channel.LoadAll(UserOid, false) : Channel.LoadChannelsForGroup(UserOid, GroupName)).ToDictionary(x => x.Oid);
            // -12 hours from start to make sure we get data that starts earlier than start, but finishes after start
            var data = Helpers.NpvrCoreHelper.GetListingsForTimePeriod(Start.AddHours(-12), End);
            var listingData = data.Where(x => channels.ContainsKey(x.Key.OID)).SelectMany(x => x.Value);
            Logger.Log("Time[0]: " + timer.Elapsed.ToString());
            

            // load here to pass into loadepglisting            

            var userRdDefault = RecordingDirectory.LoadUserDefault(UserOid);
            var listings = EpgListing.LoadEpgListings(UserOid, channels.Keys.ToArray(), listingData, userRdDefault);
            Logger.Log("Time[6]: " + timer.Elapsed.ToString());
            Dictionary<int, List<EpgListing>> listingResults = new Dictionary<int, List<EpgListing>>();
            foreach (var listing in listings)
            {
                if (!listingResults.ContainsKey(listing.ChannelOid))
                    listingResults[listing.ChannelOid] = new List<EpgListing>();
                listingResults[listing.ChannelOid].Add(listing);
            }
            Logger.Log("Time[7]: " + timer.Elapsed.ToString());
            
            List<Channel> results = new List<Channel>();
            foreach (var key in data.Keys.Where(x => channels.Keys.Contains(x.OID)))
            {
                results.Add(new Channel() 
                {
                    Name = key.Name,
                    Number = channels[key.OID].Number,
                    Oid = key.OID,
                    HasIcon = key.Icon != null,
                    Listings = listingResults.ContainsKey(key.OID) ? listingResults[key.OID] : null
                });
            }
            Logger.Log("Time[8]: " + timer.Elapsed.ToString());
            timer.Stop();
            return results.OrderBy(x => x.Number).ToList();
        }

        public static Channel[] LoadAll(int UserOid, bool IncludeDisabled = false)
        {
            return Helpers.Cacher.RetrieveOrStore<Channel[]>("Channel.LoadAll(" + UserOid + "," + IncludeDisabled  +")", new TimeSpan(0, 1, 0), delegate
            {
                var db = DbHelper.GetDatabase();
                List<Channel> results = null;
                if (UserOid == Globals.SHARED_USER_OID || !new Configuration().EnableUserSupport)
                {
                    results = db.Fetch<Channel>(@"select * from channel order by number");
                }
                else
                {
                    results = db.Fetch<Channel>(@"
select c.oid, c.name, uc.*
from userchannel uc
inner join channel c on uc.channeloid = c.oid and c.enabled = 1 and uc.useroid = @0
order by uc.number", UserOid);
                }
                if (IncludeDisabled)
                    return LoadHasIcon(results.ToArray());
                return LoadHasIcon(results.Where(x => x.Enabled).ToArray());
            });
        }

        internal static Channel[] LoadChannelsForGroup(int UserOid, string GroupName)
        {
            var db = DbHelper.GetDatabase();
            return LoadHasIcon(db.Fetch<Channel>(@"
select c.oid, c.name, uc.*
from channelgroup cg
inner join channelgroupchannel cgc on cg.oid = cgc.channelgroupoid
inner join channel c on cgc.channeloid = c.oid
inner join userchannel uc on c.oid = uc.channeloid
where c.enabled = 1 and uc.enabled = 1 and uc.useroid = @0 and cg.name = @1", UserOid, GroupName).ToArray());
        }

        internal static Channel Load(int ChannelOid, int UserOid)
        {
            var db = DbHelper.GetDatabase();
            var channel = db.FirstOrDefault<Channel>("select c.oid, c.name, uc.* from channel c inner join userchannel uc on c.oid = uc.channeloid where c.oid = @0 and uc.useroid = @1 and c.enabled = 1", ChannelOid, UserOid);
            return channel == null ? null : LoadHasIcon(new Channel[] { channel })[0];
        }

        internal static Channel Load(int ChannelOid)
        {
            var db = DbHelper.GetDatabase();
            var channel = db.FirstOrDefault<Channel>("select * from channel c where oid = @0", ChannelOid);
            return channel == null ? null : LoadHasIcon(new Channel[] { channel })[0];
        }

        private static Channel[] LoadHasIcon(Channel[] Channels)
        {
            var temp = Helpers.Cacher.RetrieveOrStore<Dictionary<int, NUtility.Channel>>("NUtility.Channel.LoadAll.ToDictionary", new TimeSpan(1, 0, 0), delegate { return NUtility.Channel.LoadAll().ToDictionary(x => x.OID); });
            foreach (var channel in Channels)
                channel.HasIcon = temp.ContainsKey(channel.Oid) && temp[channel.Oid].Icon != null;
            return Channels;
        }

        internal static void Update(int UserOid, Channel[] Channels)
        {
            var db = DbHelper.GetDatabase();
            try
            {
                db.BeginTransaction();
                int[] allowedChanneldOids = db.Fetch<int>("select oid from channel where [enabled] = 1").ToArray();
                foreach (var channel in Channels.Where(x => allowedChanneldOids.Contains(x.Oid)))
                    db.Execute("update userchannel set number = @0, [enabled] = @1 where channeloid = @2 and useroid = @3", channel.Number, channel.Enabled, channel.Oid, UserOid);
                db.CompleteTransaction();

                // flush the cacher
                Helpers.Cacher.FlushCache(new Regex(@"^Channel\.[^(]+\(" + UserOid));
            }
            catch (Exception ex)
            {
                db.AbortTransaction();
                throw ex;
            }
        }

        internal static bool SaveForUser(int UserOid, List<Channel> Channels)
        {
            var db = DbHelper.GetDatabase();
            db.BeginTransaction();
            try
            {
                if (UserOid == Globals.SHARED_USER_OID)
                {
                    // delete any missing channels
                    int[] knownOids = db.Fetch<int>("select oid from [channel]").ToArray();
                    db.Execute("delete from [userchannel] where channeloid not in ({0})".FormatStr(String.Join(",", Channels.Where(x => x.Oid > 0).Select(x => x.Oid.ToString()).ToArray())));
                    db.Execute("delete from [channel] where oid not in ({0})".FormatStr(String.Join(",", Channels.Where(x => x.Oid > 0).Select(x => x.Oid.ToString()).ToArray())));
                    foreach (var channel in Channels)
                    {
                        if (String.IsNullOrWhiteSpace(channel.Name))
                            throw new Exception("Channel name required.");
                        if (channel.Number < 0 || channel.Number > 1000)
                            throw new Exception("Channel number must be in the range 0 to 999.");
                        if (knownOids.Contains(channel.Oid))
                            db.Update(channel);
                        else
                            db.Insert("channel", "oid", false, channel);

                        // update epg source in NextPVR
                        var nChannel = NUtility.Channel.LoadByOID(channel.Oid);
                        if (channel.EpgSource.ToLower().StartsWith("xmltv-"))
                        {
                            int xmltvId = 0;
                            if(int.TryParse(channel.EpgSource.Substring(6), out xmltvId)){
                                var xmltvSource = Models.XmltvSource.LoadByOid(xmltvId);
                                if (xmltvSource != null)
                                {
                                    var xmltvChannel = xmltvSource.Channels.Where(x => x.Oid == channel.XmlTvChannel).FirstOrDefault();
                                    if (xmltvChannel != null)
                                    {
                                        nChannel.EPGMapping = @"<epg>
  <source>XMLTV</source>
  <file>{0}</file>
  <mapping_id>{1}</mapping_id>
  <mapping_name>{2}</mapping_name>
</epg>".FormatStr(xmltvSource.Filename, xmltvChannel.Oid, xmltvChannel.Name);

                                        nChannel.EPGSource = "XMLTV";
                                        nChannel.Save();
                                    }
                                }
                            }
                        }
                        else if (channel.EpgSource.ToUpper() == "DVB/ATSC EPG")
                        {
                            NUtility.ChannelMapping channelMapping = null;
                            foreach(var device in Models.Device.LoadAll().Where(x => x.Enabled))
                            {
                                channelMapping = nChannel.GetChannelMapping(device.Oid);
                                if(channelMapping != null)
                                    break;
                            }
                            if (channelMapping != null)
                            {
                                nChannel.EPGMapping = @"<epg>
  <source>DVB/ATSC EPG</source>
  <capture_source>{0}</capture_source>
  <readable_source>{1} {2}</readable_source>
  <mapping>{1}</mapping>
</epg>".FormatStr(channelMapping.captureSourceOID, "{0}:{1}:{2}".FormatStr(channelMapping.SID, channelMapping.TSID, channelMapping.ONID), nChannel.Name);
                                nChannel.EPGSource = "DVB/ATSC EPG";
                                nChannel.Save();
                            }
                        }
                        else if (channel.EpgSource.ToUpper() == "NONE")
                        {
                            nChannel.EPGSource = "None";
                            nChannel.EPGMapping = "";
                            nChannel.Save();
                        }
                        else if (channel.EpgSource.ToUpper() == "SCHEDULES DIRECT")
                        {
                            nChannel.EPGSource = "Schedules Direct";
                            nChannel.EPGMapping = "";
                            nChannel.Save();
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
                db.CompleteTransaction();
                Helpers.Cacher.FlushCache(new Regex(@"^Channel\."));
                return true;
            }
            catch (Exception ex)
            {
                db.AbortTransaction();
                throw ex;
            }
        }

        internal static System.Drawing.Image LoadIcon(int Oid)
        {
            var channel = NUtility.Channel.LoadByOID(Oid);
            if (channel == null)
                return null;
            return channel.Icon.ToIconSize();
        }
    }
}