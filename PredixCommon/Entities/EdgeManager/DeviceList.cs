using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.EdgeManager
{
    public class DeviceList
    {
        private static ILog logger = LogManager.GetLogger(typeof(DeviceList));

        //[{
        //"status": {
        //"last_change": "1491414763249",
        //        "device_status": "Offline"
        //},
        //"upTime": 60260,
        //"attributes": {
        //"groupId": "0",
        //"technicianId": null,
        //"location": null
        //},
        //"capability": {
        //"COMMAND": "2.0.0",
        //"APPLICATION": "1.0.0",
        //"CONFIGURATION": "1.0.0"
        //},
        //"location": null,
        //"firstSeenTime": null,
        //"certIssuedTime": null,
        //"deviceUUID": "b2b35940-f270-48a0-b33e-1118ffbefe29",
        //"did": "predix-vcube170100-goa-01",
        //"name": "predix-vcube170100-GOA-01",
        //"device_model_id": "Huawei-IoEE-Cube"
        //},
        //{
        //"status": {
        //"last_change": "1491414763249",
        //"device_status": "Offline"
        //},
        //"upTime": 60260,
        //"attributes": {
        //"groupId": "0",
        //"technicianId": null,
        //"location": null
        //},
        //"capability": {
        //"COMMAND": "2.0.0",
        //"APPLICATION": "1.0.0",
        //"CONFIGURATION": "1.0.0"
        //},
        //"location": null,
        //"firstSeenTime": null,
        //"certIssuedTime": null,
        //"deviceUUID": "b2b35940-f270-48a0-b33e-1118ffbefe29",
        //"did": "predix-vcube170100-goa-01",
        //"name": "predix-vcube170100-GOA-01",
        //"device_model_id": "Huawei-IoEE-Cube"
        //}]

        public List<Device> Devices { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        public DeviceList()
        {
            this.Devices = new List<Device>();
        }


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        public static IEnumerable<Device> DeserializeStream(System.IO.StreamReader data)
        {
            var readString = data.ReadToEnd();

            IEnumerable<Device> jsonObj = null;

            try
            {
                jsonObj = JsonConvert.DeserializeObject<IEnumerable<Device>>(readString);
            }
            catch (Exception ex)
            {
                logger.Fatal(string.Format("Error deserializing Device List JSON.\n\n{0}",readString), ex);
                throw;
            }

            return jsonObj;
        }
    }
}
