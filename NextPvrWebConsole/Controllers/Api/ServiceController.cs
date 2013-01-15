﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;
using NextPvrWebConsole;
using System.Text.RegularExpressions;
using System.Text;
using NextPvrWebConsole.Models;

namespace NextPvrWebConsole.Controllers.Api
{
    /// <summary>
    /// This a a service end point NextPVR clients will talk to
    /// </summary>
    public class ServiceController : ApiController
    {
        private static Dictionary<string, int> SessionUserOids = new Dictionary<string, int>();
        private static Dictionary<string, string> SidsAndSalts = new Dictionary<string, string>();

        public HttpResponseMessage Get(string Method, string Ver = null, string Device = null, string Sid = null, string Md5 = null, int channel_id = 0, int start = 0, int end = 0, string group_id = null, string filter = null, int recording_id = 0, string name = null, int channel = 0, int time_t = 0, int duration = 0)
        {
            Logger.ServiceILog("Service Request: {0}, {1}", Method, Request.RequestUri.ToString());
            object response = new Response() { ErrorCode = 0, ErrorMessage = "Unknown method." };
            try
            {
                switch ((Method ?? "").ToLower())
                {
                    case "session.initiate": response = Session_Initiate(Ver, Device); break;
                    case "session.login": response = Session_Login(Sid, Md5); break;
                    case "channel.icon": response = Channel_Icon(channel_id); break;
                    default:
                    {
                        var config = new Models.Configuration();
                        int userOid = 0;
                        if (config.EnableUserSupport) /* ensure a user is found if users are enabled */
                        {
                            if (!String.IsNullOrWhiteSpace(Sid) && SessionUserOids.ContainsKey(Sid))
                                userOid = SessionUserOids[Sid];
                            else
                                throw new UnauthorizedAccessException();
                        }
                        switch ((Method ?? "").ToLower())
                        {
                            case "setting.list": response = Setting_List(); break;
                            case "channel.listings": response = Channel_Listings(userOid, channel_id, start, end); break;
                            case "channel.list": response = Channel_List(userOid, group_id); break;
                            case "channel.groups": response = Channel_Groups(userOid); break;
                            case "recording.list": response = Recording_List(userOid, filter); break;
                            case "recording.delete": response = Recording_Delete(userOid, recording_id); break;
                            case "recording.save": response = Recording_Save(userOid, name, channel, time_t, duration); break;
                        }
                    }
                    break;
                }
            }
            catch (InvalidSessionException)
            {
                response = new Response()
                {
                    ErrorCode = 8,
                    ErrorMessage = "Invalid Session",
                    Stat = Response.ResponseStat.fail /* NOTE: this is a "fail" response */
                };
            }
            catch (ChannelNotFoundException)
            {
                response = new Response()
                {
                    ErrorCode = 5,
                    ErrorMessage = "Channel not found",
                    Stat = Response.ResponseStat.failed /* NOTE: this is a "fail" response */
                };
            }
            if (response is Response)
                return new HttpResponseMessage() { Content = new StringContent(response.ToString(), System.Text.Encoding.UTF8, "application/xml") };
            else if (response == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            return response as HttpResponseMessage;
        }

        #region session stuff
        private Response Session_Initiate(string Version, string Device)
        {
            Logger.ServiceILog("ServiceController.Session_Initiate(\"{0}\", \"{1}\")", Version, Device);
            string sid = null;
            do
            {
                sid = Guid.NewGuid().ToString("N");
            } while (SidsAndSalts.ContainsKey(sid));
            string salt = Guid.NewGuid().ToString();

            SidsAndSalts.Add(sid, salt);

            // to do, get these from npvr somehow....
            return new Response()
            {
                Type= Response.ResponseType.SessionInitiate,
                Sid = sid,
                Salt = salt
            };
        }

        private Response Session_Login(string Sid, string Md5)
        {
            Logger.ServiceILog("ServiceController.Session_Login(\"{0}\", \"{1}\")", Sid, Md5);
            try
            {
                string salt = SidsAndSalts[Sid];
                // get the useroid 
                var db = DbHelper.GetDatabase();
                var users = Models.User.LoadAll();
                int userOid = 0;
                foreach (var user in users)
                {
                    if (CalculateMd5Hash(":{0}:{1}".FormatStr(CalculateMd5Hash(user.Username).ToLower(), salt)) == Md5)
                    {
                        if (SessionUserOids.ContainsKey(Sid))
                            SessionUserOids.Remove(Sid);
                        SessionUserOids.Add(Sid, user.Oid);
                        userOid = user.Oid;
                        break;
                    }
                }

                if (userOid == 0)
                    throw new Exception("Failed to locate user.");

                return new Response()
                {
                    Type = Response.ResponseType.SessionLogin,
                    Sid = Sid
                };
            }
            catch (Exception)
            {
                return new Response()
                {
                    ErrorCode = 1,
                    ErrorMessage = "Login Failed",
                    Stat = Response.ResponseStat.failed /* NOTE: this is a "failED" response */
                };
            }
        }

        private Response Setting_List()
        {
            try
            {
                Version npvrVersion = Models.NextPvrConfigHelper.NextPvrVersion;
                if (npvrVersion.ToString().StartsWith("2.5.9"))
                    npvrVersion = new Version(2, 5, 10, 0);
                int streamingPort = Models.NextPvrConfigHelper.WebServerPort;

                return new Response()
                {
                    Type = Response.ResponseType.SettingList,
                    Stat = Response.ResponseStat.ok,
                    Version = "70230000",
                    NextPvrVersion = "{0}{1}{2}".FormatStr(npvrVersion.Major.ToString("D2"), npvrVersion.Minor.ToString("D2"), npvrVersion.Build.ToString("D2")),
                    ChannelsUseSegmenter = false,
                    RecordingsUseSegmenter = true,
                    ChannelDetailsLevel = true,
                    StreamingPort = streamingPort
                };
            }
            catch (Exception)
            {
                throw new InvalidSessionException();
            }
        }
        #endregion

        #region channel stuff
        private Response Channel_Listings(int UserOid, int ChannelOid, long Start, long End)
        {
            Logger.ServiceILog("ServiceController.Channel_List({0}, {1}, \"{2}\", \"{3}\")", UserOid, ChannelOid, Start.FromUnixTime().ToString("yyyy-MM-ddTHH:mm:ss"), End.FromUnixTime().ToString("yyyy-MM-ddTHH:mm:ss"));
            // todo: authorization session...
            var channel = Models.Channel.Load(ChannelOid, UserOid);
            if(channel == null)
                throw new ChannelNotFoundException();
            var epgdata = Helpers.NpvrCoreHelper.GetListingsForTimePeriod(Start.FromUnixTime(), End.FromUnixTime()).Where(x => x.Key.OID == ChannelOid).Select(x => x.Value).FirstOrDefault().ToList();
            return new Response()
            {
                ChannelOid = ChannelOid,
                Listings = epgdata,
                Type = Response.ResponseType.ChannelListings,
                Stat = Response.ResponseStat.ok
            };
        }

        private Response Channel_List(int UserOid, string GroupName)
        {
            Logger.ServiceILog("ServiceController.Channel_List({0}, \"{1}\")", UserOid, GroupName);
            return new Response()
            {
                Channels = String.IsNullOrWhiteSpace(GroupName) ? Models.Channel.LoadAll(UserOid).ToList() : Models.Channel.LoadChannelsForGroup(UserOid, GroupName).ToList(),
                Stat = Response.ResponseStat.ok,
                Type = Response.ResponseType.ChannelList
            };
        }

        private object Channel_Icon(int ChannelOid)
        {
            Logger.ServiceILog("ServiceController.Channel_Icon({0})", ChannelOid);
            var channel = NUtility.Channel.LoadByOID(ChannelOid);
            if (channel == null || channel.Icon == null)
                return null;

            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                // have to construct a new Image, other saving the channel.Icon directly results in a "Generic GDI+ Exception" :)
                using (System.Drawing.Image target = new System.Drawing.Bitmap(channel.Icon.Width, channel.Icon.Height))
                {
                    using (var g = System.Drawing.Graphics.FromImage(target))
                    {
                        g.DrawImage(channel.Icon, 0, 0, target.Width, target.Height);
                    }

                    target.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Position = 0;
                    HttpResponseMessage response = new HttpResponseMessage();
                    response.Content = new ByteArrayContent(stream.ToArray());
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                    return response;
                }
            }
        }

