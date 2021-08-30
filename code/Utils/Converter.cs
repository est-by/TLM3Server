using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Services.Drv.TLM3.Utils
{
    public class Converter
    {
        public static DateTimeZone minUnix = new DateTimeZone(1970, 1, 1, TimeZoneMap.Local);

        public static DateTimeZone UnixToDateLocal(int unixDate)
        {
            DateTimeZone dateTimeZ = minUnix.Add(TimeSpan.FromSeconds(unixDate));
            return dateTimeZ;
        }

        public static int DateLocalToUnix (DateTimeZone dtz)
        {
            int dateTimeUnix = (int)(dtz - minUnix).TotalSeconds; 
            return dateTimeUnix;
        }
    }
}
