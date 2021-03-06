﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SchindlerMSK
{
    public class MSKCsvData
    {
        public string MQTTMessageTimeStamp { get; set; }
        public string MQTTMessageSequence { get; set; }
        public string MQTTMessageUnixTime { get; set; }

        public string TimeStamp { get; set; }
        public string MSKID { get; set; }
        public string SensorID { get; set; }
        public string Dimension { get; set; }
        public string Value { get; set; }
    }
}
