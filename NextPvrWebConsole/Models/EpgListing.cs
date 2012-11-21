using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace NextPvrWebConsole.Models
{
    public class EpgListing
    {
        public string Aspect { get; set; }
        public string Audio { get; set; }
        public int ChannelOid { get; set; }
        public string Description { get; set; }
        public DateTime EndTime { get; set; }
        public int Episode { get; set; }
        public bool FirstRun { get; set; }
        public List<string> Genres { get; set; }
        public int Oid { get; set; }
        public DateTime OriginalAirDate { get; set; }
        public string Rating { get; set; }
        public int Season { get; set; }
        public string StarRating { get; set; }
        public DateTime StartTime { get; set; }
        public string Subtitle { get; set; }
        public string Title { get; set; }

        public bool IsRecording { get; set; }
        public bool IsRecurring { get; set; }
        public int PrePadding { get; set; }
        public int PostPadding { get; set; }
        public int Keep { get; set; }
        public string RecordingDirectoryId { get; set; }
        public RecordingType RecordingType { get; set; }
        public int RecordingOid { get; set; }


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

        internal static List<SearchResult> Search(int UserOid, string SearchText)
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
                var listings = NUtility.EPGEvent.GetListingsForTimePeriod(start, end).Where(x => channels.ContainsKey(x.Key.OID));

                Regex searchPattern = new Regex(Regex.Replace(Regex.Replace(SearchText, @"[^\w\d *]", ""), @"[\s]+", @"[\s]+").Replace("*", "(.*?)"), RegexOptions.IgnoreCase);
                foreach (var listing in listings.SelectMany(x => x.Value))
                {
                    if (searchPattern.IsMatch(Regex.Replace(listing.Title, @"[^\w\d\s*]", "")))
                        results.Add(new SearchResult(channels[listing.ChannelOID], new EpgListing(listing), 1));
                    else if (searchPattern.IsMatch(Regex.Replace(listing.Subtitle, @"[^\w\d\s*]", "")))
                        results.Add(new SearchResult(channels[listing.ChannelOID], new EpgListing(listing), 2));
                    else if (searchPattern.IsMatch(Regex.Replace(listing.Description, @"[^\w\d\s*]", "")))
                        results.Add(new SearchResult(channels[listing.ChannelOID], new EpgListing(listing), 3));
                }
                return results.OrderBy(x => x.Weighting).ThenBy(x => x.Listing.StartTime).ToList();
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
