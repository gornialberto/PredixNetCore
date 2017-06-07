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
            

       
            string baseUAAUrl = Environment.GetEnvironmentVariable("baseUAAUrl");
            string clientID = Environment.GetEnvironmentVariable("clientID");
            string clientSecret = Environment.GetEnvironmentVariable("clientSecret");
            string edgeManagerBaseUrl = Environment.GetEnvironmentVariable("edgeManagerBaseUrl");

            //this is optional if not provided the MQTT address and port is needed! 
            string csvFilePath = Environment.GetEnvironmentVariable("csvFilePath");

            //mqtt server is optional only if not writing to CSV!
            string mqttServerAddress = Environment.GetEnvironmentVariable("mqttServerAddress");


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

            if (string.IsNullOrEmpty(csvFilePath) && string.IsNullOrEmpty(mqttServerAddress))
            {
                string errMsg = string.Format("CSV Path & MQTT Server host parameter are empty! cannot be both empty!");
                logger.Fatal(errMsg);
                Console.WriteLine(errMsg);
                inputValid = false;
            }

            if (inputValid)
            {
                try
                {
                    //now execute the async part...              
                    MainAsync(baseUAAUrl, clientID, clientSecret, edgeManagerBaseUrl, csvFilePath, mqttServerAddress).Wait();
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


        static async Task MainAsync(string baseUAAUrl, string clientID, string clientSecret, string edgeManagerBaseUrl, string csvFilePath, string mqttServerAddress)
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

            if (string.IsNullOrEmpty(mqttServerAddress))
            {
                //just CSV write...
                logInfoWriter("Writing to CSV one shot data capture...");

                deviceDetailsList = await getDeviceDetails(accessToken, edgeManagerBaseUrl);

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
                //send data to MQTT
                logInfoWriter("Starting loop and sending data to MQTT");

                MqttClient mqttClient;

                logInfoWriter(string.Format("Creating MQTT Client pointing to {0} broker.", mqttServerAddress));

                // create client instance
                mqttClient = new MqttClient(mqttServerAddress);

                logInfoWriter("Connecting to MQTT Broker...");

                try
                {
                    //connect to the broker
                    var connectionResult = mqttClient.Connect("DeviceStatusLoggerClient", null, null, true, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
                        true, "deviceStatus/status", "offline", true, 90);

                    logInfoWriter(string.Format("Connection result: {0}", connectionResult));
                }
                catch (Exception ex)
                {
                    logFatalWriter(string.Format("Error connecting to the MQTT Broker.\n{0}", ex.ToString()));
                    return;
                }

                if (mqttClient.IsConnected)
                {
                    logInfoWriter("MQTT Client is connected properly. Updating online status.");

                    //ok just publish you are online properly now!!
                    mqttClient.Publish("deviceStatus/status", Encoding.UTF8.GetBytes("running"), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                    while (true)
                    {
                        deviceDetailsList = await getDeviceDetails(accessToken, edgeManagerBaseUrl);

                        if (deviceDetailsList != null)
                        {
                            var deviceCsvList = (from device in deviceDetailsList
                                                where device.deviceInfoStatus.simInfo != null &&
                                                device.deviceInfoStatus.cellularStatus != null
                                                 select DeviceStatusListCSV.FromDevice(device)).ToList();

                            logInfoWriter(string.Format("  Found {0} devices with Cellular status updated. Sending data to MQTT", deviceCsvList.Count));

                            var timeStamp = DateTimeHelper.DateTimeToUnixTime(DateTime.UtcNow).ToString();

                            foreach (var dev in deviceCsvList)
                            {
                                var topic = string.Format("deviceStatus/{0}/DeviceName", dev.DeviceID);
                                var value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}", 
                                    dev.DeviceName,timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                                                            
                                topic = string.Format("deviceStatus/{0}/mno", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.mno, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                                
                                topic = string.Format("deviceStatus/{0}/IPv6", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.IPv6, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                                
                                topic = string.Format("deviceStatus/{0}/networkMode", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.networkMode, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                                topic = string.Format("deviceStatus/{0}/iccid", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.iccid, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                                topic = string.Format("deviceStatus/{0}/imei", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.imei, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                                topic = string.Format("deviceStatus/{0}/imsi", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.imsi, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                                topic = string.Format("deviceStatus/{0}/rscp", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.rscp, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                                topic = string.Format("deviceStatus/{0}/rsrp", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.rsrp, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                                topic = string.Format("deviceStatus/{0}/rsrq", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.rsrq, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                                topic = string.Format("deviceStatus/{0}/rssi", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.rssi, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                                topic = string.Format("deviceStatus/{0}/sinr", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.sinr, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);

                                topic = string.Format("deviceStatus/{0}/Status", dev.DeviceID);
                                value = Encoding.UTF8.GetBytes(string.Format("{{\"Value\"=\"{0}\",\"TimeStamp\"=\"{1}\"}}",
                                    dev.Status, timeStamp));
                                mqttClient.Publish(topic, value, MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, true);
                            }

                            logInfoWriter("  Data sent to MQTT", ConsoleColor.Green);
                        }

                        System.Threading.Thread.Sleep(TimeSpan.FromMinutes(2));

                    } //end of while...                    
                }
                else
                {
                    logFatalWriter("MQTT Client not connected to the Broker.");
                    Environment.Exit((int)ExitCode.MQTTNotConnected);
                }
                
            }

            cleanReturn(ExitCode.Success);
        }

        



        static async Task<List<DeviceDetails>> getDeviceDetails(UAAToken accessToken, string edgeManagerBaseUrl)
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