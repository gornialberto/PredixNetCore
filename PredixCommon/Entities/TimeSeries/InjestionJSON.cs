using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PredixCommon.Entities.TimeSeries
{
    public class InjestionJSON
    {
        /// <summary>
        /// ctor
        /// </summary>
        public InjestionJSON()
        {
            //set the message ID as the current time in ms
            this.messageId = DateTimeHelper.DateTimeToUnixTime(DateTime.UtcNow).ToString();
            this.body = new List<InjestionDataBlock>();
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dataPointsList"></param>
        public InjestionJSON(IEnumerable<DataPoints> dataPointsList)
        {
            //set the message ID as the current time in ms
            this.messageId = DateTimeHelper.DateTimeToUnixTime(DateTime.UtcNow).ToString();
            this.body = new List<InjestionDataBlock>();

            foreach (var dataPoint in dataPointsList)
            {
                this.body.Add(new InjestionDataBlock(dataPoint));
            }
        }

        public string messageId { get; set; }
        public List<InjestionDataBlock> body { get; set; }
    }

    /// <summary>
    /// The interface for a generic "Attribute" property class...
    /// </summary>
    public interface IAttributes
    {

    }

    public class NoAttributes : IAttributes
    {

    }

    public class InjestionDataBlock
    {
        /// <summary>
        /// ctor
        /// </summary>
        public InjestionDataBlock()
        {
            this.attributes = new NoAttributes();
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dataPoints"></param>
        public InjestionDataBlock(DataPoints dataPoints)
        {
            this.name = dataPoints.TagName;
            this.attributes = dataPoints.attributes;

            this.datapoints = (from item in dataPoints.Values
                               select item.ToRawJSONValues()).ToList();
        }


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
        public DataPoints GetDataPoints()
        {
            DataPoints dataPoints = new DataPoints();
            dataPoints.TagName = this.name;
            dataPoints.attributes = this.attributes;

            foreach (var rawDataPoint in datapoints)
            {
                var dataPoint = new DataPoint(rawDataPoint);
                dataPoints.Values.Add(dataPoint);
            }

            return dataPoints;
        }
    }


}
