using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Helpers
{
    public class NpvrCoreHelper
    {
        public static Dictionary<NUtility.Channel, List<NUtility.EPGEvent>> GetListingsForTimePeriod(DateTime Start, DateTime End)
        {
            return Cacher.RetrieveOrStore<Dictionary<NUtility.Channel, List<NUtility.EPGEvent>>>("GetListingsForTimePeriod(" + Start.ToString() + "," + End.ToString() + ")", new TimeSpan(1, 0, 0), delegate
            {
                return NUtility.EPGEvent.GetListingsForTimePeriod(Start, End);
            });
        }
    }
}