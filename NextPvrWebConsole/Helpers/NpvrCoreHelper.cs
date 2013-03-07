using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using NUtility;
using System.Threading;

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
            return Cacher.RetrieveOrStore<Dictionary<NUtility.Channel, List<NUtility.EPGEvent>>>("GetListingsForTimePeriod(" + Start.ToString() + "," + End.ToString() + ")", new TimeSpan(0, 0, 30), delegate
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

        #region recurring
        internal static void CancelRecurring(int RecurrenceOID)
        {
            var instance = GetRecordingServiceInstance();
            instance.CancelRecurring(RecurrenceOID);
        }

        #endregion

        internal static RecurringRecording RecurringRecordingLoadByOID(int RecurrenceOID)
        {
            return RecurringRecording.LoadByOID(RecurrenceOID);
        }
        internal static List<RecurringRecording> RecurringRecordingLoadAll()
        {
            return RecurringRecording.LoadAll();
        }

        internal static void ArchiveRecording(int ScheduledRecordingOid, string RecordingDirectoryId)
        {
            var recording = ScheduledRecordingLoadByOID(ScheduledRecordingOid);
            if (recording != null)
            {
                GetRecordingServiceInstance().ArchiveRecording(recording, RecordingDirectoryId);
            }
        }

        #region EPG stuff
        internal static void EmptyEpg()
        {
            Logger.ILog("Emptying EPG");
            NShared.EPGManager manager = new NShared.EPGManager();
            manager.EmptyEPG();
            FlushEpgCache();
        }

        static Mutex EpgUpdateMutex = new Mutex();

        internal static void UpdateEpg(Action<string> CallBack = null)
        {
            Logger.ILog("Update EPG Started");
            NUtility.PluginRegistry.GetInstance().LoadPlugins();
            NShared.RecordingServiceProxy.ForceRemote();
            ScheduleHelperFactory.SetScheduleHelper(GetRecordingServiceInstance());
            NShared.EPGManager manager = new NShared.EPGManager();
            WebConsoleEpgUpdateCallback wcCallback = new WebConsoleEpgUpdateCallback(CallBack);
            System.Threading.Tasks.Task.Factory.StartNew(delegate
            {
                if (!EpgUpdateMutex.WaitOne(100))
                {
                    Logger.ELog("Failed to update EPG, already running.");
                    return;
                }
                try
                {
                    Hubs.NextPvrEventHub.Clients_ShowInfoMessage("EPG Update Started");
                    manager.UpdateEPG(wcCallback);
                    Hubs.NextPvrEventHub.Clients_ShowInfoMessage("EPG Update Completed");
                }
                catch (Exception ex)
                {
                    Hubs.NextPvrEventHub.Clients_ShowErrorMessage(ex.Message, "EPG Update Failed");
                    wcCallback.SetEPGUpdateStatus("ERROR: " + ex.Message);
                }
                finally
                {
                    System.Threading.Thread.Sleep(10000);
                    EpgUpdateMutex.ReleaseMutex();
                }
            });
        }
        #endregion
    }

    class WebConsoleEpgUpdateCallback : NShared.EPGManager.IEPGUpdateCallback
    {
        private Action<string> CallBack;

        public WebConsoleEpgUpdateCallback(Action<string> CallBack = null)
        {
            this.CallBack = CallBack;
        }

        public bool SetEPGUpdateStatus(string status)
        {
            Logger.ILog("Updating EPG Status: " + status);
            if (CallBack != null)
                CallBack(status);
            return true;
        }
    }
}