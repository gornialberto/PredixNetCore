using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.TimeSeries
{
    /// <summary>
    /// Data Point List
    /// </summary>
    public class DataPoints
    {
        public DataPoints()
        {
            this.Values = new List<DataPoint>();
            this.attributes = new NoAttributes();
        }

        public string TagName { get; set; }

        /// <summary>
        /// The Attribute for the Tag
        /// </summary>
        public IAttributes attributes { get; set; }

        public List<DataPoint> Values { get; set; }
    }
}
