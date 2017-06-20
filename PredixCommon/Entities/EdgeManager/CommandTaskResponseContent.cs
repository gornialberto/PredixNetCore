using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PredixCommon.Entities.EdgeManager
{
    /// <summary>
    /// Command Task Response
    /// </summary>
    public class CommandTaskResponseContent
    {
        private static ILog logger = LogManager.GetLogger(typeof(CommandTaskResponseContent));

        public string deviceId { get; set; }
        public string taskId { get; set; }
        public int code { get; set; }
        public string message { get; set; }


        public static CommandTaskResponseContent DeserializeStream(StreamReader data)
        {
            var readString = data.ReadToEnd();

            CommandTaskResponseContent jsonObj = null;

            try
            {
                jsonObj = JsonConvert.DeserializeObject<CommandTaskResponseContent>(readString);
            }
            catch (Exception ex)
            {
                logger.Fatal(string.Format("Error deserializing Command Task Response Content to JSON.\n\n{0}", readString), ex);
                throw;
            }

            return jsonObj;
        }

        public override string ToString()
        {
            return  string.Format("CommandTaskResponseContent - DeviceId: '{0}' - TaskId: '{1}'", deviceId, taskId);
        }
    }
}
