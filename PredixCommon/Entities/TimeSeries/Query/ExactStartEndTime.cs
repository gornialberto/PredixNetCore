using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.TimeSeries.Query
{
    public class ExactStartEndTime : IQueryTimeSettings
    {
        public ExactStartEndTime(DateTime startTime, DateTime endTime)
        {
            this.StartTime = startTime;
            this.EndTime = endTime;
        }

        /// <summary>
        /// Start Time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End Time
        /// </summary>
        public DateTime EndTime { get; set; }



        /// <summary>
        /// Get the time Filter
        /// </summary>
        /// <returns></returns>
        public string GetTimeFilters()
        {
            var st_unixTime = DateTimeHelper.DateTimeToUnixTime(this.StartTime);
            var et_unixTime = DateTimeHelper.DateTimeToUnixTime(this.EndTime);

            return "\"start\":" + st_unixTime + ",\"end\":" + et_unixTime;
        }
    }
}
