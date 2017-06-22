using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.EdgeManager
{
    public class LogEventDetected
    {
        public string DeviceId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string EventDetected { get; set; }
        public int LogRow { get; set; }
        public string LogType { get; set; }
    }
}
