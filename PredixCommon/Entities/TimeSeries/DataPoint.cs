using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

        public DataPoint(List<object> rawDataPoint)
        {
            this.TimeStamp = long.Parse(rawDataPoint[0].ToString());
            this.Value = rawDataPoint[1].ToString();
            this.Quality = (DataQuality)Enum.Parse(typeof(DataQuality), rawDataPoint[2].ToString());
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

        public override string ToString()
        {
            return string.Format("{0} ({1}) - {2} - {3}", DateTimeHelper.JavaTimeStampToDateTime(this.TimeStamp),
                this.TimeStamp, Value, Quality);
        }
    }


}
