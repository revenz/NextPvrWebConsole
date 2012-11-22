using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NextPvrWebConsole.Tests
{
    static class ExtensionMethods
    {
        public static DateTime Next(this DateTime from, DayOfWeek dayOfWeek)
        {
            if (from.DayOfWeek == dayOfWeek)
                return from;
            int start = (int)from.DayOfWeek;
            int target = (int)dayOfWeek;
            if (target <= start)
                target += 7;
            return from.AddDays(target - start);
        }
    }
}
