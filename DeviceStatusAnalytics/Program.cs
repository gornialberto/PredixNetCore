using log4net;
using log4net.Config;
using PredixCommon;
using PredixCommon.Entities;
using System;
using System.IO;
using System.Reflection;

namespace DeviceStatusAnalytics
{
    class Program
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));

        private static ExitCode lastExitCode = ExitCode.Success;

        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            string versionNumber = "1.0";

            logger.Debug("App Started");

            LoggerHelper.LogInfoWriter(logger,"-------------------------------------------");
            LoggerHelper.LogInfoWriter(logger," Device Status Logger v" + versionNumber);
            LoggerHelper.LogInfoWriter(logger,"-------------------------------------------");





        }
    }
}