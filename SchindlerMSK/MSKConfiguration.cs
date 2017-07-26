using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SchindlerMSK
{
    /// <summary>
    /// MSK Configuration
    /// </summary>
    public class DeviceConfiguration : List<DeviceConfiguration.MSKConfiguration>
    {   
        private static ILog logger = LogManager.GetLogger(typeof(DeviceConfiguration));
     

        public class MSKConfiguration
        {
            public MSKConfiguration()
            {
                this.Sensors = new List<SensorDetails>();
            }


            /// <summary>
            /// MSK ID
            /// </summary>
            public string MSKID { get; set; }

            /// <summary>
            /// List of Sensor ID
            /// </summary>
            public List<SensorDetails> Sensors { get; set; }

            public override string ToString()
            {
                return string.Format("{0}", MSKID);
            }
        }
                
        /// <summary>
        /// Sensor Details
        /// </summary>
        public class SensorDetails
        {
            /// <summary>
            /// Sensor ID
            /// </summary>
            public string SensorsID { get; set; }

            /// <summary>
            /// Description
            /// </summary>
            public string Description { get; set; }

            public override string ToString()
            {
                return string.Format("{0}",SensorsID);
            }
        }
              
        

        /// <summary>
        /// Deserialize MSK Configuration
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DeviceConfiguration DeserializeStream(string json)
        {
            DeviceConfiguration jsonObj = null;

            try
            {
                jsonObj = JsonConvert.DeserializeObject<DeviceConfiguration>(json);
            }
            catch (Exception ex)
            {
                logger.Fatal(string.Format("Error deserializing MSK Configuration JSON.\n\n{0}", json), ex);
                throw;
            }

            return jsonObj;
        }
        
        /// <summary>
        /// Get the JSON
        /// </summary>
        /// <returns></returns>
        public string GetJSON()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this);

                return json;
            }
            catch (Exception ex)
            {
                logger.Fatal(string.Format("Error serializing MSK Configuration JSON.\n\n{0}", ex.ToString()));
                throw;
            }
        }

    }
}
