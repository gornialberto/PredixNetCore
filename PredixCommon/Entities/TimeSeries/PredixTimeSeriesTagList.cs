using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PredixCommon.Entities.TimeSeries
{
    public class List
    {
        //{
        //"results": ["TAG1","TAG2"]
        //}

        internal class JSONPredixTimeSeriesTagList
        {
            public JSONPredixTimeSeriesTagList()
            {
                results = null;
            }


            public string[] results;
        }

        public string[] Tags { get; set; }


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        public List(System.IO.StreamReader data)
        {
            var readString = data.ReadToEnd();

            var jsonObj = JsonConvert.DeserializeObject<JSONPredixTimeSeriesTagList>(readString);

            this.Tags = jsonObj.results;           
        }

    }
}
