using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class GuideController : ApiController
    {
        // GET api/listings
        public IEnumerable<Models.Channel> Get(DateTime Date)
        {
            // round start to midnight today.
            DateTime start = new DateTime(Date.Year, Date.Month, Date.Day, 0, 0, 0);
            start = TimeZone.CurrentTimeZone.ToUniversalTime(start); // convert to utc
            return Models.Channel.LoadForTimePeriod(start, start.AddDays(1));
        }
    }
}
