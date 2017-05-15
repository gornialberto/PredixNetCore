using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.TimeSeries
{
    /// <summary>
    /// Data Quality Enumerator
    /// </summary>
    public enum DataQuality
    {
        Bad = 0,
        Uncertain = 1,
        NotApplicable = 2,
        Good = 3
    }
}
