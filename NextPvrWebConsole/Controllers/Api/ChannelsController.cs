using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    public class ChannelsController : ApiController
    {
        // GET api/channels
        public IEnumerable<Models.Channel> Get(bool IncludeDisabled = false)
        {
            return Models.Channel.LoadAll(this.GetUser().Oid, IncludeDisabled);
        }

        public void Update(Models.Channel[] Channels)
        {
            throw new NotImplementedException();
        }
    }
}
