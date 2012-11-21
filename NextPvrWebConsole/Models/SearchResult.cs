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
        public int Weighting { get; set; }

        public SearchResult(Channel Channel, EpgListing Listing, int Weighting)
        {
            this.Channel = Channel;
            this.Listing = Listing;
            this.Weighting = Weighting;
        }
    }
}