        private Response Channel_Groups(int UserOid)
        {
            Logger.ServiceILog("ServiceController.Channel_Groups({0})", UserOid);
            return new Response()
            {
                Type = Response.ResponseType.ChannelGroups,
                Stat = Response.ResponseStat.ok,
                Groups = Models.ChannelGroup.LoadAll(UserOid).Where(x => x.Enabled).ToList()
            };
        }
        #endregion

        #region recoring stuff
        private Response Recording_List(int UserOid, string Filter)
        {
            Logger.ServiceILog("ServiceController.Recording_List({0}, \"{1}\")", UserOid, Filter);
            try
            {
                Dictionary<int, NUtility.RecurringRecording> reocurring = new Dictionary<int,NUtility.RecurringRecording>();
                foreach(var r in NUtility.RecurringRecording.LoadAll())
                    reocurring.Add(r.OID, r);
                List<Models.Recording> recordings = null;
                switch (Filter)
                {
                    case null:
                        {
                        }
                        break;
                    case "pending":
                        {
                            recordings = Models.RecordingGroup.Get(UserOid, IncludePending: true).SelectMany(x => x.Recordings).OrderBy(x => x.StartTime).ToList();
                        }
                        break;
                    case "ready":
                        {
                            recordings = Models.RecordingGroup.Get(UserOid, IncludeAvailable: true).SelectMany(x => x.Recordings).ToList();
                        }
                        break;
                }

                return new Response()
                {
                    Type = Response.ResponseType.RecordingList,
                    Stat = Response.ResponseStat.ok,
                    Recordings = recordings
                };

                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                throw new InvalidSessionException();
            }
        }

