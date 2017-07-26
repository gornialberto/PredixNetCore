using PredixCommon.Entities.TimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using static SchindlerMSK.MSKSensorValues;

namespace SchindlerMSK
{
    /// <summary>
    /// MSK Query Response
    /// </summary>
    public class MSKQueryRawResponse : ITimeSeriesQueryResponse
    {
        #region Sample data response...

        /*
        {
    "tags": [
        {
            "name": "SN01_02",
            "results": [
                {
                    "groups": [
                        {
                            "name": "attribute",
                            "attributes": [
                                "DIM"
                            ],
                            "group": {
                                "DIM": "x"
                            }
                        },
                        {
                            "name": "type",
                            "type": "number"
                        }
                    ],
                    "filters": {
                        "attributes": {
                            "MSKSN": [
                                "SCU-1705-300065"
                            ]
                        }
                    },
                    "values": [
                        [
                            1500393614037,
                            0,
                            3
                        ],
                        [
                            1500390014037,
                            0,
                            3
                        ]
                    ],
                    "attributes": {
                        "DIM": [
                            "x"
                        ],
                        "MSKSN": [
                            "SCU-1705-300065"
                        ]
                    }
                },
                {
                    "groups": [
                        {
                            "name": "attribute",
                            "attributes": [
                                "DIM"
                            ],
                            "group": {
                                "DIM": "y"
                            }
                        },
                        {
                            "name": "type",
                            "type": "number"
                        }
                    ],
                    "filters": {
                        "attributes": {
                            "MSKSN": [
                                "SCU-1705-300065"
                            ]
                        }
                    },
                    "values": [
                        [
                            1500393614037,
                            0,
                            3
                        ],
                        [
                            1500390014037,
                            0,
                            3
                        ]
                    ],
                    "attributes": {
                        "DIM": [
                            "y"
                        ],
                        "MSKSN": [
                            "SCU-1705-300065"
                        ]
                    }
                },
                {
                    "groups": [
                        {
                            "name": "attribute",
                            "attributes": [
                                "DIM"
                            ],
                            "group": {
                                "DIM": "z"
                            }
                        },
                        {
                            "name": "type",
                            "type": "number"
                        }
                    ],
                    "filters": {
                        "attributes": {
                            "MSKSN": [
                                "SCU-1705-300065"
                            ]
                        }
                    },
                    "values": [
                        [
                            1500393614037,
                            0,
                            3
                        ],
                        [
                            1500390014037,
                            0,
                            3
                        ]
                    ],
                    "attributes": {
                        "DIM": [
                            "z"
                        ],
                        "MSKSN": [
                            "SCU-1705-300065"
                        ]
                    }
                }
            ],
            "stats": {
                "rawCount": 6
            }
        }
    ]
}
                        */ 
        
        #endregion


        public List<Tag> tags { get; set; }

        public class DimensionGroup
        {
            public string DIM { get; set; }
        }

        public class Group
        {
            public string name { get; set; }
            public List<string> attributes { get; set; }
            public DimensionGroup group { get; set; }
            public string type { get; set; }
        }

        public class Attributes
        {
            public List<string> MSKSN { get; set; }
        }

        public class Filters
        {
            public Attributes attributes { get; set; }
        }

        public class Attributes2
        {
            public List<string> DIM { get; set; }
            public List<string> MSKSN { get; set; }
        }

        public class Result
        {
            public List<Group> groups { get; set; }
            public Filters filters { get; set; }
            public List<List<object>> values { get; set; }
            public Attributes2 attributes { get; set; }
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


        /// <summary>
        /// Get Data structure processed for easy data view
        /// </summary>
        /// <returns></returns>
        public List<MSKSensorValues> GetData()
        {
            List<MSKSensorValues> mskSensorValues = new List<MSKSensorValues>();

            foreach (var sensorResult in this.tags)
            {
                if (sensorResult.results.Count > 0)
                {
                    MSKSensorValues sensorValue = new MSKSensorValues();
                    sensorValue.SensorID = sensorResult.name;

                    bool sensorHasData = false;

                    foreach (var dimension in sensorResult.results)
                    {
                        if (dimension.values.Count > 0)
                        {
                            sensorHasData = true;

                            //this is a group of result by Axes
                            var msk = dimension.attributes.MSKSN.FirstOrDefault();
                            var dimensionName = dimension.attributes.DIM.FirstOrDefault();

                            sensorValue.MSKID = msk;

                            DimensionData dimensionData = new DimensionData();
                            dimensionData.Dimension = dimensionName;
                            sensorValue.DimensionsData.Add(dimensionData);

                            foreach (var data in dimension.values)
                            {
                                var dataPoint = new DataPoint(data);
                                dimensionData.Values.Add(dataPoint);
                            }
                        }
                    }

                    if (sensorHasData)
                    {
                        mskSensorValues.Add(sensorValue);
                    }
                }
            }
            
            return mskSensorValues;
        }
    }
}
