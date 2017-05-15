using log4net;
using log4net.Config;
using PredixCommon;
using PredixCommon.Entities;
using System;
using System.IO;
using System.Linq;
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

            if (string.IsNullOrEmpty(timeSeriesBaseUrl))
            {
                string errMsg = string.Format("Time Series Base Url parameter is empty");
                logger.Fatal(errMsg);
                Console.WriteLine(errMsg);
                inputValid = false;
            }


            if (string.IsNullOrEmpty(timeSeriesWSSBaseUrl))
            {
                string errMsg = string.Format("Time Series Web Socket Base Url parameter is empty");
                logger.Fatal(errMsg);
                Console.WriteLine(errMsg);
                inputValid = false;
            }
            
            if (string.IsNullOrEmpty(timeSeriesZoneId))
            {
                string errMsg = string.Format("Time Series Zone Id parameter is empty");
                logger.Fatal(errMsg);
                Console.WriteLine(errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(csvFilePath))
            {
                string errMsg = string.Format("CSV Path parameter is empty");
                logger.Fatal(errMsg);
                Console.WriteLine(errMsg);
                inputValid = false;
            }

            if (inputValid)
            {
                try
                {
                    //now execute the async part...              
                    MainAsync(baseUAAUrl, clientID, clientSecret, timeSeriesBaseUrl, timeSeriesWSSBaseUrl, timeSeriesZoneId, csvFilePath).Wait();
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

            Console.Write("Hit Enter to quit...");
            Console.ReadLine();
        }


        static async Task MainAsync(string baseUAAUrl, string clientID, string clientSecret, string timeSeriesBaseUrl, string timeSeriesWSSBaseUrl, string timeSeriesZoneId, string csvFilePath)
        {
            logger.Debug("Entering MainAsync");

            logInfoWriter("Getting Access Token for ClientID: " + clientID);

            UAAToken accessToken = await UAAHelper.GetClientCredentialsGrantAccessToken(baseUAAUrl, clientID, clientSecret);

            logInfoWriter("Token obtained!");

            logInfoWriter("Querying Time Series for Tag List.  TS Url: " + timeSeriesBaseUrl);

            var listOfTags = await TimeSeriesHelper.GetFullTagListOfTimeSeriesZoneId(timeSeriesBaseUrl, timeSeriesZoneId, accessToken);

            logInfoWriter(string.Format("Discovered {0} tags.", listOfTags.Tags.Count()));

            var webSocket = await TimeSeriesHelper.GetWebSocketConnection(timeSeriesWSSBaseUrl, timeSeriesZoneId, accessToken);
        }


        private static void logInfoWriter(string content)
        {
            Console.WriteLine(content);
            logger.Info(content);
        }

        private static void logErrorWriter(string content)
        {
            Console.WriteLine(content);
            logger.Error(content);

        }
        private static void logFatalWriter(string content)
        {
            Console.WriteLine(content);
            logger.Fatal(content);
        }
    }
}