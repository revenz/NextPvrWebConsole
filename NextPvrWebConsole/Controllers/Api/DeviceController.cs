using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class DeviceController : NextPvrWebConsoleApiController
    {
        static Dictionary<string, object> ScanningStatus = new Dictionary<string, object>();

        // GET api/tuners
        public IEnumerable<Models.Device> Get()
        {
            return Models.Device.GetDevices();        
        }
        
        [HttpDelete]
        public bool DeleteStream(int Handle)
        {
            Models.Device.StopStream(Handle);
            return true;
        }

        [HttpGet]
        public string Scan(int Oid)
        {
            NShared.Visible.CaptureSource cs = Helpers.NpvrCoreHelper.CaptureSourceLoadAll().Where(x => x.OID == Oid).FirstOrDefault();
            if (cs == null)
                return null;

            string scanOid = Guid.NewGuid().ToString("N");

            System.Threading.Thread thread = new System.Threading.Thread(delegate()
            {
                var paramters = cs.GetChannelScannerParameters().ToDictionary(x => x.Key, x => (object)x.Value);
                foreach (var p in paramters)
                {
                    List<object> o = cs.GetChannelScannerParameterOptions(p.Key);
                }
                string reason = null;
                var scannner = cs.GetChannelScanner(paramters, out reason);
                scannner.StartScan(out reason);

                while (!scannner.IsScanComplete())
                {
                    string status = scannner.GetStatusDescription();

                    if (ScanningStatus.ContainsKey("DeviceScan_" + scanOid))
                        ScanningStatus["DeviceScan_" + scanOid] = status;
                    else
                        ScanningStatus.Add("DeviceScan_" + scanOid, status);

                    System.Threading.Thread.Sleep(1000);
                }
                if (ScanningStatus.ContainsKey("DeviceScan_{0}_Completed".FormatStr(scanOid)))
                    ScanningStatus["DeviceScan_{0}_Completed".FormatStr(scanOid)] = true;
                else
                    ScanningStatus.Add("DeviceScan_{0}_Completed".FormatStr(scanOid), true);
            });
            thread.IsBackground = true;
            thread.Start();
            return scanOid;
        }

        [HttpPost]
        public string ScanStatus(string Oid)
        {
            if (ScanningStatus.ContainsKey("DeviceScan_{0}_Completed".FormatStr(Oid)) && ScanningStatus["DeviceScan_{0}_Completed".FormatStr(Oid)] as bool? == true)
                return "::done";

            return !ScanningStatus.ContainsKey("DeviceScan_{0}".FormatStr(Oid)) ? "" : ScanningStatus["DeviceScan_{0}".FormatStr(Oid)] as string;
        }
    }
}
