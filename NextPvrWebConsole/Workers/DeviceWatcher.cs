using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Web;

namespace NextPvrWebConsole.Workers
{
    public class DeviceWatcher
    {
        public delegate void TunerStatusUpdated_EventHandler(DeviceUpdateEvent[] Event);
        public event TunerStatusUpdated_EventHandler TunerStatusUpdated;

        List<NextPvrWebConsole.Models.Device> Devices;

        private Mutex mutex = new Mutex();

        System.Timers.Timer _Timer;
        public DeviceWatcher()
        {
        }

        public void Start()
        {
            // load initial devices
            Devices = NextPvrWebConsole.Models.Device.GetDevicesBasic();

            _Timer = new System.Timers.Timer(5 * 1000);
            _Timer.AutoReset = true;
            _Timer.Elapsed += _Timer_Elapsed;
            _Timer.Start();
        }

        public void Stop()
        {
            if(_Timer != null)
                _Timer.Stop();
            _Timer = null;
        }

        void _Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (TunerStatusUpdated == null)
                return; // no need running if nothing is watching.

            if(!mutex.WaitOne(1))
                return;

            List<DeviceUpdateEvent> events = Updated();
            if (events.Count > 0 && TunerStatusUpdated != null) // just incase the only watcher stopped watching
            {
                TunerStatusUpdated(events.ToArray());
            }

            mutex.ReleaseMutex();
        }

        List<DeviceUpdateEvent> Updated()
        {
            var newDevices = Models.Device.GetDevicesBasic();
            try
            {
                // currently can't detect a channel change, they appear as stream stop / start
                // this is because a change might be on a different tuner, and no way to tell what client 
                // requested the channel change.
                // however we can probably do a best guess and say if theres only 1 start and 1 stop that it was
                // a channel change.
                // also not sure what happens when a live tv buffer file roles over, will appear as a stream change...
                List<DeviceUpdateEvent> events = new List<DeviceUpdateEvent>();

                // check if a device have been removed
                foreach (var d in Devices)
                    if (newDevices.Where(x => x.Oid == d.Oid).Count() == 0)
                        events.Add(new DeviceUpdateEvent() { Code = DeviceUpdateEventCode.DeviceRemoved, Message = d.Name });
                // check if a device have been added
                foreach (var d in newDevices)
                    if (Devices.Where(x => x.Oid == d.Oid).Count() == 0)
                        events.Add(new DeviceUpdateEvent() { Code = DeviceUpdateEventCode.DeviceAdded, Message = d.Name });

                // check if a stream has been stopped
                foreach (var d in Devices)
                {
                    var newD = newDevices.Where(x => x.Oid == d.Oid).FirstOrDefault();
                    if (newD == null)
                    {
                        // was removed, so all streams stopped.
                        foreach (var s in d.Streams)
                        {
                            events.Add(new DeviceUpdateEvent()
                            {
                                Code = s.Type == Models.Stream.StreamType.LiveTV ? DeviceUpdateEventCode.LiveStream_Stopped : DeviceUpdateEventCode.Recording_Stopped,
                                Message = s.Filename
                            });
                        }
                        continue;
                    }
                    if (d.Streams != null)
                    {
                        foreach (var s in d.Streams)
                        {
                            if (newD.Streams.Where(x => x.Filename == s.Filename).Count() == 0)
                                events.Add(new DeviceUpdateEvent() { Code = s.Type == Models.Stream.StreamType.LiveTV ? DeviceUpdateEventCode.LiveStream_Stopped : DeviceUpdateEventCode.Recording_Stopped, Message = s.Filename });
                        }
                    }
                }

                // check if a stream has started
                foreach (var d in newDevices)
                {
                    var oldD = Devices.Where(x => x.Oid == d.Oid).FirstOrDefault();
                    if (oldD == null)
                    {
                        // new device, so all devices are new
                        foreach (var s in d.Streams)
                        {
                            events.Add(new DeviceUpdateEvent()
                            {
                                Code = s.Type == Models.Stream.StreamType.LiveTV ? DeviceUpdateEventCode.LiveStream_Started : DeviceUpdateEventCode.Recording_Started,
                                Message = s.Filename
                            });
                        }
                        continue;
                    }

                    if (d.Streams != null)
                    {
                        foreach (var s in d.Streams)
                        {
                            if (oldD.Streams == null || oldD.Streams.Where(x => x.Filename == s.Filename).Count() == 0)
                                events.Add(new DeviceUpdateEvent() { Code = s.Type == Models.Stream.StreamType.LiveTV ? DeviceUpdateEventCode.LiveStream_Started : DeviceUpdateEventCode.Recording_Started });
                        }
                    }
                }

                return events;

            }
            finally
            {
                Devices = newDevices;
            }
        }
    }

    public enum DeviceUpdateEventCode
    {
        DeviceAdded = 100, 
        DeviceRemoved = 101,
        LiveStream_Started = 200,
        LiveStream_Stopped = 201,
        LiveStream_Updated = 202, /* occurs when a new show starts on the same channel */
        Recording_Started = 300,
        Recording_Stopped = 301,
    }

    public class DeviceUpdateEvent
    {
        public DeviceUpdateEventCode Code { get; set; }
        public string Message { get; set; }
        public string CodeString
        {
            get
            {
                switch (Code)
                {
                    case DeviceUpdateEventCode.DeviceAdded: return "Device Added";
                    case DeviceUpdateEventCode.DeviceRemoved: return "Device Removed";
                    case DeviceUpdateEventCode.LiveStream_Started: return "Live Stream Started";
                    case DeviceUpdateEventCode.LiveStream_Stopped: return "Live Stream Stopped";
                    case DeviceUpdateEventCode.LiveStream_Updated: return "Live Stream Updated";
                    case DeviceUpdateEventCode.Recording_Started: return "Recording Started";
                    case DeviceUpdateEventCode.Recording_Stopped: return "Recording Stopped";
                }
                return null;
            }
        }
    }
}