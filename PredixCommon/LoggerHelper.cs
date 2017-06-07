using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace PredixCommon
{
    public class LoggerHelper
    {
        public static void LogInfoWriter(ILog logger, string content, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            logger.Info(content);
        }

        public static void LogErrorWriter(ILog logger, string content, ConsoleColor color = ConsoleColor.Red)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            logger.Error(content);

        }

        public static void LogFatalWriter(ILog logger, string content, ConsoleColor color = ConsoleColor.DarkRed)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            logger.Fatal(content);
        }
    }
}
