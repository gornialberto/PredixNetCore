using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.TimeSeries
{
    public class InjestionJSON
    {
        public string messageId { get; set; }
        public List<Body> body { get; set; }
    }

    /// <summary>
    /// The interface for a generic "Attribute" property class...
    /// </summary>
    public interface IAttributes
    {

    }

    public class Body
    {
        /// <summary>
        /// The Tag Name
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The Data Point list
        /// </summary>
        public List<List<string>> datapoints { get; set; }

        /// <summary>
        /// The Attribute for the Tag
        /// </summary>
        public IAttributes attributes { get; set; }

        /// <summary>
        /// Get the List of Data Point
        /// </summary>
        /// <returns></returns>
        public List<DataPoint> GetDataPoints()
        {
            List<DataPoint> dataPoints = new List<DataPoint>();

            foreach (var rawDataPoint in datapoints)
            {
                var dataPoint = new DataPoint(rawDataPoint);
                dataPoints.Add(dataPoint);
            }

            return dataPoints;
        }
    }


}
