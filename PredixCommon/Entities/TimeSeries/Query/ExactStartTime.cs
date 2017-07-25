using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.TimeSeries.Query
{
    public class ExactStartTime : IQueryTimeSettings
    {
        public ExactStartTime(DateTime startTime)
        {
            this.StartTime = startTime;
        }

        /// <summary>
        /// Start Time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Get the time Filter
        /// </summary>
        /// <returns></returns>
        public string GetTimeFilters()
        {
            var unixTime = DateTimeHelper.DateTimeToUnixTime(this.StartTime);

            return "\"start\":" + unixTime;
        }
    }
}
