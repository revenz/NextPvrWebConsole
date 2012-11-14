using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace NextPvrWebConsole.Models
{
    public class Device
    {
        public int Oid { get; set; }
        public string Name { get; set; }

        public string SourceType { get; set; }
        public int Priority { get; set; }
        public bool Present { get; set; }
        public bool Enabled { get; set; }
        public int NumberOfChannels { get; set; }
        
        public List<Stream> Streams { get; set; }
        public static List<Device> GetDevices()
        {
            var devices = GetDevicesBasic();
            foreach (var d in devices)
            {
                if (d.Streams != null)
                {
                    foreach (var s in d.Streams)
                        s.LookupInfo(); // loads more info about the streams
                }
            }
            return devices;
        }

        public static List<Device> LoadAll()
        {
            List<Device> devices = new List<Device>();

            var captureSources = NShared.Visible.CaptureSource.LoadAll().OrderBy(x => x.Priority);
            foreach (var cs in captureSources)
            {                
                devices.Add(new Device()
                {
                    SourceType = cs.SourceType,
                    Priority = cs.Priority,
                    Present = cs.Present,
                    Oid = cs.OID,
                    Name = cs.Name,
                    Enabled = cs.Enabled,
                    NumberOfChannels = NUtility.Channel.LoadForCaptureSource(cs.OID).Count
                });
            }
            return devices;
        }

        public static List<Device> GetDevicesBasic()
        {
            Dictionary<int, Device> devices = LoadAll().ToDictionary(x => x.Oid);

            try
            {
                string xml = "";
#if(DEBUG)
                try
                {
#endif
                    var instance = NShared.RecordingServiceProxy.GetInstance();
                    xml = instance.GetServerStatus();
#if(DEBUG)
                }
                catch (Exception) { }
#endif
#if(DEBUG)
                if (String.IsNullOrEmpty(xml))
                {
                    xml = @"<Status>
      <Device oid=""20"" identifier=""TBS 6984 Quad DVBS/S2 Tuner A:1"">
        <Recording handle=""140010"">D:\Videos\NPVR Recordings\TV Shows\Movie Clash Of The Titans\Movie Clash Of The Titans_20121104_20302235.ts</Recording>
        <Recording handle=""140011"">D:\Videos\NPVR Recordings\TV Shows\MGM Premiere Dune\MGM Premiere Dune_20121104_20302245.ts</Recording>
      </Device>
      <Device oid=""29"" identifier=""TBS 6984 Quad DVBS/S2 Tuner B:1"">
      </Device>
      <Device oid=""30"" identifier=""TBS 6984 Quad DVBS/S2 Tuner C:1"">
        <LiveTV handle=""1E0023"">LIVE&amp;R:\LiveTV\live-PRIME-2772-22.ts</LiveTV>
      </Device>
      <Device oid=""31"" identifier=""TBS 6984 Quad DVBS/S2 Tuner D:1"">
        <LiveTV handle=""1F002B"">LIVE&amp;R:\LiveTV\live-TCM-10752.ts</LiveTV>
      </Device>
    </Status>";
                }
#endif
                XDocument doc = XDocument.Parse(xml);
                foreach (var element in doc.Element("Status").Elements("Device"))
                {
                    int oid = int.Parse(element.Attribute("oid").Value);
                    if (devices.ContainsKey(oid))
                    {
                        devices[oid].Streams = (element.Elements("LiveTV") == null ?
                                                    new List<Stream>() :
                                                    element.Elements("LiveTV").Select(x => new Stream(Stream.StreamType.LiveTV, int.Parse(x.Attribute("handle").Value, System.Globalization.NumberStyles.HexNumber), int.Parse(element.Attribute("oid").Value), x.Value))
                                               ).Union(
                                               element.Elements("Recording") == null ?
                                                    new List<Stream>() :
                                                    element.Elements("Recording").Select(x => new Stream(Stream.StreamType.Recording, int.Parse(x.Attribute("handle").Value, System.Globalization.NumberStyles.HexNumber), int.Parse(element.Attribute("oid").Value), x.Value))
                                               ).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                Hubs.NextPvrEventHub.Clients_ShowErrorMessage(ex.Message);
            }
            return devices.Values.ToList();
        }

        public static bool StopStream(int Handle)
        {
            try
            {
                var instance = NShared.RecordingServiceProxy.GetInstance();

                var stream = (from s in GetDevices().SelectMany(x => x.Streams)
                              where s.Handle == Handle
                              select s).FirstOrDefault();
                if (stream == null)
                    return true; // noting to stop

                if (stream.Type == Stream.StreamType.LiveTV)
                {
                    instance.StopStream(Handle);
                    System.Threading.Thread.Sleep(1000);
                    // check if handle exists
                    return !(GetDevices().Where(x => x.Streams.Where(y => y.Handle == Handle).Count() > 0).Count() > 0);
                }
                else if (stream.Type == Stream.StreamType.Recording)
                {
                    // can't just force an abort, have to stop the stream gracefully.      
                    var recording = stream.LoadRecordingDetails();
                    if (recording == null)
                        throw new Exception("Failed to locate recording details.");
                    RecordingSchedule.CancelRecording(recording.OID);

                }
                throw new Exception("Unknown stream type.");
            }
            catch (Exception) { return false; }
        }

        internal bool Save()
        {
            NShared.Visible.CaptureSource cs = NShared.Visible.CaptureSource.LoadAll().Where(x => x.OID == this.Oid).FirstOrDefault();
            if (cs == null)
                return false;

            cs.Priority = this.Priority;
            cs.Enabled = this.Enabled;
            cs.Save();
            return true;
        }
    }
    
    public class Stream
    {
        public Stream(StreamType Type, int Handle, int CaptureSourceOid, string Filename)
        {
            this.Type = Type;
            this.Handle = Handle;
            this.CaptureSourceOid = CaptureSourceOid;
            this.Filename = Filename;
        }

        private string GetChannelLookupName(string Input)
        {
            return String.Join("", Input.ToArray().Where(y => !System.IO.Path.GetInvalidFileNameChars().Contains(y)).ToArray()).ToUpper().Replace(" ", "");
        }

        internal void LookupInfo()
        {
            if (Type == StreamType.LiveTV)
            {
                string channelName = Regex.Match(Filename, "(?<=(live-))[^-]+").Value;
                if (!String.IsNullOrEmpty(channelName))
                {
                    string lookupName = GetChannelLookupName(channelName);
                    var channel = NUtility.Channel.LoadAll().Where(x => GetChannelLookupName(x.Name) == lookupName).FirstOrDefault();
                    if (channel != null)
                    {
                        this.ChannelHasIcon = channel.Icon != null;
                        this.ChannelName = channel.Name;
                        this.ChannelNumber = channel.Number;
                        this.ChannelOid = channel.OID;
                        NUtility.EPGEvent epg = NUtility.EPGEvent.GetListingsForTimePeriod(DateTime.UtcNow.AddHours(-6), DateTime.UtcNow.AddHours(1))
                                                              .Where(x => x.Key.OID == channel.OID)
                                                              .Select(x => x.Value.Where(y => y.EndTime > DateTime.UtcNow && y.StartTime < DateTime.UtcNow).FirstOrDefault()).FirstOrDefault();
                        if (epg != null)
                        {
                            this.Title = epg.Title;
                            this.Subtitle = epg.Subtitle;
                            this.Description = epg.Description;
                            this.StartTime = epg.StartTime;
                            this.EndTime = epg.EndTime;
                        }
                    }
                }
            }
            else if (Type == StreamType.Recording)
            {
                var recording = LoadRecordingDetails();
                if (recording != null)
                {
                    this.ChannelName = recording.ChannelName;
                    this.ChannelOid = recording.ChannelOID;
                    var channel = NUtility.Channel.LoadByOID(recording.ChannelOID);
                    if (channel != null)
                    {
                        this.ChannelNumber = channel.Number;
                        this.ChannelHasIcon = channel.Icon != null;
                    }
                    NUtility.EPGEvent epg = NUtility.EPGEvent.LoadByOID(recording.EventOID);
                    if (epg != null)
                    {
                        this.Title = epg.Title;
                        this.Subtitle = epg.Subtitle;
                        this.Description = epg.Description;
                        this.StartTime = epg.StartTime;
                        this.EndTime = epg.EndTime;
                    }
                }
                
            }
        }

        public NUtility.ScheduledRecording LoadRecordingDetails()
        {
            return NUtility.ScheduledRecording.LoadAll().Where(x => x.Filename == this.Filename).FirstOrDefault();
        }

        public int CaptureSourceOid { get; set; }
        public int Handle { get; set; }

        public string Filename { get; set; }

        public StreamType Type { get; set; }

        public int ChannelNumber { get; set; }
        public int ChannelOid { get; set; }
        public string ChannelName { get; set; }
        public bool ChannelHasIcon { get; set; }

        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }



        public enum StreamType
        {
            LiveTV = 1,
            Recording = 2
        }
    }
}