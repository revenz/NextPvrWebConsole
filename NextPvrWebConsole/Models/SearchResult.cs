using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public class SearchResult
    {
        public Channel Channel { get; set; }
        public EpgListing Listing { get; set; }

        public SearchResult(Channel Channel, EpgListing Listing)
        {
            this.Channel = Channel;
            this.Listing = Listing;
        }
    }
}