using log4net;
using Newtonsoft.Json;
using PredixCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DeviceStatusMQTT
{
    /// <summary>
    /// DTO class   
    /// </summary>
    public class ValueTimeStamp
    {
        private static ILog logger = LogManager.GetLogger(typeof(ValueTimeStamp));

        /// <summary>
        /// ctor
        /// </summary>
        public ValueTimeStamp()
        {

        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="timeStamp"></param>
        public ValueTimeStamp(string value, DateTime timeStamp)
        {
            this.Value = value;
            this.TimeStamp = timeStamp;
        }

        

        public string Value { get; set; }

        public DateTime TimeStamp { get; set; }


        /// <summary>
        /// Return JSON
        /// </summary>
        /// <returns></returns>
        public string ToJSON()
        {
            var json = JsonConvert.SerializeObject(this);

            return json;
        }

        /// <summary>
        /// Deserialize JSON
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static ValueTimeStamp FromJSON(string json)
        {
            ValueTimeStamp entity = null;

            try
            {
                entity = JsonConvert.DeserializeObject<ValueTimeStamp>(json);
                return entity;
            }
            catch (Exception ex)
            {
                LoggerHelper.LogErrorWriter(logger, string.Format("Error deserializing Device Details JSON.\n\n{0}", json));
                return null;
            }
        }
    }
}
