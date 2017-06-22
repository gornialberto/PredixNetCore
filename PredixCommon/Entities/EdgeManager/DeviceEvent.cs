using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.EdgeManager
{
    public class DeviceEvent
    {
        public string DeviceId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string EventDetected { get; set; }
        public string EventSource { get; set; }
    }
}
