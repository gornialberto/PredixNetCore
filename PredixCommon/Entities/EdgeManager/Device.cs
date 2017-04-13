using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.EdgeManager
{
    public class Device
    {
        public Status status { get; set; }
        public long? upTime { get; set; }
        public Attributes attributes { get; set; }
        public Capability capability { get; set; }
        public Location location { get; set; }
        public long? firstSeenTime { get; set; }
        public object certIssuedTime { get; set; }
        public string deviceUUID { get; set; }
        public string did { get; set; }
        public string name { get; set; }
        public string device_model_id { get; set; }
    }

    public class Status
    {
        public string last_change { get; set; }
        public string device_status { get; set; }
    }

    public class Attributes
    {
        public string groupId { get; set; }
        public string technicianId { get; set; }
        public string location { get; set; }
    }

    public class Capability
    {
        public string COMMAND { get; set; }
        public string APPLICATION { get; set; }
        public string CONFIGURATION { get; set; }
    }

    public class Location
    {
        public string description { get; set; }
        public object photo { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string timezone { get; set; }
        public object elevation { get; set; }
        public object lat { get; set; }
        public object lng { get; set; }
    }

}
