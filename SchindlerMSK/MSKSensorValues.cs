using PredixCommon.Entities.TimeSeries;
using System;
using System.Collections.Generic;
using System.Text;

namespace SchindlerMSK
{
    /// <summary>
    /// MSK Sensor Value
    /// </summary>
    public class MSKSensorValues
    {
        public MSKSensorValues()
        {
            this.Data = new List<DimensionData>();
        }

        public string MSKID { get; set; }

        public string SensorID { get; set; }

        public List<DimensionData> Data { get; set; }

        public override string ToString()
        {
            return string.Format("{0} - {1}", MSKID, SensorID);
        }

        public class DimensionData
        {
            public DimensionData()
            {
                this.Values = new List<DataPoint>();
            }

            public string Dimension { get; set; }
           
            public List<DataPoint> Values { get; set; }

            public override string ToString()
            {
                return string.Format("Dimension: {0} - {1} sample/s", Dimension, Values.Count);
            }
        }
    }
}
