using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.TimeSeries
{
    /// <summary>
    /// Time Series Query Time Settings Interface
    /// </summary>
    public interface IQueryTimeSettings
    {
        /// <summary>
        /// Get Time Filters Parameters
        /// </summary>
        /// <returns></returns>
        string GetTimeFilters();
    }
}
