using System;
using System.Collections.Generic;
using System.Text;

namespace IngestCSVDataIntoTimeSeries
{
    /// <summary>
    /// Time Series Data CSV
    /// </summary>
    public class TimeSeriesDataCSV
    {
        /// <summary>
        /// Tag Name
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Time Stamp
        /// </summary>
        public long TimeStamp { get; set; }

        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; set; }
    }
}
