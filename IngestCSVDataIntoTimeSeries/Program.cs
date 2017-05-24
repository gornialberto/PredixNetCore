using log4net;
using log4net.Config;
using PredixCommon;
using PredixCommon.Entities;
using PredixCommon.Entities.TimeSeries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading.Tasks;

namespace IngestCSVDataIntoTimeSeries
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
            Console.WriteLine(" Ingest CSV Data Into TimeSeries v " + versionNumber);
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine();
            
            string baseUAAUrl = Environment.GetEnvironmentVariable("baseUAAUrl");
            string clientID = Environment.GetEnvironmentVariable("clientID");
            string clientSecret = Environment.GetEnvironmentVariable("clientSecret");
            string timeSeriesBaseUrl = Environment.GetEnvironmentVariable("timeSeriesBaseUrl");
            string timeSeriesWSSBaseUrl = Environment.GetEnvironmentVariable("timeSeriesWSSBaseUrl");      
            string timeSeriesZoneId = Environment.GetEnvironmentVariable("timeSeriesZoneId");
            string csvFilePath = Environment.GetEnvironmentVariable("csvFilePath");

            bool inputValid = true;

            if (string.IsNullOrEmpty(baseUAAUrl))
            {
                string errMsg = string.Format("Base UAA Url parameter is empty");
                logFatalWriter(errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(clientID))
            {
                string errMsg = string.Format("Client ID parameter is empty");
                logFatalWriter(errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(clientSecret))
            {
                string errMsg = string.Format("Client Secret parameter is empty");
                logFatalWriter(errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(timeSeriesBaseUrl))
            {
                string errMsg = string.Format("Time Series Base Url parameter is empty");
                logFatalWriter(errMsg);
                inputValid = false;
            }


            if (string.IsNullOrEmpty(timeSeriesWSSBaseUrl))
            {
                string errMsg = string.Format("Time Series Web Socket Base Url parameter is empty");
                logFatalWriter(errMsg);
                inputValid = false;
            }
            
            if (string.IsNullOrEmpty(timeSeriesZoneId))
            {
                string errMsg = string.Format("Time Series Zone Id parameter is empty");
                logFatalWriter(errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(csvFilePath))
            {
                string errMsg = string.Format("CSV Path parameter is empty");
                logFatalWriter(errMsg);
                inputValid = false;
            }

            if (inputValid)
            {
                logInfoWriter("All needed input is provided.", ConsoleColor.Green);

                try
                {
                    //now execute the async part...              
                    MainAsync(baseUAAUrl, clientID, clientSecret, timeSeriesBaseUrl, timeSeriesWSSBaseUrl, timeSeriesZoneId, csvFilePath).Wait();
                }
                catch (Exception ex)
                {
                    string errMsg = string.Format("There was an error during the execution of the tool.\n\n***\n{0}\n***\n\n", ex);
                    logFatalWriter(errMsg);
                    cleanReturn(ExitCode.UnknownIssue);
                }
            }
            else
            {
                string errMsg = string.Format("Some parameters is missing. Cannot execute the tool!");
                logFatalWriter(errMsg);
                cleanReturn(ExitCode.MissingParameters);
            }
        }


        static async Task MainAsync(string baseUAAUrl, string clientID, string clientSecret, string timeSeriesBaseUrl, string timeSeriesWSSBaseUrl, string timeSeriesZoneId, string csvFilePath)
        {
            logger.Debug("Entering MainAsync");


            logInfoWriter("Getting the CSV list...");

            //get the list of CSV file...  then process one file at time...

            var fileList = Directory.GetFiles(csvFilePath, "*.csv", SearchOption.TopDirectoryOnly);

            logInfoWriter(string.Format("  Found {0} csv files.", fileList.Count()), ConsoleColor.Green);

            if (fileList.Count() == 0)
            {
                cleanReturn(ExitCode.NoFileToProcess);
                return;
            }
                
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

            ClientWebSocket webSocket = null;

            try
            {
                webSocket = await TimeSeriesHelper.GetWebSocketConnection(timeSeriesWSSBaseUrl, timeSeriesZoneId, accessToken);
            }
            catch (Exception ex)
            {
                logFatalWriter(string.Format("\n\n***\n{0}\n***\n\n", ex.ToString()));
                cleanReturn(ExitCode.WebSocketIssue);
                return;
            }

            bool allClear = true;

            //now process the files...
            foreach (var file in fileList.OrderBy(n => n))
            {
                var ok = await processCsvFile(webSocket, file);

                if (!ok)
                    allClear = false;
            }

            if (allClear)
            {
                cleanReturn(ExitCode.Success);
            }
        }

     

        static async Task<bool> processCsvFile(ClientWebSocket webSocket, string csvFilePath)
        {
            List<DataPoints> dataPointsList = new List<DataPoints>();

            logInfoWriter(string.Format("Reading Time Series Data from CSV '{0}'...",csvFilePath));

            try
            {
                using (var csvFileStream = System.IO.File.OpenRead(csvFilePath))
                {
                    using (var csvFileReader = new System.IO.StreamReader(csvFileStream))
                    {
                        using (CsvHelper.CsvReader csvReader = new CsvHelper.CsvReader(csvFileReader))
                        {
                            var timeSeriesData = csvReader.GetRecords<TimeSeriesDataCSV>();

                            //now fill the Data Points list...

                            //group by TagName the data
                            var timeSeriesDataByTagName = timeSeriesData.GroupBy(i => i.TagName);

                            foreach (var dataGroupByTag in timeSeriesDataByTagName)
                            {
                                DataPoints dataPoints = new DataPoints();
                                dataPoints.TagName = dataGroupByTag.Key;

                                var dataPointItems = from item in dataGroupByTag
                                                     select new DataPoint() { TimeStamp = item.TimeStamp,
                                                         Value = item.Value,
                                                         Quality = DataQuality.Good };

                                dataPoints.Values.AddRange(dataPointItems);

                                dataPointsList.Add(dataPoints);
                            }
                        }
                    }
                }

                logInfoWriter(string.Format("  Read all the data from CSV '{0}'.",csvFilePath), ConsoleColor.Green);

                var tagCount = dataPointsList.Count();

                int sampleCount = 0;

                foreach(var dataPoints in dataPointsList)
                     sampleCount += dataPoints.Values.Count();

                logInfoWriter(string.Format("  There are {0} different Tags and a total of {1} samples.", tagCount,sampleCount), ConsoleColor.DarkGreen);
            }
            catch (Exception ex)
            {
                var msg = string.Format("  An error occurred reading CSV '{0}'.\n\n***\n{1}\n***\n\n", csvFilePath, ex);
                logErrorWriter(msg);

                //nothing more to do with this file...
                return false;
            }

            //DataPoints dataPoints = new DataPoints();
            //dataPoints.TagName = "aSampleTag3";
            //dataPoints.Values.Add(new DataPoint() { TimeStamp = (long)DateTimeHelper.DateTimeToUnixTime(DateTime.UtcNow),
            // Value = "100", Quality = DataQuality.Good});

            try
            {
                logInfoWriter("Sending data to Time Series...");

                await TimeSeriesHelper.IngestData(webSocket, dataPointsList);

                logInfoWriter("  Data sent!", ConsoleColor.Green);
            }
            catch (Exception ex)
            {
                logErrorWriter(string.Format("  Error sending data to Time Series.\n\n***\n{0}\n***\n\n", ex.ToString()));
                return false;
            }

            return true;
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