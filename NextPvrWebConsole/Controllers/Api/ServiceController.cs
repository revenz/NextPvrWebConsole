using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;
using NextPvrWebConsole;
using System.Text.RegularExpressions;

namespace NextPvrWebConsole.Controllers.Api
{
    /// <summary>
    /// This a a service end point NextPVR clients will talk to
    /// </summary>
    public class ServiceController : ApiController
    {
        [HttpGet]
        public void Live()
        { 
            // Server.Transfer only works locally.... hmm...
            throw new NotImplementedException();
        }

        public HttpResponseMessage Get(string Method, string Ver = null, string Device = null, string Sid = null, string Md5 = null, int channel_id = 0, int start = 0, int end = 0, string group_id = null, string filter = null, int recording_id = 0, string name = null, int channel = 0, int time_t = 0, int duration = 0)
        {
            object response = new Response() { ErrorCode = 0, ErrorMessage = "Unknown method." };
            try
            {
                switch ((Method ?? "").ToLower())
                {
                    case "session.initiate": response = Session_Initiate(Ver, Device); break;
                    case "session.login": response = Session_Login(Sid, Md5); break;
                    case "setting.list": response = Setting_List(); break;
                    case "channel.listings": response = Channel_Listings(channel_id, start, end); break;
                    case "channel.list": response = Channel_List(group_id); break;
                    case "channel.icon": response = Channel_Icon(channel_id); break;
                    case "channel.groups": response = Channel_Groups(); break;
                    case "recording.list": response = Recording_List(filter); break;
                    case "recording.delete": response = Recording_Delete(recording_id); break;
                    case "recording.save": response = Recording_Save(name, channel, time_t, duration); break;
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
            // to do, get these from npvr somehow....
            return new Response()
            {
                Type= Response.ResponseType.SessionInitiate,
                Sid = Guid.NewGuid().ToString("N"),
                Salt = Guid.NewGuid().ToString()
            };
        }

        private Response Session_Login(string Sid, string Md5)
        {
            try
            {
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
                return new Response()
                {
                    Type = Response.ResponseType.SettingList,
                    Stat = Response.ResponseStat.ok,
                    Version = "70230000",
                    NextPvrVersion = "020509",
                    ChannelsUseSegmenter = false,
                    RecordingsUseSegmenter = true,
                    ChannelDetailsLevel = true
                };
            }
            catch (Exception)
            {
                throw new InvalidSessionException();
            }
        }
        #endregion

        #region channel stuff
        private Response Channel_Listings(int ChannelOid, long Start, long End)
        {
            // todo: authorization session...
            var channel = NUtility.Channel.LoadByOID(ChannelOid);
            if(channel == null)
                throw new ChannelNotFoundException();
            var epgdata = NUtility.EPGEvent.GetListingsForTimePeriod(Start.FromUnixTime(), End.FromUnixTime()).Where(x => x.Key.OID == ChannelOid).Select(x => x.Value).FirstOrDefault().ToList();
            return new Response()
            {
                ChannelOid = ChannelOid,
                Listings = epgdata,
                Type = Response.ResponseType.ChannelListings,
                Stat = Response.ResponseStat.ok
            };
            try
            {
            }
            catch (Exception)
            {
                throw new InvalidSessionException();
            }
        }

        private Response Channel_List(string Group)
        {
            return new Response()
            {
                Channels = NUtility.Channel.LoadForGroup(Group),
                Stat = Response.ResponseStat.ok,
                Type = Response.ResponseType.ChannelList
            };
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {
                throw new InvalidSessionException();
            }
        }

        private object Channel_Icon(int ChannelOid)
        {
            var channel = NUtility.Channel.LoadByOID(ChannelOid);
            if (channel == null || channel.Icon == null)
                return null;

            //using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                // cant dispose of this, otherwise the response is empty...
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                channel.Icon.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Position = 0;
                HttpResponseMessage response = new HttpResponseMessage() { Content = new StreamContent(stream) };
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                return response;
            }
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {
                throw new InvalidSessionException();
            }
        }

        private Response Channel_Groups()
        {
            return new Response()
            {
                Type = Response.ResponseType.ChannelGroups,
                Stat = Response.ResponseStat.ok,
                Groups = NUtility.Channel.GetChannelGroups()
            };

            try
            {
                throw new NotImplementedException();
            }
            catch (Exception)
            {
                throw new InvalidSessionException();
            }
        }
        #endregion

        #region recoring stuff
        private Response Recording_List(string Filter)
        {
            try
            {
                Dictionary<int, NUtility.RecurringRecording> reocurring = new Dictionary<int,NUtility.RecurringRecording>();
                foreach(var r in NUtility.RecurringRecording.LoadAll())
                    reocurring.Add(r.OID, r);
                List<MyScheduledRecording> recordings = null;
                switch (Filter)
                {
                    case null:
                        {
                        }
                        break;
                    case "pending":
                        {
                            recordings = (from r in NUtility.ScheduledRecording.LoadAll(true)
                                          select new MyScheduledRecording()
                                          {
                                              Recording = r,
                                              Recurring = r.RecurrenceOID > 0 && reocurring.ContainsKey(r.RecurrenceOID) ? reocurring[r.RecurrenceOID] : null
                                          }).ToList();
                        }
                        break;
                    case "ready":
                        {
                            recordings = (from r in NUtility.ScheduledRecording.LoadCompleted()
                                          select new MyScheduledRecording()
                                          {
                                              Recording = r,
                                              Recurring = r.RecurrenceOID > 0 && reocurring.ContainsKey(r.RecurrenceOID) ? reocurring[r.RecurrenceOID] : null
                                          }).ToList();
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
            catch (Exception)
            {
                throw new InvalidSessionException();
            }
        }

        private Response Recording_Delete(int RecordingId)
        {
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

        private Response Recording_Save(string Name, int ChannelOid, long Time, int Duration)
        {
            DateTime start = Time.FromUnixTime();
            DateTime end = start.AddSeconds(Duration);
            if (Models.ScheduledRecordingModel.BasicRecord(Name, ChannelOid, start, end) == null)
                return new Response() { Stat = Response.ResponseStat.failed, ErrorMessage = "Failed to schedule recording." };                     
            return new Response();
        }
        #endregion

        class InvalidSessionException : Exception { }
        class ChannelNotFoundException : Exception { }

        private class MyScheduledRecording
        {
            public NUtility.ScheduledRecording Recording { get; set; }
            public NUtility.RecurringRecording Recurring { get; set; }
        }

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
            public bool ChannelsUseSegmenter { get; set; }
            public bool RecordingsUseSegmenter { get; set; }
            public bool ChannelDetailsLevel { get; set; }
            public int ChannelOid { get; set; }
            public List<NUtility.EPGEvent> Listings { get; set; }
            public List<NUtility.Channel> Channels { get; set; }
            public List<string> Groups { get; set; }
            public List<MyScheduledRecording> Recordings { get; set; }

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
                        case ResponseType.SettingList: root.Add(new XElement("Version", this.Version ?? ""), new XElement("NextPVRVersion", this.NextPvrVersion ?? ""), new XElement("ChannelsUseSegmenter", this.ChannelsUseSegmenter), new XElement("RecordingsUseSegmenter", this.RecordingsUseSegmenter), new XElement("ChannelDetailsLevel", this.ChannelDetailsLevel)); break;
                        case ResponseType.ChannelListings:
                            {
                                XElement listings = new XElement("listings", new XElement("channel_id", this.ChannelOid));
                                listings.Add(this.Listings.Select(x => new XElement("l", new XElement("id", x.OID), new XElement("name", x.Title), new XElement("description", x.Description), new XElement("start", x.StartTime.ToUnixTime()), new XElement("end", x.EndTime.ToUnixTime()), new XElement("genre", x.EncodedGenres))));
                                root.Add(listings);
                            }
                            break;
                        case ResponseType.ChannelList:
                            {
                                //todo: change type from hardcoded 0x02 (which I assume is live tv...)
                                root.Add(new XElement("channels", this.Channels.Select(x => new XElement("channel", new XElement("id", x.OID), new XElement("name", x.Name), new XElement("number", x.Number), new XElement("type", "0x02")))));
                            }
                            break;
                        case ResponseType.ChannelGroups:
                            {
                                root.Add(new XElement("groups", this.Groups.Select(x => new XElement("group", new XElement("id", x), new XElement("name", x)))));
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
                                root.Add(new XElement("recordings", this.Recordings.Select(x =>
                                            new XElement("recording",
                                                new XElement("id", x.Recording.OID),
                                                new XElement("recurring_parent", x.Recording.RecurrenceOID),
                                                new XElement("name", x.Recording.Name),
                                                new XElement("desc", x.Recording.Name), // this is a desc, but original service sets this to the same as the name...
                                                new XElement("start_time", TimeZone.CurrentTimeZone.ToLocalTime(x.Recording.StartTime).ToString("d/M/yyyy h:mm:ss tt")),
                                                new XElement("start_time_ticks", x.Recording.StartTime.ToUnixTime()),
                                                new XElement("duration", x.Recording.EndTime.Subtract(x.Recording.StartTime).ToString("hh':'mm")),
                                                new XElement("duration_seconds", ((int)x.Recording.EndTime.Subtract(x.Recording.StartTime).TotalSeconds)),
                                                new XElement("status", statusToString(x.Recording.Status)),
                                                new XElement("quality", x.Recording.Quality.ToString()),
                                                new XElement("channel", x.Recording.ChannelOID),
                                                new XElement("channel_id", x.Recording.ChannelOID),
                                                new XElement("recurring", x.Recording.RecurrenceOID > 0),
                                                new XElement("daymask", x.Recurring != null ? x.Recurring.DayMask.ToString() : ""),
                                                new XElement("recurring_start", x.Recurring != null ? x.Recurring.StartTime.ToString("d/M/yyyy h:mm:ss tt") : ""),
                                                new XElement("recurring_start_ticks", x.Recurring != null ? x.Recurring.StartTime.Ticks.ToString() : ""),
                                                new XElement("recurring_end", x.Recurring != null ? x.Recurring.EndTime.ToString("d/M/yyyy h:mm:ss tt") : ""),
                                                new XElement("recurring_end_ticks", x.Recurring != null ? x.Recurring.EndTime.Ticks.ToString() : "")))));

                            }
                            break;
                    }
                }

                XDocument doc = new XDocument(root);
                return doc.ToString();
            }
        }
    }
}
