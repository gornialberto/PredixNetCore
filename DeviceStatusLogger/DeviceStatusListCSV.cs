using PredixCommon;
using PredixCommon.Entities.EdgeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeviceStatusLogger
{
    public class DeviceStatusListCSV
    {
        public string DeviceName { get; set; }

        public string DeviceID { get; set; }

        public string DeviceModel { get; set; }

        public TimeSpan? DeviceUptime { get; set; }

        public DateTime? FirstSeen { get; set; }

        public string Status { get; set; }

        public DateTime? LastStatusChange { get; set; }

        public string Description { get; set; }


        //
        public string networkMode { get; set; }

        //
        public string mno { get; set; }
        
        //
        public string iccid { get; set; }

        //
        public string imsi { get; set; }

        //
        public string imei { get; set; }

        public int? rssi { get; set; }

        public int? rsrq { get; set; }

        public int? rsrp { get; set; }

        public int? ecio { get; set; }

        public int? rscp { get; set; }

        public int? sinr { get; set; }
        
        public string module { get; set; }

        public string firmware { get; set; }

        public string IPv6 { get; set; }



        public static DeviceStatusListCSV FromDevice(Device device, IEnumerable<DeviceDetails> deviceDetailsList)
        {
            DeviceStatusListCSV deviceCsv = new DeviceStatusListCSV();

            deviceCsv.DeviceName = device.name;
            deviceCsv.DeviceID = device.did;
            deviceCsv.DeviceModel = device.device_model_id;
            deviceCsv.Status = device.status.device_status;
            deviceCsv.LastStatusChange = !string.IsNullOrEmpty(device.status.last_change) ? DateTimeHelper.JavaTimeStampToDateTime(double.Parse(device.status.last_change)) : (DateTime?)null;

            deviceCsv.Description = device.location != null ? device.location.description.Replace("\n"," ") : null;

            if (deviceDetailsList != null)
            {                
                var deviceDetails = deviceDetailsList.Where(dd => dd.did == device.did).FirstOrDefault();

                if (deviceDetails != null)
                {
                    deviceCsv.DeviceUptime = TimeSpan.FromMilliseconds((double)deviceDetails.upTime);                    
                    deviceCsv.FirstSeen = deviceDetails.firstSeenTime.HasValue ? DateTimeHelper.JavaTimeStampToDateTime((double)deviceDetails.firstSeenTime.Value) : (DateTime?)null;  

                    if (deviceDetails.deviceInfoStatus.simInfo != null)
                    {
                        var simInfo = deviceDetails.deviceInfoStatus.simInfo.FirstOrDefault();

                        if (simInfo != null)
                        {
                            deviceCsv.iccid = simInfo.iccid;
                            deviceCsv.imei = simInfo.imei;

                            if (simInfo.attributes != null)
                            {
                                if (simInfo.attributes.imsi != null)
                                {
                                    deviceCsv.imsi = simInfo.attributes.imsi.value;
                                }

                                if (simInfo.attributes.mno != null)
                                {
                                    deviceCsv.mno = simInfo.attributes.mno.value;
                                }

                                if (simInfo.attributes.module != null)
                                {
                                    deviceCsv.module = simInfo.attributes.module.value;
                                }

                                if (simInfo.attributes.firmware != null)
                                {
                                    deviceCsv.firmware = simInfo.attributes.firmware.value;
                                }
                            }
                        }

                    }
                    
                    if (deviceDetails.deviceInfoStatus.cellularStatus != null)
                    {
                        var cellularStatus = deviceDetails.deviceInfoStatus.cellularStatus.FirstOrDefault();

                        if (cellularStatus != null)
                        {
                            deviceCsv.networkMode = cellularStatus.networkMode;

                            deviceCsv.rssi = cellularStatus.signalStrength.rssi;
                            deviceCsv.rsrq = cellularStatus.signalStrength.rsrq;
                            deviceCsv.rsrp = cellularStatus.signalStrength.rsrp;
                            deviceCsv.ecio = cellularStatus.signalStrength.ecio;
                            deviceCsv.rscp = cellularStatus.signalStrength.rscp;
                            deviceCsv.sinr = cellularStatus.signalStrength.sinr;
                        }
                    }

                    if (deviceDetails.deviceInfoStatus.dynamicStatus != null)
                    {
                        if (deviceDetails.deviceInfoStatus.dynamicStatus.networkInfo != null)
                        {
                            var tun0Network = deviceDetails.deviceInfoStatus.dynamicStatus.networkInfo.Where(ni => ni.name == "tun0").FirstOrDefault();

                            if (tun0Network != null)
                            {
                                deviceCsv.IPv6 = tun0Network.ipv6Addresses.FirstOrDefault();
                            }
                        }
                    }
                    
                } //end device detials != null
            }

            return deviceCsv;
        }

 
    }
}
