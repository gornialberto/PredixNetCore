using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.TimeSeries.Query
{
    public class StartTimeAgo : IQueryTimeSettings
    {
        public StartTimeAgo(TimeSpan timeInThePast)
        {
            this.TimeInThePast = timeInThePast;
        }

        public TimeSpan TimeInThePast { get; set; }
        
        /// <summary>
        /// Get Time Filters string
        /// </summary>
        /// <returns></returns>
        public string GetTimeFilters()
        {
            //ms, s, mi, h, d, w, mm, y

            //it will render a "-ago" sentence close to the biggest range e.g. 1 days and half will be 2 days ago.

            string time = null;

            if (this.TimeInThePast < TimeSpan.FromSeconds(1.0))
            {
                time = this.TimeInThePast.Milliseconds + "ms";
            }
            else
                if (this.TimeInThePast < TimeSpan.FromMinutes(1.0))
                {
                    time = Math.Ceiling( this.TimeInThePast.TotalSeconds ) + "s";
                }
                else
                    if (this.TimeInThePast < TimeSpan.FromHours(1.0))
                    {
                        time = Math.Ceiling(this.TimeInThePast.TotalMinutes) + "mi";
                    }
                    else
                        if (this.TimeInThePast < TimeSpan.FromDays(1.0))
                        {
                            time = Math.Ceiling(this.TimeInThePast.TotalHours) + "h";
                        }
                        else
                            time = Math.Ceiling(this.TimeInThePast.TotalDays) + "d";
                            

            return "\"start\":\"" + time + "-ago\"";
        }
    }
}
