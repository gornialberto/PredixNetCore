using log4net;
using log4net.Config;
using PredixCommon;
using PredixCommon.Entities;
using PredixCommon.Entities.EdgeManager;
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

     


            string baseUAAUrl = Environment.GetEnvironmentVariable("baseUAAUrl");
            string clientID = Environment.GetEnvironmentVariable("clientID");
            string clientSecret = Environment.GetEnvironmentVariable("clientSecret");
            string edgeManagerBaseUrl = Environment.GetEnvironmentVariable("edgeManagerBaseUrl");


            string redisServerAddress = Environment.GetEnvironmentVariable("redisServerAddress");
            
            bool inputValid = true;

            if (string.IsNullOrEmpty(baseUAAUrl))
            {
                string errMsg = string.Format("Base UAA Url parameter is empty");
                logger.Fatal(errMsg);
                Console.WriteLine(errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(clientID))
            {
                string errMsg = string.Format("Client ID parameter is empty");
                logger.Fatal(errMsg);
                Console.WriteLine(errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                string errMsg = string.Format("Client Secret parameter is empty");
                logger.Fatal(errMsg);
                Console.WriteLine(errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(edgeManagerBaseUrl))
            {
                string errMsg = string.Format("Edge Manager Base Url parameter is empty");
                logger.Fatal(errMsg);
                Console.WriteLine(errMsg);
                inputValid = false;
            }


            if (string.IsNullOrEmpty(redisServerAddress))
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
                    MainAsync(baseUAAUrl, clientID, clientSecret, edgeManagerBaseUrl, redisServerAddress).Wait();
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

        

        static async Task MainAsync(string baseUAAUrl, string clientID, string clientSecret, string edgeManagerBaseUrl, string redisServerAddress)
        {
            LoggerHelper.LogInfoWriter(logger, "Starting...");

            var redisClient =  DeviceStatus.DeviceStatusHelper.ConnectRedisService(redisServerAddress);

            while (true)
            {
                LoggerHelper.LogInfoWriter(logger, "Loading from REDIS device list...");
                var deviceList = DeviceStatus.DeviceStatusHelper.GetRedisDeviceList(redisClient);

                if ( deviceList != null)
                {
                    //now for each device check if there is any device update that matters...

                    LoggerHelper.LogInfoWriter(logger, string.Format( "Gathering Device Details and check for updates for {0} devices...", deviceList.Value.Count));

                    foreach (var deviceId in deviceList.Value)
                    {
                        var dev = DeviceStatus.DeviceStatusHelper.GetRedisDeviceDetails(redisClient, deviceId);

                        if (dev != null)
                        {
                            DeviceStatus.DeviceStatusHelper.CheckDeviceDetailsForUpdate(redisClient, dev);
                        }
                    }


                    //async execution...
                    DeviceStatus.DeviceStatusHelper.CheckHistoryAndSendReport(baseUAAUrl, clientID, clientSecret, edgeManagerBaseUrl,redisClient, deviceList.Value);                    
                }
                
                //wait for 5 minutes...  before next check...
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }


    }
}