using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.TimeSeries
{
    public class DataPoint
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DataPoint()
        {

        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="rawDataPoint"></param>
        public DataPoint(List<string> rawDataPoint)
        {
            this.TimeStamp = long.Parse(rawDataPoint[0]);
            this.Value = rawDataPoint[1];
            this.Quality = (DataQuality)Enum.Parse(typeof(DataQuality), rawDataPoint[2]);
        }

        /// <summary>
        /// Time Stamp in UNIX Epoch time with milliseconds
        /// </summary>
        public long TimeStamp { get; set; }

        /// <summary>
        /// The Value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The Data Quality
        /// </summary>
        public DataQuality Quality { get; set; }


        /// <summary>
        /// Get the raw JSON Serialzation for data injestion
        /// </summary>
        /// <returns></returns>
        public List<string> ToRawJSONValues()
        {
            List<string> rawJSON = new List<string>();

            rawJSON.Add(this.TimeStamp.ToString());
            rawJSON.Add(this.Value);
            rawJSON.Add(((int)this.Quality).ToString());

            return rawJSON;
        }
    }


}
