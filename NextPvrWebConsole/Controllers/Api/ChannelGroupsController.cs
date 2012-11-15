using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class ChannelGroupsController : ApiController
    {
        // GET api/channelgroups
        public IEnumerable<Models.ChannelGroup> Get(bool LoadChannelOids = false)
        {
            return Models.ChannelGroup.LoadAll(this.GetUser().Oid, LoadChannelOids);
        }

        // GET api/channelgroups/getchannels
        [ActionName("Channels")]
        public IEnumerable<dynamic> GetChannels(int Oid)
        {
            var user = this.GetUser();
            var groupChannels = Models.ChannelGroup.LoadChannelOids(user.Oid, Oid);
            var allChannels = Models.Channel.LoadAll(user.Oid);
            return allChannels.Select(x => new 
            {
                Name = x.Name,
                Oid = x.Oid,
                Number = x.Number,
                Enabled = groupChannels.Contains(x.Oid)
            }).OrderBy(x => x.Number);
        }
        
        // POST api/channelgroup
        public bool Update(List<Models.ChannelGroup> Groups)
        {
            var user = this.GetUser();
            // validate request
            if (Groups.Where(x => !x.IsShared).DuplicatesBy(x => x.Name.ToLower().Trim()).Count() > 0)
                throw new ArgumentException("Channel Group names must be unique.");

            return Models.ChannelGroup.SaveForUser(user.Oid, Groups);
        }

        // DELETE api/channelgroup/5
        public bool Delete(int Oid)
        {
            var user = this.GetUser();
            if (user == null)
                throw new UnauthorizedAccessException();
            return Models.ChannelGroup.Delete(Oid, user.Oid);
        }
    }
}
