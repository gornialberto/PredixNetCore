﻿using log4net;
using log4net.Config;
using PredixCommon;
using PredixCommon.Entities;
using PredixCommon.Entities.TimeSeries;
using PredixCommon.Entities.TimeSeries.Query;
using SchindlerMSK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace MSKMQTTDataDumper
{
    class Program
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));

        private static List<MSKMQTTRawData> rawDataBuffer = new List<MSKMQTTRawData>();

        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            string versionNumber = "1.0";

            logger.Debug("App Started");

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(" MSK MQTT Data Dumper v " + versionNumber);
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine();

            Environment.SetEnvironmentVariable("mqttAdapterConfiguration", "com.ge.dspmicro.machineadapter.mqtt-0.xml");
            Environment.SetEnvironmentVariable("mqttAddress", "localhost");
            //Environment.SetEnvironmentVariable("mqttPort", "1883");
            Environment.SetEnvironmentVariable("csvOutputPath", ".\\mqttDump.cv");

            string mqttAdapterConfiguration = Environment.GetEnvironmentVariable("mqttAdapterConfiguration");
            string mqttAddress = Environment.GetEnvironmentVariable("mqttAddress");
            string mqttPort = Environment.GetEnvironmentVariable("mqttPort");
            string csvOutputPath = Environment.GetEnvironmentVariable("csvOutputPath");


            bool inputValid = true;

            if (string.IsNullOrEmpty(mqttAdapterConfiguration))
            {
                string errMsg = string.Format("MQTT Machine Adapter configuration file path parameter is empty");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(mqttAddress))
            {
                string errMsg = string.Format("MQTT Address parameter is empty");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(mqttPort))
            {
                string errMsg = string.Format("MQTT Port parameter is empty");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(csvOutputPath))
            {
                string errMsg = string.Format("CSV Output parameter is empty");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                inputValid = false;
            }

            if (inputValid)
            {
                LoggerHelper.LogInfoWriter(logger, "All needed input is provided.", ConsoleColor.Green);

                try
                {
                    //now execute the async part...              
                    MainAsync(mqttAdapterConfiguration, mqttAddress, mqttPort, csvOutputPath).Wait();
                }
                catch (Exception ex)
                {
                    string errMsg = string.Format("There was an error during the execution of the tool.\n\n***\n{0}\n***\n\n", ex);
                    LoggerHelper.LogFatalWriter(logger, errMsg);
                    cleanReturn(ExitCode.UnknownIssue);
                }
            }
            else
            {
                string errMsg = string.Format("Some parameters is missing. Cannot execute the tool!");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                cleanReturn(ExitCode.MissingParameters);
            }

        }

        static async Task MainAsync(string mqttAdapterConfiguration, string mqttAddress, string mqttPort, string csvOutputPath)
        {
            logger.Debug("Entering MainAsync");


            DeviceConfiguration deviceConfiguration = new DeviceConfiguration();

            DeviceConfiguration.MSKConfiguration mskConfiguration = new DeviceConfiguration.MSKConfiguration();
            deviceConfiguration.Add(mskConfiguration);

            mskConfiguration.MSKID = "SCU-1705-300065";
            //mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN01_01" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN01_02" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN01_03" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN01_04" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN01_05" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN01_06" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN01_07" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN01_08" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN02_01" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN02_02" });
            //mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN02_03" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN02_04" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN02_05" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN02_06" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN02_07" });

            DeviceConfiguration.MSKConfiguration mskConfigurationReduced = new DeviceConfiguration.MSKConfiguration();
            deviceConfiguration.Add(mskConfigurationReduced);

            mskConfigurationReduced.MSKID = "MSK065reduced";
            mskConfigurationReduced.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN01_01" });
            mskConfigurationReduced.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN02_03" });

            var json = deviceConfiguration.GetJSON();



            //ok register to the topic...
            uPLibrary.Networking.M2Mqtt.MqttClient mqttClient = null;


            try
            {
                mqttClient = new uPLibrary.Networking.M2Mqtt.MqttClient(mqttAddress, int.Parse(mqttPort), false, null, null, uPLibrary.Networking.M2Mqtt.MqttSslProtocols.None);

                mqttClient.Connect("MSK-MQTT-DataDumper");
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Can't connect to MQTT");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                cleanReturn(ExitCode.MQTTNotConnected);
            }

            mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;

            List<string> topicToSubscribe = new List<string>();

            foreach (var deviceConf in deviceConfiguration)
            {
                var topic = from sensor in deviceConf.Sensors
                            select string.Format("data/{0}/{1}", deviceConf.MSKID, sensor.SensorsID);

                topicToSubscribe.AddRange(topic);
            }

            var qos = from item in topicToSubscribe
                      select uPLibrary.Networking.M2Mqtt.Messages.MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE;

            //subscribe...
            mqttClient.Subscribe(topicToSubscribe.ToArray(), qos.ToArray());

            LoggerHelper.LogInfoWriter(logger,"Starting listening to data...");

            DateTime start = DateTime.UtcNow;

            bool acquireData = true;

            while (acquireData)
            {
                await Task.Delay(100);

                if ((DateTime.UtcNow - start) > TimeSpan.FromSeconds(60))
                {
                    acquireData = false;
                }
            }

            //ok flush the buffer to the CSV!!
            //MSKSensorValues

            List<MSKCsvData> csvData = new List<MSKCsvData>();

            foreach (var rawData in rawDataBuffer)
            {
                var topicData = rawData.Topic.Split('/');
                var mskID = topicData[1];
                var sensorID = topicData[2];

                var samples = rawData.Payload.Split('@');

                foreach (var sample in samples)
                {
                    MSKCsvData csvRow = new MSKCsvData();
                    csvData.Add(csvRow);

                    //now split the data bucket..
                    var sampleData = sample.Split(';');
                    var timeStamp = sampleData[0];

                    csvRow.TimeStamp = timeStamp;
                    csvRow.MSKID = mskID;
                    csvRow.SensorID = sensorID;

                    for (int index = 1; index < sampleData.Length; index++)
                    {
                        if (sampleData.Length > 2)
                        {
                            if (index == 1)
                            {
                                csvRow.Dimension = "x";
                            }

                            if (index == 2)
                            {
                                csvRow.Dimension = "y";
                            }

                            if (index == 3)
                            {
                                csvRow.Dimension = "z";
                            }
                        }
                        else
                        {
                            if (index == 1)
                            {
                                csvRow.Dimension = "v";
                            }
                        }

                        csvRow.Value = sampleData[index];
                    }
                }
            }

            //csvData contains the whole samples...

            try
            {
                using (var csvFileStream = System.IO.File.Create(csvOutputPath))
                {
                    using (var csvFileWriter = new System.IO.StreamWriter(csvFileStream))
                    {
                        using (CsvHelper.CsvWriter csvWriter = new CsvHelper.CsvWriter(csvFileWriter))
                        {
                            csvWriter.WriteHeader<MSKCsvData>();

                            foreach (var csvRow in csvData)
                                csvWriter.WriteRecord<MSKCsvData>(csvRow);

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
                LoggerHelper.LogFatalWriter(logger, msg);
                cleanReturn(ExitCode.ErrorWritingCsv);
            }

            cleanReturn(ExitCode.Success);
        }

        /// <summary>
        /// Received a message!!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MqttClient_MqttMsgPublishReceived(object sender, 
            uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            string messageString = System.Text.Encoding.UTF8.GetString(e.Message);

            rawDataBuffer.Add(new MSKMQTTRawData() { Topic = e.Topic, Payload = messageString });            
        }

        private static void cleanReturn(ExitCode exitCode)
        {
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("End with exit code: {0}", (int)exitCode);
            Environment.Exit((int)exitCode);
        }

    }
}