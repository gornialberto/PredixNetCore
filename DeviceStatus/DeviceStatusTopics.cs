﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceStatus
{
    /// <summary>
    /// Device Status Topics
    /// </summary>
    public class DeviceStatusTopics
    {
        public const string RedisDeviceListKey = "RedisDeviceList";
        public const string RedisDeviceDetails = "RedisDeviceDetails_{DeviceId}";

       

        public const string TopicTemplate = "deviceStatus/{DeviceId}/{typeOfValue}";
        
        public const string MQTTStatusTopic = "deviceStatus/status";
        public const string MQTTDeviceLisTopic = "deviceStatus/deviceList";


        public const string DeviceName = "DeviceName";
        public const string DeviceModel = "DeviceModel";
        public const string mno = "mno";
        public const string IPv6 = "IPv6";
        public const string networkMode = "networkMode";
        public const string iccid = "iccid";
        public const string imei = "imei";
        public const string imsi = "imsi";
        public const string rscp = "rscp";
        public const string rsrp = "rsrp";
        public const string rsrq = "rsrq";
        public const string rssi = "rssi";
        public const string ecio = "ecio";
        public const string sinr = "sinr";
        public const string Status = "Status";
        

        /// <summary>
        /// Get a Topic String
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="typeOfValue"></param>
        /// <returns></returns>
        public static string GetTopic(string deviceID, string typeOfValue)
        {
            return TopicTemplate.Replace("{DeviceId}", deviceID).Replace("{typeOfValue}", typeOfValue);
        }
    }
}
