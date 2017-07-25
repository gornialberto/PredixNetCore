using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PredixCommon.Entities.TimeSeries.Query
{
    public class GetLastValuesByTagsQueryResponse : ITimeSeriesQueryResponse
    {
        #region sample

        /*
        
            {
  "tags": [
    {
      "name": "LV5.OPCDA01- Out:Uac_vw",
      "results": [
        {
          "groups": [
            {
              "name": "type",
              "type": "number"
            }
          ],
          "filters": {
            "qualities": {
              "values": [
                "0",
                "3"
              ]
            }
          },
          "values": [
            [
              1479306650306,
              550,
              3
            ]
          ],
          "attributes": {
            "address": [
              "opcua-//10.172.139.231-32401/opc.tcp/2/HPCI.LV5.LV5.F000.SYS/MBOut/Uac_vw"
            ],
            "category": [
              "REAL"
            ],
            "datatype": [
              "INTEGER"
            ]
          }
        },
        {
          "groups": [
            {
              "name": "type",
              "type": "text"
            }
          ],
          "filters": {
            "qualities": {
              "values": [
                "0",
                "3"
              ]
            }
          },
          "values": [
            [
              1479306647806,
              "null",
              0
            ]
          ],
          "attributes": {
            "address": [
              "opcua-//10.172.139.231-32401/opc.tcp/2/HPCI.LV5.LV5.F000.SYS/MBOut/Uac_vw"
            ],
            "category": [
              "REAL"
            ],
            "datatype": [
              "STRING"
            ]
          }
        }
      ],
      "stats": {
        "rawCount": 1
      }
    },
    {
      "name": "LV5.OPCDA01- Out:Uac_uv",
      "results": [
        {
          "groups": [
            {
              "name": "type",
              "type": "number"
            }
          ],
          "filters": {
            "qualities": {
              "values": [
                "0",
                "3"
              ]
            }
          },
          "values": [
            [
              1479306650306,
              550,
              3
            ]
          ],
          "attributes": {
            "address": [
              "opcua-//10.172.139.231-32401/opc.tcp/2/HPCI.LV5.LV5.F000.SYS/MBOut/Uac_uv"
            ],
            "category": [
              "REAL"
            ],
            "datatype": [
              "INTEGER"
            ]
          }
        },
        {
          "groups": [
            {
              "name": "type",
              "type": "text"
            }
          ],
          "filters": {
            "qualities": {
              "values": [
                "0",
                "3"
              ]
            }
          },
          "values": [
            [
              1479306647806,
              "null",
              0
            ]
          ],
          "attributes": {
            "address": [
              "opcua-//10.172.139.231-32401/opc.tcp/2/HPCI.LV5.LV5.F000.SYS/MBOut/Uac_uv"
            ],
            "category": [
              "REAL"
            ],
            "datatype": [
              "STRING"
            ]
          }
        }
      ],
      "stats": {
        "rawCount": 1
      }
    }
  ]
}
              
        */

        #endregion

        /// <summary>
        /// The response is a list of tag
        /// </summary>
        public List<Tag> tags { get; set; }

        public class Group
        {
            public string name { get; set; }
            public string type { get; set; }
        }

        public class Qualities
        {
            public List<string> values { get; set; }
        }

        public class Filters
        {
            public Qualities qualities { get; set; }
        }

        public class Attributes
        {
            public List<string> address { get; set; }
            public List<string> category { get; set; }
            public List<string> datatype { get; set; }
        }

        public class Result
        {
            public List<Group> groups { get; set; }
            public Filters filters { get; set; }
            public List<List<object>> values { get; set; }
            public Attributes attributes { get; set; }
        }

        public class Stats
        {
            public int rawCount { get; set; }
        }

        public class Tag
        {
            public string name { get; set; }
            public List<Result> results { get; set; }
            public Stats stats { get; set; }
        }
    }
}
