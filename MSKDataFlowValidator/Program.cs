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

namespace MSKDataFlowValidator
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

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(" MSKExtractTimeSeriesSamples v " + versionNumber);
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine();

            Environment.SetEnvironmentVariable("baseUAAUrl", "https://62423ac4-7e97-4840-ab38-5548ab9b4719.predix-uaa.run.aws-usw02-pr.ice.predix.io");
            Environment.SetEnvironmentVariable("clientID", "app_client");
            Environment.SetEnvironmentVariable("clientSecret", "Qwerty1234%");
            Environment.SetEnvironmentVariable("timeSeriesBaseUrl", "https://time-series-store-predix.run.aws-usw02-pr.ice.predix.io");
            Environment.SetEnvironmentVariable("timeSeriesWSSBaseUrl", "wss://gateway-predix-data-services.run.aws-usw02-pr.ice.predix.io");
            Environment.SetEnvironmentVariable("timeSeriesZoneId", "7b9a7ea6-1c65-45e2-b414-0bc4ee2326e6");

            Environment.SetEnvironmentVariable("mqttAdapterConfiguration", "com.ge.dspmicro.machineadapter.mqtt-0.xml");
            Environment.SetEnvironmentVariable("mqttAddress", "localhost");
            Environment.SetEnvironmentVariable("mqttPort", "1883");


            string baseUAAUrl = Environment.GetEnvironmentVariable("baseUAAUrl");
            string clientID = Environment.GetEnvironmentVariable("clientID");
            string clientSecret = Environment.GetEnvironmentVariable("clientSecret");
            string timeSeriesBaseUrl = Environment.GetEnvironmentVariable("timeSeriesBaseUrl");
            string timeSeriesWSSBaseUrl = Environment.GetEnvironmentVariable("timeSeriesWSSBaseUrl");
            string timeSeriesZoneId = Environment.GetEnvironmentVariable("timeSeriesZoneId");

            string mqttAdapterConfiguration = Environment.GetEnvironmentVariable("mqttAdapterConfiguration");
            string mqttAddress = Environment.GetEnvironmentVariable("mqttAddress");
            string mqttPort = Environment.GetEnvironmentVariable("mqttPort");


            bool inputValid = true;

            if (string.IsNullOrEmpty(baseUAAUrl))
            {
                string errMsg = string.Format("Base UAA Url parameter is empty");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(clientID))
            {
                string errMsg = string.Format("Client ID parameter is empty");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                string errMsg = string.Format("Client Secret parameter is empty");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(timeSeriesBaseUrl))
            {
                string errMsg = string.Format("Time Series Base Url parameter is empty");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                inputValid = false;
            }


            if (string.IsNullOrEmpty(timeSeriesWSSBaseUrl))
            {
                string errMsg = string.Format("Time Series Web Socket Base Url parameter is empty");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(timeSeriesZoneId))
            {
                string errMsg = string.Format("Time Series Zone Id parameter is empty");
                LoggerHelper.LogFatalWriter(logger, errMsg);
                inputValid = false;
            }

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

            if (inputValid)
            {
                LoggerHelper.LogInfoWriter(logger, "All needed input is provided.", ConsoleColor.Green);

                try
                {
                    //now execute the async part...              
                    MainAsync(baseUAAUrl, clientID, clientSecret, timeSeriesBaseUrl, 
                        timeSeriesWSSBaseUrl, timeSeriesZoneId, mqttAdapterConfiguration, mqttAddress, mqttPort).Wait();
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

        static async Task MainAsync(string baseUAAUrl, string clientID, string clientSecret, string timeSeriesBaseUrl,
            string timeSeriesWSSBaseUrl, string timeSeriesZoneId, string mqttAdapterConfiguration, string mqttAddress, string mqttPort)
        {
            logger.Debug("Entering MainAsync");


            DeviceConfiguration deviceConfiguration = new DeviceConfiguration();

            DeviceConfiguration.MSKConfiguration mskConfiguration = new DeviceConfiguration.MSKConfiguration();
            deviceConfiguration.Add(mskConfiguration);

            mskConfiguration.MSKID = "SCU-1705-300065";
            //mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN01_01" });
            mskConfiguration.Sensors.Add(new DeviceConfiguration.SensorDetails() { SensorsID = "SN01_02"} );
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


            LoggerHelper.LogInfoWriter(logger, "Getting Access Token for ClientID: " + clientID);

            UAAToken accessToken = null;

            try
            {
                accessToken = await UAAHelper.GetClientCredentialsGrantAccessToken(baseUAAUrl, clientID, clientSecret);

                if (accessToken != null)
                {
                    LoggerHelper.LogInfoWriter(logger, "  Token obtained!", ConsoleColor.Green);
                }
                else
                {
                    LoggerHelper.LogFatalWriter(logger, "  Error obtaining Token");
                    cleanReturn(ExitCode.UAAIssue);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogFatalWriter(logger, string.Format("\n\n***\n{0}\n***\n\n", ex.ToString()));
                cleanReturn(ExitCode.UAAIssue);
                return;
            }


            IQueryTimeSettings queryTimeSettings = new StartTimeAgo(TimeSpan.FromMinutes(5));

            List<string> sensors = new List<string>();
            sensors.Add("SN01_01");
            //sensors.Add("SN01_03");

            ITimeSeriesQuery mskQuery = new SchindlerMSK.MSKTimeSeriesQuery("TST-Processor", sensors, queryTimeSettings);

            var query = mskQuery.GetJsonQuery();
            LoggerHelper.LogInfoWriter(logger, query);

            var mskQueryResult = await TimeSeriesHelper.QueryTimeSeries<SchindlerMSK.MSKQueryRawResponse>(timeSeriesBaseUrl,
                timeSeriesZoneId, accessToken, mskQuery);


            var data = mskQueryResult.GetData();
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