using PredixCommon.Entities.TimeSeries;
using PredixCommon.Entities.TimeSeries.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchindlerMSK
{
    public class MSKTimeSeriesQuery : TimeSeriesBaseQuery, ITimeSeriesQuery
    {
        private string _mskId;
        private IEnumerable<string>  _sensorIdList;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="mskId"></param>
        public MSKTimeSeriesQuery(string mskId, IEnumerable<string> sensorIdList, IQueryTimeSettings queryFilters)
        {
            this._mskId = mskId;
            this._sensorIdList = sensorIdList;
            this.TimeFilters = queryFilters;
        }
        
        /// <summary>
        /// Get the JSON Query
        /// </summary>
        /// <returns></returns>
        public override string GetJsonQuery()
        {
            var timeFilters = this.TimeFilters.GetTimeFilters();

            string listOfSensors = string.Empty;

            foreach (var sensorId in _sensorIdList)
            {
                listOfSensors += "\"" + sensorId + "\",";
            }

            if (listOfSensors.Length > 0)
            {
                listOfSensors = listOfSensors.TrimEnd(',');
            }


            string jsonRequest =
 @"{
	[TIME_FILTERS],
    ""tags"": [{
		""name"":[[LIST_OF_TAG]],
		""order""   : ""desc"",
		""filters"" : {
	                    ""attributes"": {
	                                     ""MSKSN"" : ""[MSKID]""
                                       }
},
        ""groups"": [
        {
          ""name"": ""attribute"",
          ""attributes"" : [""DIM""]
        }
        ]
	}]
}".Replace("[LIST_OF_TAG]", listOfSensors).Replace("[MSKID]",_mskId).Replace("[TIME_FILTERS]", timeFilters);


            return jsonRequest;
        }        
    }
}
