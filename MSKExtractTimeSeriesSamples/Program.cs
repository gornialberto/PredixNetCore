using log4net;
using log4net.Config;
using PredixCommon;
using PredixCommon.Entities;
using PredixCommon.Entities.TimeSeries;
using PredixCommon.Entities.TimeSeries.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MSKExtractTimeSeriesSamples
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

             string baseUAAUrl = Environment.GetEnvironmentVariable("baseUAAUrl");
            string clientID = Environment.GetEnvironmentVariable("clientID");
            string clientSecret = Environment.GetEnvironmentVariable("clientSecret");
            string timeSeriesBaseUrl = Environment.GetEnvironmentVariable("timeSeriesBaseUrl");
            string timeSeriesWSSBaseUrl = Environment.GetEnvironmentVariable("timeSeriesWSSBaseUrl");
            string timeSeriesZoneId = Environment.GetEnvironmentVariable("timeSeriesZoneId");
            
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

            if (inputValid)
            {
                LoggerHelper.LogInfoWriter(logger, "All needed input is provided.", ConsoleColor.Green);

                try
                {
                    //now execute the async part...              
                    MainAsync(baseUAAUrl, clientID, clientSecret, timeSeriesBaseUrl, timeSeriesWSSBaseUrl, timeSeriesZoneId).Wait();
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

        static async Task MainAsync(string baseUAAUrl, string clientID, string clientSecret, string timeSeriesBaseUrl, string timeSeriesWSSBaseUrl, string timeSeriesZoneId)
        {
            logger.Debug("Entering MainAsync");
            
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