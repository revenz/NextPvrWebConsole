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

        public string Identifier { get; set; }

        public List<Stream> Streams { get; set; }
        public static List<Device> GetDevices()
        {
            var devices = GetDevicesBasic();
            foreach (var d in devices)
            {
                foreach (var s in d.Streams)
                    s.LookupInfo(); // loads more info about the streams
            }
            return devices;
        }

        public static List<Device> GetDevicesBasic()
        {
            var instance = NShared.RecordingServiceProxy.GetInstance();
            string xml = instance.GetServerStatus();
            List<Device> devices = new List<Device>();
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
            try
            {
                XDocument doc = XDocument.Parse(xml);
                foreach (var element in doc.Element("Status").Elements("Device"))
                {
                    devices.Add(new Device()
                    {
                        Oid = int.Parse(element.Attribute("oid").Value),
                        Identifier = element.Attribute("identifier").Value,
                        Streams = (element.Elements("LiveTV") == null ? 
                                        new List<Stream>() :
                                        element.Elements("LiveTV").Select(x => new Stream(Stream.StreamType.LiveTV, int.Parse(element.Attribute("oid").Value), int.Parse(x.Attribute("handle").Value, System.Globalization.NumberStyles.HexNumber), x.Value))
                                   ).Union(
                                   element.Elements("Recording") == null ? 
                                        new List<Stream>() :
                                        element.Elements("Recording").Select(x => new Stream(Stream.StreamType.Recording, int.Parse(element.Attribute("oid").Value), int.Parse(x.Attribute("handle").Value, System.Globalization.NumberStyles.HexNumber), x.Value))
                                   ).ToList()
                                                    
                    });
                }
            }
            catch (Exception ex)
            {
                Hubs.NextPvrEventHub.Clients_ShowErrorMessage(ex.Message);
            }
            return devices;
        }

        public static bool StopStream(int Handle)
        {
            try
            {
                var instance = NShared.RecordingServiceProxy.GetInstance();
                instance.StopStream(Handle);
                return true;
            }
            catch (Exception) { return false; }
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
                        this.ChannelIcon = channel.Icon != null ? channel.Icon.ToBase64String() : null;
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
                var recording = NUtility.ScheduledRecording.LoadAll().Where(x => x.Filename == this.Filename).FirstOrDefault();
                if (recording != null)
                {
                    this.ChannelName = recording.ChannelName;
                    this.ChannelOid = recording.ChannelOID;
                    var channel = NUtility.Channel.LoadByOID(recording.ChannelOID);
                    if (channel != null)
                    {
                        this.ChannelNumber = channel.Number;
                        this.ChannelIcon = channel.Icon != null ? channel.Icon.ToBase64String() : null;
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

        public int CaptureSourceOid { get; set; }
        public int Handle { get; set; }

        public string Filename { get; set; }

        public StreamType Type { get; set; }

        public int ChannelNumber { get; set; }
        public int ChannelOid { get; set; }
        public string ChannelName { get; set; }
        public string ChannelIcon { get; set; }

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