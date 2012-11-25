using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using NUtility;

namespace NextPvrWebConsole.Helpers
{
    public class NpvrCoreHelper
    {
        private static NShared.RecordingServiceProxy GetRecordingServiceInstance()
        {
            return NShared.RecordingServiceProxy.GetInstance();
        }

        #region epg stuff
        public static void FlushEpgCache()
        {
            Helpers.Cacher.FlushCache(new System.Text.RegularExpressions.Regex("GetListingsForTimePeriod"));
        }

        public static Dictionary<NUtility.Channel, List<NUtility.EPGEvent>> GetListingsForTimePeriod(DateTime Start, DateTime End)
        {
            return Cacher.RetrieveOrStore<Dictionary<NUtility.Channel, List<NUtility.EPGEvent>>>("GetListingsForTimePeriod(" + Start.ToString() + "," + End.ToString() + ")", new TimeSpan(1, 0, 0), delegate
            {
                return NUtility.EPGEvent.GetListingsForTimePeriod(Start, End);
            });
        }
        public static EPGEvent EPGEventLoadByOID(int OID)
        {
            return NUtility.EPGEvent.LoadByOID(OID);
        }
        #endregion

        #region device/tuner stuff
        public static XDocument GetServerStatus()
        {
            var instance = GetRecordingServiceInstance();
            string xml = instance.GetServerStatus();
            return XDocument.Parse(xml);
        }

        public static void StopStream(int Handle)
        {
            GetRecordingServiceInstance().StopStream(Handle);
        }

        public static List<NShared.Visible.CaptureSource> CaptureSourceLoadAll()
        {
            return Cacher.RetrieveOrStore<List<NShared.Visible.CaptureSource>>("CaptureSource.LoadAll", new TimeSpan(1, 0, 0), delegate { return NShared.Visible.CaptureSource.LoadAll(); });
        }
        #endregion

        #region channel stuff
        public static List<NUtility.Channel> ChannelLoadAll()
        {
            return Cacher.RetrieveOrStore<List<NUtility.Channel>>("Channel.LoadAll", new TimeSpan(1, 0, 0), delegate { return NUtility.Channel.LoadAll(); });
        }

        public static NUtility.Channel ChannelLoadByOID(int OID)
        {
            return Cacher.RetrieveOrStore<NUtility.Channel>("Channel.LoadByOID(" + OID + ")", new TimeSpan(0, 10, 0), delegate { return NUtility.Channel.LoadByOID(OID); });
        }
        #endregion

        #region recording stuff
        public static void FlushRecordingsCache()
        {
            Helpers.Cacher.FlushCache(new System.Text.RegularExpressions.Regex("ScheduledRecording"));
        }
        public static List<NUtility.ScheduledRecording> ScheduledRecordingLoadAll()
        {
            // might not cache this.
            return Cacher.RetrieveOrStore<List<NUtility.ScheduledRecording>>("ScheduledRecording.LoadAll", new TimeSpan(0, 0, 5), delegate { return NUtility.ScheduledRecording.LoadAll(); });
        }
        public static NUtility.ScheduledRecording ScheduledRecordingLoadByOID(int OID)
        {
            return NUtility.ScheduledRecording.LoadByOID(OID);
        }
        public static void DeleteRecording(ScheduledRecording Recording)
        {
            GetRecordingServiceInstance().DeleteRecording(Recording);
        }
        #endregion
    }
}