using PredixCommon;
using PredixCommon.Entities.EdgeManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace GetDeviceList
{
    public class DeviceListCSV
    {
        public string DeviceName { get; set; }

        public string DeviceID { get; set; }

        public string DeviceModel { get; set; }

        //public TimeSpan? DeviceUptime { get; set; }

        //public DateTime? FirstSeen { get; set; }

        public string Status { get; set; }

        public DateTime? LastStatusChange { get; set; }



        public static DeviceListCSV FromDevice(Device device)
        {
            DeviceListCSV deviceCsv = new DeviceListCSV();

            deviceCsv.DeviceName = device.name;
            deviceCsv.DeviceID = device.did;
            deviceCsv.DeviceModel = device.device_model_id;
            //deviceCsv.DeviceUptime = device.upTime.HasValue ? TimeSpan.FromMilliseconds((double)device.upTime) : (TimeSpan?)null;
            //deviceCsv.FirstSeen = device.firstSeenTime.HasValue ? DateTimeHelper.JavaTimeStampToDateTime((double)device.firstSeenTime.Value) : (DateTime?)null;
            deviceCsv.Status = device.status.device_status;
            deviceCsv.LastStatusChange = !string.IsNullOrEmpty(device.status.last_change) ? DateTimeHelper.JavaTimeStampToDateTime(double.Parse(device.status.last_change)) : (DateTime?)null;

            return deviceCsv;
        }
    }
}
