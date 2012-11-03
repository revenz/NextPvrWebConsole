using System;
using System.Collections.Generic;
using System.Linq;
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
            var instance = NShared.RecordingServiceProxy.GetInstance();
            string xml = instance.GetServerStatus();
            List<Device> devices = new List<Device>();
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
                                        element.Elements("LiveTV").Select(x => new Stream(Stream.StreamType.LiveTV, int.Parse(x.Attribute("handle").Value, System.Globalization.NumberStyles.HexNumber), x.Value))
                                   ).Union(
                                   element.Elements("Recording") == null ? 
                                        new List<Stream>() :
                                        element.Elements("Recording").Select(x => new Stream(Stream.StreamType.Recording, int.Parse(x.Attribute("handle").Value, System.Globalization.NumberStyles.HexNumber), x.Value ))
                                   ).ToList()
                                                    
                    });
                }
            }
            catch (Exception) { }
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
        public Stream(StreamType Type, int Handle, string Filename)
        {
            this.Type = Type;
            this.Handle = Handle;
            this.Filename = Filename;

            //var instance = NShared.RecordingServiceProxy.GetInstance();
            

        }
        public int Handle { get; set; }

        public string Filename { get; set; }

        public StreamType Type { get; set; }

        public enum StreamType
        {
            LiveTV = 1,
            Recording = 2
        }
    }
}