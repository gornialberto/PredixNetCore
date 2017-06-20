using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PredixCommon.Entities.EdgeManager
{
    public class CommandTaskResponse
    {
        public string taskId { get; set; }
        public string deviceId { get; set; }
        public bool success { get; set; }
        public string message { get; set; }

        public override string ToString()
        {
            return string.Format("CommandTaskResponse - DeviceId: '{0}' - TaskId: '{1}'", deviceId, taskId);
        }
    }

    public class CommandResponse
    {
        private static ILog logger = LogManager.GetLogger(typeof(CommandResponse));


        public string operationId { get; set; }
        public List<CommandTaskResponse> taskResponse { get; set; }
        

        public static CommandResponse DeserializeStream(StreamReader data)
        {
            var readString = data.ReadToEnd();

            CommandResponse jsonObj = null;

            try
            {
                jsonObj = JsonConvert.DeserializeObject<CommandResponse>(readString);
            }
            catch (Exception ex)
            {
                logger.Fatal(string.Format("Error deserializing Command Response to JSON.\n\n{0}", readString), ex);
                throw;
            }

            return jsonObj;
        }
    }
}
