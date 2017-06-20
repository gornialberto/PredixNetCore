using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PredixCommon.Entities.EdgeManager
{

    public class CommandDefinitionResponse
    {
        private static ILog logger = LogManager.GetLogger(typeof(CommandDefinitionResponse));

        public int commandId { get; set; }
        public string commandDisplayName { get; set; }
        public string command { get; set; }
        public bool hasOutput { get; set; }
        public object cmdParameters { get; set; }
        public string commandType { get; set; }
        public object tenantId { get; set; }
        public string handler { get; set; }

        public override string ToString()
        {
            return string.Format("CommandDefinitionResponse - commandDisplayName: '{0}' - commandId: '{1}'", commandDisplayName, commandId);
        }


        public static List<CommandDefinitionResponse> DeserializeStream(StreamReader data)
        {
            var readString = data.ReadToEnd();

            List<CommandDefinitionResponse> jsonObj = null;

            try
            {
                jsonObj = JsonConvert.DeserializeObject<List<CommandDefinitionResponse>>(readString);
            }
            catch (Exception ex)
            {
                logger.Fatal(string.Format("Error deserializing CommandDefinitionResponse to JSON.\n\n{0}", readString), ex);
                throw;
            }

            return jsonObj;
        }
    }

}
