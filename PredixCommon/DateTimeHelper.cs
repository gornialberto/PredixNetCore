using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon
{
    public class DateTimeHelper
    {
        public static DateTime unixEpocStartTime = new DateTime(1970, 1, 1,0,0,0,0, DateTimeKind.Utc);

        /// <summary>
        /// From Java Unix Time (including milliseconds)
        /// </summary>
        /// <param name="javaTimeStamp"></param>
        /// <returns></returns>
        public static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            // Java timestamp is millisecods past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(javaTimeStamp);
            return dtDateTime;
        }

        /// <summary>
        /// Get Unix Time including milliseconds
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static double DateTimeToUnixTime(DateTime timeStamp)
        {
            return (timeStamp.Subtract(unixEpocStartTime)).TotalMilliseconds;
        }
    }
}
