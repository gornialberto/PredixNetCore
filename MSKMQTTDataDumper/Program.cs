using log4net;
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
using System.Globalization;

namespace MSKMQTTDataDumper
{
    class Program
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));

        private static List<MSKMQTTRawData> rawDataBuffer = new List<MSKMQTTRawData>();

        private static bool acquireData = true;

        private static long messageSequence = 0;

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
            //Environment.SetEnvironmentVariable("mqttAddress", "localhost");
            //Environment.SetEnvironmentVariable("mqttPort", "1883");
            //Environment.SetEnvironmentVariable("csvOutputPath", ".\\mqttDump.csv");

            string mqttAdapterConfiguration = Environment.GetEnvironmentVariable("mqttAdapterConfiguration");
            string mqttAddress = Environment.GetEnvironmentVariable("mqttAddress");
            string mqttPort = Environment.GetEnvironmentVariable("mqttPort");
            string csvOutputPath = Environment.GetEnvironmentVariable("csvOutputPath");

            string acquisitionSeconds = Environment.GetEnvironmentVariable("acquisitionSeconds");

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

            if (string.IsNullOrEmpty(acquisitionSeconds))
            {
                string errMsg = string.Format("Acquisition seconds parameter is empty");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                inputValid = false;
            }

            if (inputValid)
            {
                LoggerHelper.LogInfoWriter(logger, "All needed input is provided.", ConsoleColor.Green);

                try
                {
                    //now execute the async part...              
                    MainAsync(mqttAdapterConfiguration, mqttAddress, mqttPort, csvOutputPath, acquisitionSeconds).Wait();
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

        static async Task MainAsync(string mqttAdapterConfiguration, string mqttAddress,
            string mqttPort, string csvOutputPath, string acquisitionSeconds)
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

            LoggerHelper.LogInfoWriter(logger, "Connecting to " + mqttAddress + ":" + mqttPort);

            try
            {
                mqttClient = new uPLibrary.Networking.M2Mqtt.MqttClient(mqttAddress, int.Parse(mqttPort), false, null, null, uPLibrary.Networking.M2Mqtt.MqttSslProtocols.None);

                mqttClient.Connect("MSK-MQTT-DataDumper");

                LoggerHelper.LogInfoWriter(logger, "Connected to the Broker!");
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

            LoggerHelper.LogInfoWriter(logger, "Subscribing...");

            //subscribe...
            mqttClient.Subscribe(topicToSubscribe.ToArray(), qos.ToArray());

            LoggerHelper.LogInfoWriter(logger,"Starting listening to data...");

            DateTime start = DateTime.UtcNow;

            double acquisitionSecondsDouble = double.Parse(acquisitionSeconds);

            while (acquireData)
            {
                await Task.Delay(100);

                if ((DateTime.UtcNow - start) > TimeSpan.FromSeconds(acquisitionSecondsDouble))
                {
                    acquireData = false;
                }
            }

            mqttClient.Unsubscribe(topicToSubscribe.ToArray());

            LoggerHelper.LogInfoWriter(logger, "Ok some data was acquired now creating CSV");


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
                    //now split the data bucket..
                    var sampleData = sample.Split(';');
                    var timeStamp = sampleData[0];

                    
                    for (int index = 1; index < sampleData.Length; index++)
                    {
                        MSKCsvData csvRow = new MSKCsvData();
                        csvData.Add(csvRow);

                        csvRow.MQTTMessageSequence = rawData.Sequence.ToString();
                        csvRow.MQTTMessageTimeStamp = rawData.TimeStamp.ToString("yyyy'-'MM'-'dd HH':'mm':'ss':'fff");
                        csvRow.MQTTMessageUnixTime = DateTimeHelper.DateTimeToUnixTime(rawData.TimeStamp).ToString();

                        csvRow.TimeStamp = timeStamp;
                        csvRow.MSKID = mskID;
                        csvRow.SensorID = sensorID;
                        csvRow.Value = sampleData[index];

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
                    }
                }
            }

            //csvData contains the whole samples...

            try
            {
                using (var csvFileStream = System.IO.File.Create(csvOutputPath + ".csv"))
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

                LoggerHelper.LogInfoWriter(logger, "Work done!");
            }
            catch (Exception ex)
            {
                var msg = string.Format("An error occurred writing CSV file.\n{0}", ex);
                LoggerHelper.LogFatalWriter(logger, msg);
                cleanReturn(ExitCode.ErrorWritingCsv);
            }

            //calculate some statistic...
            List<MQTTStatistics> statistics = new List<MQTTStatistics>();

            //get the topic list
            var topics = (from item in rawDataBuffer
                          select item.Topic).Distinct();

            foreach (var topic in topics)
            {
                //Get the message for this topic
                var messages = (from item in rawDataBuffer
                               where item.Topic == topic
                               orderby item.Sequence
                               select item).ToList();

                DateTime previousTimeStamp = messages[0].TimeStamp;

                TimeSpan sumOfDelay = new TimeSpan(0);

                for (int index = 1; index < messages.Count; index++)
                {
                    TimeSpan delta = messages[index].TimeStamp - previousTimeStamp;
                    sumOfDelay = sumOfDelay + delta;
                    previousTimeStamp = messages[index].TimeStamp;
                }

                double avarageDeltaS =  (sumOfDelay.TotalMilliseconds / (double)messages.Count) / 1000.0;

                double messageFrequency = 1.0 / avarageDeltaS;

                MQTTStatistics stat = new MQTTStatistics();
                statistics.Add(stat);
                stat.Name = "Message Frequency (Hz)";
                stat.Subject = topic;
                stat.Value = messageFrequency.ToString();
            }

            try
            {
                using (var csvFileStream = System.IO.File.Create(csvOutputPath + "_stat.csv"))
                {
                    using (var csvFileWriter = new System.IO.StreamWriter(csvFileStream))
                    {
                        using (CsvHelper.CsvWriter csvWriter = new CsvHelper.CsvWriter(csvFileWriter))
                        {
                            csvWriter.WriteHeader<MQTTStatistics>();

                            foreach (var stat in statistics)
                                csvWriter.WriteRecord<MQTTStatistics>(stat);

                            csvFileWriter.Flush();
                            csvFileStream.Flush();
                        }
                    }
                }

                LoggerHelper.LogInfoWriter(logger, "Work done!");
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

            lock(rawDataBuffer)
            {
                if (acquireData)
                {
                    rawDataBuffer.Add(new MSKMQTTRawData() { Sequence = messageSequence++, TimeStamp = DateTime.UtcNow, Topic = e.Topic, Payload = messageString });
                }
            }           
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