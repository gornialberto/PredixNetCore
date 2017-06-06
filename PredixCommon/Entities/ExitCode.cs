using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon.Entities
{
    public enum ExitCode
    {
        Success = 0,
        UAAIssue = 1,
        WebSocketIssue = 2,
        NoFileToProcess = 3,
        MissingParameters = 4,
        PartiallyImported = 10,
        UnknownIssue = 50,
        EdgeManagerIssue = 11
    }
}
