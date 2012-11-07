using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    public class ChannelGroupsController : ApiController
    {
        // GET api/channelgroups
        public IEnumerable<Models.ChannelGroup> Get()
        {
            return Models.ChannelGroup.LoadAll(this.GetUser().Oid);
        }

        // GET api/channelgroups/getchannels
        public IEnumerable<dynamic> GetChannels(string GroupName)
        {
            var user = this.GetUser();
            var groupChannels = Models.ChannelGroup.LoadChannelOids(user.Oid, GroupName);
            var allChannels = Models.Channel.LoadAll(user.Oid);

            return allChannels.Select(x => new 
            {
                Name = x.Name,
                Oid = x.Oid,
                Number = x.Number,
                Enabled = groupChannels.Contains(x.Oid)
            }).OrderBy(x => x.Number);
        }

        [HttpGet]
        public bool SaveChannelGroup(int Oid, string Name, string Channels)
        {
            var user = this.GetUser();
            int[] channelOids = (Channels ?? "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
            var channelGroup = new Models.ChannelGroup() { UserOid = user.Oid };
            if (Oid != 0)
            {
                channelGroup = Models.ChannelGroup.GetById(Oid);
                if (channelGroup == null || channelGroup.UserOid != user.Oid)
                    throw new ArgumentException();
            }
            channelGroup.Name = Name;
            channelGroup.Save(channelOids);
            return true;
        }
    }
}
