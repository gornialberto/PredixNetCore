using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities.EdgeManager
{
    /// <summary>
    /// Command Request Body JSON
    /// </summary>
    public class CommandRequest
    {
        public int commandId { get; set; }
        public List<string> devices { get; set; }
        public string name { get; set; }
        public bool outputUrlRequired { get; set; }
        public object @params { get; set; } //the @ is to escape the keyword name params ahahahah 
        public int timeout { get; set; }
        public string type { get; set; }
    }
}
