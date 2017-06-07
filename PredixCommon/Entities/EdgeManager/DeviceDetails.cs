using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.EdgeManager
{
    public class DeviceDetails
    {
        private static ILog logger = LogManager.GetLogger(typeof(DeviceDetails));

        public Status status { get; set; }
        public long upTime { get; set; }
        public Attributes attributes { get; set; }
        public Capability capability { get; set; }
        public Location location { get; set; }
        public long? firstSeenTime { get; set; }
        public long? certIssuedTime { get; set; }
        public object alert { get; set; }
        public object statisticsList { get; set; }
        public DeviceModel deviceModel { get; set; }
        public List<GroupPath> groupPath { get; set; }
        public object hasA { get; set; }
        public object relatesTo { get; set; }
        public Config config { get; set; }
        public object connectivity { get; set; }
        public CustomAttributes customAttributes { get; set; }
        public DeviceInfoStatus deviceInfoStatus { get; set; }
        public string deviceUUID { get; set; }
        public string did { get; set; }
        public string name { get; set; }
        public string device_model_id { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        public static DeviceDetails DeserializeStream(System.IO.StreamReader data)
        {
            var readString = data.ReadToEnd();

            DeviceDetails jsonObj = null;

            try
            {
                jsonObj = JsonConvert.DeserializeObject<DeviceDetails> (readString);
            }
            catch (Exception ex)
            {
                logger.Fatal(string.Format("Error deserializing Device Details JSON.\n\n{0}", readString), ex);
                throw;
            }

            return jsonObj;
        }

        public class Location
        {
            public string description { get; set; }
            public object photo { get; set; }
            public object city { get; set; }
            public object state { get; set; }
            public object country { get; set; }
            public object timezone { get; set; }
            public object elevation { get; set; }
            public object lat { get; set; }
            public object lng { get; set; }
        }

        public class Status
        {
            public string last_change { get; set; }
            public string device_status { get; set; }
        }

        public class Attributes
        {
            public string groupId { get; set; }
            public object technicianId { get; set; }
            public object location { get; set; }
        }

        public class Capability
        {
            public string COMMAND { get; set; }
            public string APPLICATION { get; set; }
            public string CONFIGURATION { get; set; }
        }

        public class DeviceModel
        {
            public string id { get; set; }
            public string description { get; set; }
            public string os { get; set; }
            public string processor { get; set; }
            public int coreNum { get; set; }
            public double memoryGB { get; set; }
            public double storageGB { get; set; }
            public object customAttributes { get; set; }
            public string uri { get; set; }
            public string icon { get; set; }
        }

        public class GroupPath
        {
            public string name { get; set; }
            public string identifier { get; set; }
            public bool isOpenable { get; set; }
        }

        public class Config
        {
            public object technicianId { get; set; }
            public bool? dockerEnabled { get; set; }
            public bool? simulated { get; set; }
            public string csn { get; set; }
            public string deviceUUID { get; set; }
            public object activationCode { get; set; }
        }

        public class CustomAttributes
        {
        }

        public class CpuStatus
        {
            public double cpuPercentUser { get; set; }
            public double cpuPercentSystem { get; set; }
            public double cpuPercentIdle { get; set; }
            public List<double> cpuLoadAverage { get; set; }
        }

        public class MemoryStatus
        {
            public long totalBytes { get; set; }
            public long freeBytes { get; set; }
        }

        public class DiskStatus
        {
            public string name { get; set; }
            public string type { get; set; }
            public bool machineDisk { get; set; }
            public object totalBytes { get; set; }
            public object freeBytes { get; set; }
        }

        public class NetworkInfo
        {
            public string name { get; set; }
            public string displayName { get; set; }
            public List<string> ipv4Addresses { get; set; }
            public List<string> ipv6Addresses { get; set; }
        }

        public class DynamicStatus
        {
            public long bootTime { get; set; }
            public CpuStatus cpuStatus { get; set; }
            public MemoryStatus memoryStatus { get; set; }
            public List<DiskStatus> diskStatus { get; set; }
            public List<NetworkInfo> networkInfo { get; set; }
        }
        
        public class OsInfo
        {
            public string osName { get; set; }
            public string osVersion { get; set; }
            public string osArch { get; set; }
        }

        public class MachineInfo
        {
            public object bundle { get; set; }
            public string prosystVersion { get; set; }
            public string prosystKeyExpire { get; set; }
            public string javaVersion { get; set; }
            public string javaVendor { get; set; }
            public object service { get; set; }
            public OsInfo osInfo { get; set; }
            public string machineVersion { get; set; }
        }

        public class DeviceInfoStatus
        {
            public object deviceId { get; set; }
            public List<HardwareInfo> hardwareInfo { get; set; }
            public List<SimInfo> simInfo { get; set; }
            public object powerSupplyStatus { get; set; }
            public DynamicStatus dynamicStatus { get; set; }
            public Capability capability { get; set; }
            public MachineInfo machineInfo { get; set; }
            public object deviceProps { get; set; }
            public object bluetoothStatus { get; set; }
            public object wifiStatus { get; set; }
            public List<CellularStatus> cellularStatus { get; set; }
            public object deviceInfoProperties { get; set; }
            public object deviceStatusProperties { get; set; }
        }


        public class CellularStatus
        {
            public string id { get; set; }
            public string networkMode { get; set; }
            public int dataVolume { get; set; }
            public SignalStrength signalStrength { get; set; }
            public object attributes { get; set; }
        }

        public class SignalStrength
        {
            public int rssi { get; set; }
            public int rsrq { get; set; }
            public int rsrp { get; set; }
            public int ecio { get; set; }
            public int rscp { get; set; }
            public int sinr { get; set; }
        }



        public class Module
        {
            public string value { get; set; }
            public string dataType { get; set; }
        }

        public class Imsi
        {
            public string value { get; set; }
            public string dataType { get; set; }
        }

        public class Firmware
        {
            public string value { get; set; }
            public string dataType { get; set; }
        }

        public class Mno
        {
            public string value { get; set; }
            public string dataType { get; set; }
        }

        public class SimInfoAttributes
        {
            public Module module { get; set; }
            public Imsi imsi { get; set; }
            public Firmware firmware { get; set; }
            public Mno mno { get; set; }
        }

        public class SimInfo
        {
            public string iccid { get; set; }
            public string imei { get; set; }
            public SimInfoAttributes attributes { get; set; }
        }


        public class SerialNumber
        {
            public string value { get; set; }
            public string dataType { get; set; }
        }

        public class BootTime
        {
            public string value { get; set; }
            public string dataType { get; set; }
        }

        public class BootMethod
        {
            public string value { get; set; }
            public string dataType { get; set; }
        }

        public class HardwareInfoAttributes
        {
            public SerialNumber serialNumber { get; set; }
            public BootTime bootTime { get; set; }
            public BootMethod bootMethod { get; set; }
        }

        public class HardwareInfo
        {
            public string category { get; set; }
            public string manufacturer { get; set; }
            public string model { get; set; }
            public string firmware { get; set; }
            public object properties { get; set; }
            public HardwareInfoAttributes attributes { get; set; }
        }
    }
}
