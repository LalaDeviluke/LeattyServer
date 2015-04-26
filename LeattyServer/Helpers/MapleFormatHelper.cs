﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeattyServer.Helpers
{
    static class MapleFormatHelper
    {        
        /// <summary>
        /// Converts ticks from a DateTime object into Windows file time
        /// </summary>
        /// <param name="timeStamp">The UTC timestamp to be converted to Windows file time</param>
        /// <returns></returns>
        public static long GetMapleTimeStamp(long timeStamp) //timestamp in ticks, -1 -2 and -3 are reserved
        {           
            if (timeStamp == -1)
            {
                return 0x217E646BB058000; //1-1-2079 0:00:00
            }
            if (timeStamp == -2)
            {
                return 0x14F373BFDE04000; //1-1-1900 0:00:00
            }
            if (timeStamp == -3)
            {
                return 0x217E57D909BC000; //31-12-2078 0:00:00
            }
            return new DateTime(timeStamp).ToFileTimeUtc();
        }

        public static long GetMapleTimeStamp(DateTime date)
        {
            return date.ToFileTimeUtc();
        }

        public static DateTime GetDateTimeFromMapleTimeStamp(long mapleTimeStamp)
        {
            return DateTime.FromFileTimeUtc(mapleTimeStamp);
        }

        public static int GetCurrentDate()
        {
            string date = DateTime.UtcNow.ToString("yyyyMMddhh");
            return Int32.Parse(date);
        }
    }
}
