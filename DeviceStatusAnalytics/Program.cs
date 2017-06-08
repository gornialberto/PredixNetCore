using log4net;
using log4net.Config;
using PredixCommon;
using PredixCommon.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;

namespace DeviceStatusAnalytics
{
    class Program
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));
        
        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            string versionNumber = "1.0";

            logger.Debug("App Started");

            LoggerHelper.LogInfoWriter(logger,"-------------------------------------------");
            LoggerHelper.LogInfoWriter(logger," Device Status Logger v" + versionNumber);
            LoggerHelper.LogInfoWriter(logger,"-------------------------------------------");

            Environment.SetEnvironmentVariable("mqttServerAddress", "77.95.143.115");
            Environment.SetEnvironmentVariable("redisServerAddress", "77.95.143.115");


            //mqtt server is optional only if not writing to CSV!
            string mqttServerAddress = Environment.GetEnvironmentVariable("mqttServerAddress");
            string redisServerAddress = Environment.GetEnvironmentVariable("redisServerAddress");
            
            bool inputValid = true;

            if (string.IsNullOrEmpty(mqttServerAddress))
            {
                string errMsg = string.Format("MQTT Server address parameter is empty");
                logger.Fatal(errMsg);
                Console.WriteLine(errMsg);
                inputValid = false;
            }

            if (inputValid)
            {
                try
                {
                    //now execute the async part...              
                    MainAsync(mqttServerAddress, redisServerAddress).Wait();
                }
                catch (Exception ex)
                {
                    string errMsg = string.Format("There was an error during the execution of the tool.\n{0}", ex);
                    LoggerHelper.LogFatalWriter(logger,errMsg);
                }
            }
            else
            {
                string errMsg = string.Format("Some parameters is missing. Cannot execute the tool!");
                LoggerHelper.LogFatalWriter(logger, errMsg);
            }

        }

        

        static async Task MainAsync(string mqttServerAddress, string redisServerAddress)
        {
            LoggerHelper.LogInfoWriter(logger, "Starting...");

            MqttClient mqttClient = DeviceStatus.DeviceStatusHelper.GetMqttClient(mqttServerAddress, "DeviceSTatusAnalytics");

            if (mqttClient == null)
            {
                Environment.Exit((int)ExitCode.MQTTNotConnected);
            }

            DeviceStatus.DeviceStatusHelper.ConnectRedisService(redisServerAddress);

            DeviceStatus.DeviceStatusHelper.SubscribeDeviceStatusTopics(mqttClient);

            while (true)
            {
                await Task.Delay(5000);
            }
        }


    }
}