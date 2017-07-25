using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.TimeSeries
{
    /// <summary>
    /// Time Series Query interface
    /// </summary>
    public interface ITimeSeriesQuery
    {
        /// <summary>
        /// Expose the Query Time Filters
        /// </summary>
        IQueryTimeSettings TimeFilters { get; }

        /// <summary>
        /// Get the Json String for the Query
        /// </summary>
        /// <returns></returns>
        string GetJsonQuery();
    }
}