        private Response Recording_Delete(int UserOid, int RecordingId)
        {
            Logger.ServiceILog("ServiceController.Recording_Delete({0}, \"{1}\")", UserOid, RecordingId);
            NUtility.ScheduledRecording recording = NUtility.ScheduledRecording.LoadByOID(RecordingId);
            if (recording == null)
                return new Response() { Stat = Response.ResponseStat.ok };
            try
            {
                recording.Delete();
                return new Response() { Stat = Response.ResponseStat.ok };
            }
            catch (Exception)
            {
                throw new Exception("Failed to delete recording");
            }
        }

        private Response Recording_Save(int UserOid, string Name, int ChannelOid, long Time, int Duration)
        {
            Logger.ServiceILog("ServiceController.Recording_Save({0}, \"{1}\", {2}, \"{3}\", {4})", UserOid, Name, ChannelOid, Time.FromUnixTime().ToString("yyyy-MM-ddTHH:mm:ss"), Duration);
            DateTime start = Time.FromUnixTime();
            DateTime end = start.AddSeconds(Duration);
            var epgevent = NUtility.EPGEvent.LoadByNameAndTime(ChannelOid, Name, start, end);
            if (epgevent == null)
                return new Response() { Stat = Response.ResponseStat.failed, ErrorMessage = "Failed to locate EPG Event to schedule." };      
            if (Models.RecordingSchedule.QuickRecord(UserOid, epgevent.OID) == null)
                return new Response() { Stat = Response.ResponseStat.failed, ErrorMessage = "Failed to schedule recording." };                     
            return new Response();
        }
        #endregion

        class InvalidSessionException : Exception { }
        class ChannelNotFoundException : Exception { }

        public string CalculateMd5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        //private class MyScheduledRecording
        //{
        //    public NUtility.ScheduledRecording Recording { get; set; }
        //    public NUtility.RecurringRecording Recurring { get; set; }
        //}

        private class Response
        {
            public enum ResponseStat { ok, failed, fail }
            public enum ResponseType { General = 0, SessionInitiate = 1, SessionLogin, SettingList, ChannelListings, ChannelList, ChannelGroups, RecordingList }

            public ResponseStat Stat { get; set; }
            public string ErrorMessage { get; set; }
            public int ErrorCode { get; set; }
            public ResponseType Type { get; set; }

            public string Sid { get; set; }
            public string Salt { get; set; }

            public string Version { get; set; }
            public string NextPvrVersion { get; set; }
            public int StreamingPort { get; set; }
            public bool ChannelsUseSegmenter { get; set; }
            public bool RecordingsUseSegmenter { get; set; }
            public bool ChannelDetailsLevel { get; set; }
            public int ChannelOid { get; set; }
            public List<NUtility.EPGEvent> Listings { get; set; }
            public List<Models.Channel> Channels { get; set; }
            public List<Models.ChannelGroup> Groups { get; set; }
            //public List<MyScheduledRecording> Recordings { get; set; }
            public List<Models.Recording> Recordings { get; set; }

            public Response()
            {
                this.Stat = ResponseStat.ok;
                this.Type = ResponseType.General;
            }

