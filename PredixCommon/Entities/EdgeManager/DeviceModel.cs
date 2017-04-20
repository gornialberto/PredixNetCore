using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.EdgeManager
{
    public class DeviceModel
    {
        public int coreNum { get; set; }
        public ICustomAttributes customAttributes { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
        public string id { get; set; }
        public double memoryGB { get; set; }
        public string os { get; set; }
        public string photo { get; set; }
        public string processor { get; set; }
        public double storageGB { get; set; }
    }
}
