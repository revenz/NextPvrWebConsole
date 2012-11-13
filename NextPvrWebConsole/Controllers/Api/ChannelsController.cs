using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class ChannelsController : ApiController
    {
        // GET api/channels
        public IEnumerable<Models.Channel> Get(bool IncludeDisabled = false)
        {
            return Models.Channel.LoadAll(this.GetUser().Oid, IncludeDisabled);
        }

        public void Update(Models.Channel[] Channels)
        {
            Models.Channel.Update(this.GetUser().Oid, Channels);
        }

        public IEnumerable<Models.Channel> GetMissingChannels()
        {
            int[] knownChannels = Models.Channel.LoadAll(Globals.SHARED_USER_OID, true).Select(x => x.Oid).OrderBy(x => x).ToArray();
            var npvrChannels = NUtility.Channel.LoadAll();
            return npvrChannels.Where(x => !knownChannels.Contains(x.OID)).OrderBy(x => x.Number).Select(x => new Models.Channel() { Oid = x.OID, Name = x.Name, Number = x.Number, Enabled = true }).OrderBy(x => x.Oid);
        }

    }
}
