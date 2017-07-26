using System;
using System.Collections.Generic;
using System.Text;

namespace SchindlerMSK
{
    public class MSKMQTTRawData
    {
        public long Sequence { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Topic { get; set; }
        public string Payload { get; set; }
    }
}
