using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.TimeSeries.Query
{
    public class GetLastValuesByTagsQuery : TimeSeriesBaseQuery, ITimeSeriesQuery
    {
        private List _tagList = null;

        /// <summary>
        /// ctor
        /// </summary>
        public GetLastValuesByTagsQuery(List tagList)
        {
            this._tagList = tagList;

            //search max 60 days in the past...
            this.TimeFilters = new StartTimeAgo(TimeSpan.FromDays(60.0));
        }

        /// <summary>
        /// Get the JSON Query
        /// </summary>
        /// <returns></returns>
        public override string GetJsonQuery()
        { 
            string listOfTagForJson = string.Empty;

            foreach (var tagName in _tagList.Tags)
            {
                listOfTagForJson += "\"" + tagName + "\",";
            }

            if (listOfTagForJson.Length > 0)
            {
                listOfTagForJson = listOfTagForJson.TrimEnd(',');
            }
            
            var timeFilters = this.TimeFilters.GetTimeFilters();

            string jsonRequest =
 @"{
                  [TIME_FILTERS],
            	  ""tags"":[{
            		    		""name"":[**LIST_OF_TAG**],
            			     	""limit"":1,
            				    ""order"": ""desc"",
            				    ""filters"": {
                            	                ""qualities"": {
                                	                              ""values"": [""3""]
                                                               }
                                             }
            			}]	
            }".Replace("**LIST_OF_TAG**", listOfTagForJson).Replace("[TIME_FILTERS]", timeFilters);
            
            return jsonRequest;
        }
    }
}
