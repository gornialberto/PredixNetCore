using log4net;
using Newtonsoft.Json;
using PredixCommon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DeviceStatus
{
    /// <summary>
    /// DTO class   
    /// </summary>
    public class ValueTimeStamp<T> where T : class
    {
        private static ILog logger = LogManager.GetLogger(typeof(ValueTimeStamp<T>));

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
        public ValueTimeStamp(T value, DateTime timeStamp)
        {
            this.Value = value;
            this.TimeStamp = timeStamp;
        }

        

        public T Value { get; set; }

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
        public static ValueTimeStamp<T> FromJSON(string json)
        {
            ValueTimeStamp<T> entity = null;

            try
            {
                entity = JsonConvert.DeserializeObject<ValueTimeStamp<T>>(json);
                return entity;
            }
            catch (Exception ex)
            {
                LoggerHelper.LogErrorWriter(logger, string.Format("Error deserializing Device Details JSON.\n\n{0}", json));
                LoggerHelper.LogErrorWriter(logger, string.Format(ex.ToString()));
                return null;
            }
        }
    }
}
