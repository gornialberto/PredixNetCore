using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PredixEntities
{
    public class PredixTimeSeriesTagList
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
        public PredixTimeSeriesTagList(System.IO.StreamReader data)
        {
            var readString = data.ReadToEnd();

            var jsonObj = JsonConvert.DeserializeObject<JSONPredixTimeSeriesTagList>(readString);

            this.Tags = jsonObj.results;           
        }

    }
}
