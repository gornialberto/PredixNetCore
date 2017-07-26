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

            Environment.SetEnvironmentVariable("csvDataPath", "C:\\Users\\dev\\Documents\\data.csv");
           


            string baseUAAUrl = Environment.GetEnvironmentVariable("baseUAAUrl");
            string clientID = Environment.GetEnvironmentVariable("clientID");
            string clientSecret = Environment.GetEnvironmentVariable("clientSecret");
            string timeSeriesBaseUrl = Environment.GetEnvironmentVariable("timeSeriesBaseUrl");
            string timeSeriesWSSBaseUrl = Environment.GetEnvironmentVariable("timeSeriesWSSBaseUrl");
            string timeSeriesZoneId = Environment.GetEnvironmentVariable("timeSeriesZoneId");

            string csvDataPath = Environment.GetEnvironmentVariable("csvDataPath");
           

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

            if (string.IsNullOrEmpty(csvDataPath))
            {
                string errMsg = string.Format("csvData file path parameter is empty");
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
                        timeSeriesWSSBaseUrl, timeSeriesZoneId, csvDataPath).Wait();
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
            string timeSeriesWSSBaseUrl, string timeSeriesZoneId, string csvDataPath)
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


            List<MSKCsvData> csvData = null;

            try
            {
                using (var csvFileStream = System.IO.File.OpenRead(csvDataPath))
                {
                    using (var csvFileReader = new System.IO.StreamReader(csvFileStream))
                    {
                        using (CsvHelper.CsvReader csvReader = new CsvHelper.CsvReader(csvFileReader))
                        {
                            csvData = csvReader.GetRecords<MSKCsvData>().ToList();
                        }
                    }
                }

                LoggerHelper.LogInfoWriter(logger, "CSV parsed properly!");
            }
            catch (Exception ex)
            {
                var msg = string.Format("An error occurred reading CSV file.\n{0}", ex);
                LoggerHelper.LogFatalWriter(logger, msg);
                throw;
            }



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



                        

            List<MSKSensorValues> dataFromCsv = new List<MSKSensorValues>();



            var mskIdList = (from csvRow in csvData
                             select csvRow.MSKID).Distinct();

            foreach (var mskId in mskIdList)
            {
                var sensorForKit = (from csvRow in csvData
                                   where csvRow.MSKID == mskId
                                   select csvRow.SensorID).Distinct();

                foreach (var sensor in sensorForKit)
                {
                    MSKSensorValues sensorValue = new MSKSensorValues();
                    dataFromCsv.Add(sensorValue);

                    sensorValue.MSKID = mskId;
                    sensorValue.SensorID = sensor;


                    var sensorDimension = (from csvRow in csvData
                                           where csvRow.MSKID == mskId && csvRow.SensorID == sensor
                                           select csvRow.Dimension).Distinct();

                    foreach (var dimension in sensorDimension)
                    {
                        var dimensionData = new MSKSensorValues.DimensionData();
                        sensorValue.DimensionsData.Add(dimensionData);
                        dimensionData.Dimension = dimension;

                        var dimensionDataSample = (from csvRow in csvData
                                          where csvRow.MSKID == mskId && csvRow.SensorID == sensor && csvRow.Dimension == dimension
                                          select csvRow).ToList();

                        foreach (var sample in dimensionDataSample)
                        {
                            dimensionData.Values.Add(new DataPoint()
                            {
                                TimeStamp = long.Parse(sample.TimeStamp),
                                Quality = DataQuality.Good,
                                Value = sample.Value
                            });
                        }
                    }                  
                 }   
            }

            List<MSKSensorValues> dataFromTimeSeries = new List<MSKSensorValues>();

            foreach (var mskId in mskIdList)
            {
                var sensorForKit = (from csvRow in csvData
                                    where csvRow.MSKID == mskId
                                    select csvRow.SensorID).Distinct().ToList();

                foreach (var sensor in sensorForKit)
                {
                    var startTime = (from csvRow in csvData
                                     where csvRow.MSKID == mskId && csvRow.SensorID == sensor
                                     select csvRow.TimeStamp).OrderBy(v => v).First();

                    DateTime st = DateTimeHelper.JavaTimeStampToDateTime(double.Parse( startTime));

                    var endTime = (from csvRow in csvData
                                   where csvRow.MSKID == mskId && csvRow.SensorID == sensor
                                   select csvRow.TimeStamp).OrderBy(v => v).Last();

                    DateTime et = DateTimeHelper.JavaTimeStampToDateTime(double.Parse(endTime));


                    IQueryTimeSettings queryTimeSettings = new ExactStartEndTime(st,et);
                    
                    ITimeSeriesQuery mskQuery = new SchindlerMSK.MSKTimeSeriesQuery(mskId, 
                        new List<string>() { sensor }, queryTimeSettings);

                    var query = mskQuery.GetJsonQuery();
                    
                    var mskQueryResult = await TimeSeriesHelper.QueryTimeSeries<SchindlerMSK.MSKQueryRawResponse>(timeSeriesBaseUrl,
                        timeSeriesZoneId, accessToken, mskQuery);


                    var data = mskQueryResult.GetData();

                    dataFromTimeSeries.AddRange(data);                        
                 }                
            }


            //TO IMPLEMENT AN AUTOMATIC CHECK 

            //dataFromTimeSeries
            //dataFromCsv

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