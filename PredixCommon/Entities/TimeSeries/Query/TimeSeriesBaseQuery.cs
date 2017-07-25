using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.TimeSeries.Query
{
    /// <summary>
    /// Time Series Base Query
    /// </summary>
    public abstract class TimeSeriesBaseQuery : ITimeSeriesQuery
    {
        /// <summary>
        /// Time Filters
        /// </summary>
        public IQueryTimeSettings TimeFilters { get; protected set; }

        /// <summary>
        /// TO BE IMPLEMENTED IN THE CONCRETE CLASS
        /// </summary>
        /// <returns></returns>
        public abstract string GetJsonQuery();
    }
}
