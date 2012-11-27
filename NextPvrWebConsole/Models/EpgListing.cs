using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NextPvrWebConsole.Models
{
    public class EpgListing
    {
        public int Oid { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int ChannelOid { get; set; }
        public int RecordingOid { get; set; }
        public bool IsRecording { get; set; }
        public bool IsRecurring { get; set; }
        public int RecurrenceOid { get; set; }

        public int ChannelNumber { get; set; }
        public string ChannelName { get; set; }

        public string Aspect { get; set; }
        public string Audio { get; set; }
        public string Description { get; set; }
        public int Episode { get; set; }
        public bool FirstRun { get; set; }
        public List<string> Genres { get; set; }
        public DateTime OriginalAirDate { get; set; }
        public string Rating { get; set; }
        public int Season { get; set; }
        public string StarRating { get; set; }
        public string Subtitle { get; set; }

        public int PrePadding { get; set; }
        public int PostPadding { get; set; }
        public int Keep { get; set; }
        public string RecordingDirectoryId { get; set; }
        public RecordingType RecordingType { get; set; }

        public EpgListing(NUtility.EPGEvent EpgEvent)
        {
            this.Aspect = EpgEvent.Aspect;
            this.Audio = EpgEvent.Audio;
            this.ChannelOid = EpgEvent.ChannelOID;
            this.Description = EpgEvent.Description;
            this.EndTime = EpgEvent.EndTime;
            this.Episode = EpgEvent.Episode;
            this.FirstRun = EpgEvent.FirstRun;
            this.Genres = EpgEvent.Genres;
            this.Oid = EpgEvent.OID;
            this.OriginalAirDate = EpgEvent.OriginalAirDate;
            this.Rating = EpgEvent.Rating;
            this.Season = EpgEvent.Season;
            this.StarRating = EpgEvent.StarRating;
            this.StartTime = EpgEvent.StartTime;
            this.Subtitle = EpgEvent.Subtitle;
            this.Title = EpgEvent.Title;
        }

        public static List<EpgListing> LoadEpgListings(int UserOid, int[] ChannelOids, IEnumerable<NUtility.EPGEvent> Data, RecordingDirectory UserDefault = null)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            Logger.Log("Loading EPG Listings [0]: " + timer.Elapsed);
            var config = new Configuration();
            if(UserDefault == null)
                UserDefault = RecordingDirectory.LoadUserDefault(UserOid);

            var allowedRecordings = EpgRecordingData.LoadAllowedRecordings(UserOid);
            Logger.Log("Loading EPG Listings [2]: " + timer.Elapsed);

            timer.Stop();
            return Data.Select(x =>
            {
                var listing = new EpgListing(x);
                if (allowedRecordings.ContainsKey(x.OID))
                {
                    listing.PrePadding = allowedRecordings[x.OID].PrePadding;
                    listing.PostPadding = allowedRecordings[x.OID].PostPadding;
                    listing.RecordingDirectoryId = allowedRecordings[x.OID].RecordingDirectoryId;
                    listing.Keep = allowedRecordings[x.OID].Keep;
                    listing.IsRecurring = allowedRecordings[x.OID].IsRecurring;
                    listing.RecordingType = allowedRecordings[x.OID].RecordingType;
                    listing.RecordingOid = allowedRecordings[x.OID].RecordingOid;
                    listing.IsRecording = true;
                }
                else
                {
                    listing.PrePadding = config.PrePadding;
                    listing.PostPadding = config.PostPadding;
                    listing.RecordingDirectoryId = UserDefault == null ? null : UserDefault.RecordingDirectoryId;
                }
                return listing;
            }).ToList();
        }

        internal static List<SearchResult> Search(int UserOid, string SearchText, int MaximumResults = 50)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            try
            {
                List<SearchResult> results = new List<SearchResult>();
                DateTime start = DateTime.Now;
                start = new DateTime(start.Year, start.Month, start.Day, 0, 0, 0).ToUniversalTime();
                DateTime end = start.AddDays(30);
                // get channels available for user
                var channels = Channel.LoadAll(UserOid).ToDictionary(x => x.Oid);
                // get listings, filtered out by the channels they have access to.
                var listings = Helpers.NpvrCoreHelper.GetListingsForTimePeriod(start, end).Where(x => channels.ContainsKey(x.Key.OID));

                // create a indexed list with priority of search matches 
                Regex searchPattern = new Regex(Regex.Replace(Regex.Replace(SearchText, @"[^\w\d *]", ""), @"[\s]+", @"[\s]+").Replace("*", "(.*?)"), RegexOptions.IgnoreCase);
                Dictionary<int, List<NUtility.EPGEvent>> listings2 = new Dictionary<int, List<NUtility.EPGEvent>>();
                listings2.Add(1, new List<NUtility.EPGEvent>());
                listings2.Add(2, new List<NUtility.EPGEvent>());
                listings2.Add(3, new List<NUtility.EPGEvent>());
                foreach (NUtility.EPGEvent listing in listings.SelectMany(x => x.Value))
                {
                    if (searchPattern.IsMatch(Regex.Replace(listing.Title, @"[^\w\d\s*]", "")))
                        listings2[1].Add(listing); //results.Add(new SearchResult(channels[listing.ChannelOID], new EpgListing(listing), 1));
                    else if (searchPattern.IsMatch(Regex.Replace(listing.Subtitle, @"[^\w\d\s*]", "")))
                        listings2[2].Add(listing); //results.Add(new SearchResult(channels[listing.ChannelOID], new EpgListing(listing), 2));
                    else if (searchPattern.IsMatch(Regex.Replace(listing.Description, @"[^\w\d\s*]", "")))
                        listings2[3].Add(listing); //results.Add(new SearchResult(channels[listing.ChannelOID], new EpgListing(listing), 3));                    
                }

                // now pull all the epgevents into one list
                var listings3 = listings2.SelectMany(x => x.Value).ToList();
                
                // now load the complete EpgList object for the list
                return EpgListing.LoadEpgListings(UserOid, channels.Keys.ToArray(), listings3).Take(MaximumResults).Select(x => new SearchResult(channels[x.ChannelOid], x)).ToList();
            }
            finally
            {
                timer.Stop();
                // dont log the search text, keep that private, but do log how long it took
                Logger.Log("Search took {0}", timer.Elapsed);
            }
        }
    }
}
