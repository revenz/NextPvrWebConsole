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

        //[HttpGet]
        //public bool SaveChannelGroup(int Oid, string Name, string Channels)
        //{
        //    var user = this.GetUser();
        //    int[] channelOids = (Channels ?? "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
        //    var channelGroup = new Models.ChannelGroup() { UserOid = user.Oid };
        //    if (Oid != 0)
        //    {
        //        channelGroup = Models.ChannelGroup.GetById(Oid);
        //        if (channelGroup == null || channelGroup.UserOid != user.Oid)
        //            throw new ArgumentException();
        //    }
        //    channelGroup.Name = Name;
        //    channelGroup.Save(channelOids);
        //    return true;
        //}


        // POST api/channelgroup
        public void Update(List<Models.ChannelGroup> Groups)
        {
            var user = this.GetUser();
            // validate request
            if (Groups.Where(x => !x.IsShared).DuplicatesBy(x => x.Name.ToLower().Trim()).Count() > 0)
                throw new ArgumentException("Channel Group names must be unique.");

            Models.ChannelGroup.SaveForUser(user.Oid, Groups);
        }

        // DELETE api/channelgroup/5
        public void Delete(int Oid)
        {
            var user = this.GetUser();
            if (user == null)
                throw new UnauthorizedAccessException();
            Models.ChannelGroup.Delete(Oid, user.Oid);
        }
    }
}