            public override string ToString()
            {
                var root = new XElement("rsp", new XAttribute("stat", this.Stat.ToString()));
                if (Stat == ResponseStat.failed || Stat == ResponseStat.fail)
                    root.Add(new XElement("err", new XAttribute("code", ErrorCode), new XAttribute("msg", ErrorMessage ?? "")));
                else
                {
                    switch (this.Type)
                    {
                        case ResponseType.SessionInitiate: root.Add(new XElement("sid", this.Sid ?? ""), new XElement("salt", this.Salt ?? "")); break;
                        case ResponseType.SessionLogin: root.Add(new XElement("sid", this.Sid ?? "")); break;
                        case ResponseType.SettingList: root.Add(new XElement("Version", this.Version ?? ""), new XElement("NextPVRVersion", this.NextPvrVersion ?? ""), new XElement("ChannelsUseSegmenter", this.ChannelsUseSegmenter), new XElement("RecordingsUseSegmenter", this.RecordingsUseSegmenter), new XElement("ChannelDetailsLevel", this.ChannelDetailsLevel), new XElement("StreamingPort", this.StreamingPort)); break;
                        case ResponseType.ChannelListings:
                            {
                                XElement listings = new XElement("listings", new XElement("channel_id", this.ChannelOid));
                                listings.Add(this.Listings.Select(x => new XElement("l", new XElement("id", x.OID), new XElement("name", x.Title), new XElement("description", x.Description), new XElement("start", x.StartTime.ToUnixTime()), new XElement("end", x.EndTime.ToUnixTime()), new XElement("genre", x.EncodedGenres))));
                                root.Add(listings);
                            }
                            break;
                        case ResponseType.ChannelList:
                            {
                                //todo: change type from hardcoded 0x1 (which I assume is live tv...)
                                // 0xa == radio, any other value will be treated like channel
                                root.Add(new XElement("channels", this.Channels.Select(x => new XElement("channel", new XElement("id", x.Oid), new XElement("name", x.Name), new XElement("number", x.Number), new XElement("type", "0x1"), new XElement("icon", x.HasIcon.ToString().ToLower())))));
                            }
                            break;
                        case ResponseType.ChannelGroups:
                            {
                                root.Add(new XElement("groups", this.Groups.Select(x => new XElement("group", new XElement("id", x.Name), new XElement("name", x.Name)))));
                            }
                            break;
                        case ResponseType.RecordingList:
                            {
                                Func<NUtility.RecordingStatus, string> statusToString = delegate(NUtility.RecordingStatus status)
                                {
                                    switch(status){
                                        case NUtility.RecordingStatus.STATUS_PENDING: return "Pending";
                                    }
                                    return String.Empty;
                                };
                                Dictionary<int, NUtility.RecurringRecording> recurring = Helpers.NpvrCoreHelper.RecurringRecordingLoadAll().ToDictionary(x => x.OID);
                                root.Add(new XElement("recordings", this.Recordings.Select(x =>
                                            new XElement("recording",
                                                new XElement("id", x.OID),
                                                new XElement("recurring_parent", x.RecurrenceOid),
                                                new XElement("name", x.Name),
                                                new XElement("desc", x.Name), // this is a desc, but original service sets this to the same as the name...
                                                new XElement("start_time", TimeZone.CurrentTimeZone.ToLocalTime(x.StartTime).ToString("d/M/yyyy h:mm:ss tt")),
                                                new XElement("start_time_ticks", x.StartTime.ToUnixTime()),
                                                new XElement("duration", x.EndTime.Subtract(x.StartTime).ToString("hh':'mm")),
                                                new XElement("duration_seconds", ((int)x.EndTime.Subtract(x.StartTime).TotalSeconds)),
                                                new XElement("status", statusToString(x.Status)),
                                                new XElement("quality", x.Quality == null ? "QUALITY_DEFAULT" : x.Quality.ToString()),
                                                new XElement("channel", x.ChannelOID),
                                                new XElement("channel_id", x.ChannelOID),
                                                new XElement("recurring", x.RecurrenceOid > 0),
                                                new XElement("daymask", x.RecurrenceOid > 0 && recurring.ContainsKey(x.RecurrenceOid) ? recurring[x.RecurrenceOid].DayMask.ToString() : ""),
                                                new XElement("recurring_start", x.RecurrenceOid > 0 && recurring.ContainsKey(x.RecurrenceOid) ? recurring[x.RecurrenceOid].StartTime.ToString("d/M/yyyy h:mm:ss tt") : ""),
                                                new XElement("recurring_start_ticks", x.RecurrenceOid > 0 && recurring.ContainsKey(x.RecurrenceOid) ? recurring[x.RecurrenceOid].StartTime.Ticks.ToString() : ""),
                                                new XElement("recurring_end", x.RecurrenceOid > 0 && recurring.ContainsKey(x.RecurrenceOid) ? recurring[x.RecurrenceOid].EndTime.ToString("d/M/yyyy h:mm:ss tt") : ""),
                                                new XElement("recurring_end_ticks", x.RecurrenceOid > 0 && recurring.ContainsKey(x.RecurrenceOid) ? recurring[x.RecurrenceOid].EndTime.Ticks.ToString() : "")
                                                ))));

                            }
                            break;
                    }
                }

                XDocument doc = new XDocument(root);                
                StringBuilder builder = new StringBuilder();
                using (System.IO.TextWriter writer = new Utf8StringWriter(builder))
                {
                    doc.Save(writer);
                }
                string result = builder.ToString();
                result = result.Replace("\"?>", "\" ?>");
                return result + Environment.NewLine;
            }
        }

        public class Utf8StringWriter : System.IO.StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb) { }
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }
}
