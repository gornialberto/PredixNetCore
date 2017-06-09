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
using System.Linq;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;

namespace DeviceStatusLogger
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

            logInfoWriter("-------------------------------------------");
            logInfoWriter(" Device Status Logger v" + versionNumber);
            logInfoWriter("-------------------------------------------");





            //mqtt server is optional only if not writing to CSV!
            string mqttServerAddress = Environment.GetEnvironmentVariable("mqttServerAddress");
            string redisServerAddress = Environment.GetEnvironmentVariable("redisServerAddress");



            string baseUAAUrl = Environment.GetEnvironmentVariable("baseUAAUrl");
            string clientID = Environment.GetEnvironmentVariable("clientID");
            string clientSecret = Environment.GetEnvironmentVariable("clientSecret");
            string edgeManagerBaseUrl = Environment.GetEnvironmentVariable("edgeManagerBaseUrl");

            //this is optional if not provided the MQTT address and port is needed! 
            string csvFilePath = Environment.GetEnvironmentVariable("csvFilePath");


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

            if (string.IsNullOrEmpty(csvFilePath) && (string.IsNullOrEmpty(mqttServerAddress) && string.IsNullOrEmpty(redisServerAddress)))
            {
                string errMsg = string.Format("CSV Path & MQTT or REDIS Server host parameter are empty! cannot be both empty!");
                logger.Fatal(errMsg);
                Console.WriteLine(errMsg);
                inputValid = false;
            }

            if (inputValid)
            {
                try
                {
                    //now execute the async part...              
                    MainAsync(baseUAAUrl, clientID, clientSecret, edgeManagerBaseUrl, csvFilePath, mqttServerAddress, redisServerAddress).Wait();
                }
                catch (Exception ex)
                {
                    string errMsg = string.Format("There was an error during the execution of the tool.\n{0}", ex);
                    logFatalWriter(errMsg);
                }
            }
            else
            {
                string errMsg = string.Format("Some parameters is missing. Cannot execute the tool!");
                logFatalWriter(errMsg);
            }
        }


        static async Task MainAsync(string baseUAAUrl, string clientID, string clientSecret, string edgeManagerBaseUrl, string csvFilePath, string mqttServerAddress, string redisServerAddress)
        {
            logInfoWriter("Starting...");

            logInfoWriter("Getting Access Token for ClientID: " + clientID);

            UAAToken accessToken = null;

            try
            {
                accessToken = await UAAHelper.GetClientCredentialsGrantAccessToken(baseUAAUrl, clientID, clientSecret);

                if (accessToken != null)
                {
                    logInfoWriter("  Token obtained!", ConsoleColor.Green);
                }
                else
                {
                    logFatalWriter("  Error obtaining Token");
                    cleanReturn(ExitCode.UAAIssue);
                }
            }
            catch (Exception ex)
            {
                logFatalWriter(string.Format("\n\n***\n{0}\n***\n\n", ex.ToString()));
                cleanReturn(ExitCode.UAAIssue);
                return;
            }

            List<DeviceDetails> deviceDetailsList = null;

            if (string.IsNullOrEmpty(mqttServerAddress) && string.IsNullOrEmpty(redisServerAddress))
            {
                //just CSV write...
                logInfoWriter("Writing to CSV one shot data capture...");

                deviceDetailsList = await getDeviceDetails(baseUAAUrl, clientID, clientSecret, accessToken, edgeManagerBaseUrl);

                if (deviceDetailsList != null)
                {
                    var deviceCsvList = from device in deviceDetailsList
                                        select DeviceStatusListCSV.FromDevice(device);

                    try
                    {
                        using (var csvFileStream = System.IO.File.Create(csvFilePath))
                        {
                            using (var csvFileWriter = new System.IO.StreamWriter(csvFileStream))
                            {
                                using (CsvHelper.CsvWriter csvWriter = new CsvHelper.CsvWriter(csvFileWriter))
                                {
                                    csvWriter.WriteHeader<DeviceStatusListCSV>();

                                    foreach (var deviceCsv in deviceCsvList)
                                        csvWriter.WriteRecord<DeviceStatusListCSV>(deviceCsv);

                                    csvFileWriter.Flush();
                                    csvFileStream.Flush();
                                }
                            }
                        }

                        Console.WriteLine("Work done!");
                    }
                    catch (Exception ex)
                    {
                        var msg = string.Format("An error occurred writing CSV file.\n{0}", ex);
                        logFatalWriter(msg);
                    }
                }

            }
            else
            {
                if (redisServerAddress != null)
                {
                    LoggerHelper.LogInfoWriter(logger, "Starting loop and sending data to Redis");

                    var redisClient = DeviceStatus.DeviceStatusHelper.ConnectRedisService(redisServerAddress);

                    while (true)
                    {
                        deviceDetailsList = await getDeviceDetails(baseUAAUrl, clientID, clientSecret, accessToken, edgeManagerBaseUrl);

                        if (deviceDetailsList != null)
                        {
                            var deviceCsvList = (from device in deviceDetailsList

                                                 select device).ToList();

                            var timeStamp = DateTime.UtcNow;

                            DeviceStatus.DeviceStatusHelper.PublishRedisDeviceList(redisClient, deviceCsvList, timeStamp);

                            //logInfoWriter(string.Format("  Found {0} devices with Cellular status updated. Sending data to MQTT", deviceCsvList.Count));
                            LoggerHelper.LogInfoWriter(logger, string.Format("  Sending deetails to REDIS for {0} devices at {1}", deviceCsvList.Count, timeStamp));

                            foreach (var dev in deviceCsvList)
                            {
                                DeviceStatus.DeviceStatusHelper.PublishRedisDeviceDetails(redisClient, dev, timeStamp);
                            }

                            LoggerHelper.LogInfoWriter(logger, "  Data sent to REDIS", ConsoleColor.Green);
                        }

                        System.Threading.Thread.Sleep(TimeSpan.FromMinutes(5));

                    } //end of while...    

                }
                else
                {
                    //send data to MQTT
                    LoggerHelper.LogInfoWriter(logger, "Starting loop and sending data to MQTT");

                    MqttClient mqttClient = DeviceStatus.DeviceStatusHelper.GetMqttClient(mqttServerAddress, "DeviceStatusLoggerClient");

                    if (mqttClient == null)
                    {
                        Environment.Exit((int)ExitCode.MQTTNotConnected);
                    }

                    while (true)
                    {
                        deviceDetailsList = await getDeviceDetails(baseUAAUrl, clientID, clientSecret, accessToken, edgeManagerBaseUrl);

                        if (deviceDetailsList != null)
                        {
                            var deviceCsvList = (from device in deviceDetailsList

                                                 select device).ToList();

                            //where device.deviceInfoStatus.simInfo != null &&
                            //device.deviceInfoStatus.cellularStatus != null

                            var timeStamp = DateTime.UtcNow;

                            DeviceStatus.DeviceStatusHelper.PublishMQTTDeviceList(mqttClient, deviceCsvList, timeStamp);

                            //logInfoWriter(string.Format("  Found {0} devices with Cellular status updated. Sending data to MQTT", deviceCsvList.Count));
                            LoggerHelper.LogInfoWriter(logger, string.Format("  Sending deetails to MQTT for {0} devices at {1}", deviceCsvList.Count, timeStamp));

                            foreach (var dev in deviceCsvList)
                            {
                                DeviceStatus.DeviceStatusHelper.PublishMQTTDeviceDetails(mqttClient, dev, timeStamp);
                            }

                            LoggerHelper.LogInfoWriter(logger, "  Data sent to MQTT", ConsoleColor.Green);
                        }

                        System.Threading.Thread.Sleep(TimeSpan.FromMinutes(5));

                    } //end of while...    
                }
               
                                
                 
            }

            cleanReturn(ExitCode.Success);
        }
        

        static async Task<List<DeviceDetails>> getDeviceDetails(string baseUAAUrl, string clientID, string clientSecret, UAAToken accessToken, string edgeManagerBaseUrl)
        {
            logInfoWriter("Querying Edge Manager for Device List: " + edgeManagerBaseUrl);

            //get the list of tags
            DeviceList deviceList = null;

            try
            {
                deviceList = await EdgeManagerHelper.GetDeviceList(edgeManagerBaseUrl, accessToken);
            }
            catch (Exception ex)
            {
                logFatalWriter(string.Format("\n\n***\n{0}\n***\n\n", ex.ToString()));
                lastExitCode = ExitCode.EdgeManagerIssue;
                return null;
            }

            if (EdgeManagerHelper.LatestHTTPStatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                LoggerHelper.LogInfoWriter(logger, "UAA Token was expired? Let's try to login agian...");

                try
                {
                    accessToken = await UAAHelper.GetClientCredentialsGrantAccessToken(baseUAAUrl, clientID, clientSecret);

                    if (accessToken != null)
                    {
                        logInfoWriter("  Token obtained!", ConsoleColor.Green);
                    }
                    else
                    {
                        logFatalWriter("  Error obtaining Token");
                        cleanReturn(ExitCode.UAAIssue);
                    }
                }
                catch (Exception ex)
                {
                    logFatalWriter(string.Format("\n\n***\n{0}\n***\n\n", ex.ToString()));
                    cleanReturn(ExitCode.UAAIssue);
                }
                
                try
                {
                    deviceList = await EdgeManagerHelper.GetDeviceList(edgeManagerBaseUrl, accessToken);
                }
                catch (Exception ex)
                {
                    logFatalWriter(string.Format("\n\n***\n{0}\n***\n\n", ex.ToString()));
                    lastExitCode = ExitCode.EdgeManagerIssue;
                    return null;
                }
            }


            logInfoWriter("  Found " + deviceList.Devices.Count() + " devices.", ConsoleColor.Green);

            logInfoWriter("  Gathering device details...");

            var deviceDetailsList = new List<DeviceDetails>();

            //ok now for each device gets its details..  it will be time consuming!!
            foreach (var device in deviceList.Devices)
            {
                DeviceDetails deviceDetails = null;

                deviceDetails = await EdgeManagerHelper.GetSingleDeviceDetails(edgeManagerBaseUrl, accessToken, device.did);

                if (deviceDetails != null)
                {
                    deviceDetailsList.Add(deviceDetails);
                }
                else
                {
                    logErrorWriter(string.Format("Cannot load the device details for {0} - {1}", device.name, device.deviceUUID));
                }
            }

            return deviceDetailsList;
        }


        private static void cleanReturn(ExitCode exitCode)
        {
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("End with exit code: {0}", (int)exitCode);
            Environment.Exit((int)exitCode);
        }
        
        private static void logInfoWriter(string content, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            logger.Info(content);
        }

        private static void logErrorWriter(string content, ConsoleColor color = ConsoleColor.Red)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            logger.Error(content);

        }

        private static void logFatalWriter(string content, ConsoleColor color = ConsoleColor.DarkRed)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            logger.Fatal(content);
        }
    }

}