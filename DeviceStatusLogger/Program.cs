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

namespace DeviceStatusLogger
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

            logInfoWriter("-------------------------------------------");
            logInfoWriter(" Device Status Logger v" + versionNumber);
            logInfoWriter("-------------------------------------------");
            logInfoWriter();

            Environment.SetEnvironmentVariable("baseUAAUrl", "https://schindler-dev.predix-uaa.run.aws-eu-central-1-pr.ice.predix.io");
            Environment.SetEnvironmentVariable("clientID", "em-d-app-client");
            Environment.SetEnvironmentVariable("clientSecret", "9d9T4vthclUgS6J");
            Environment.SetEnvironmentVariable("edgeManagerBaseUrl", "https://em-d.schindler.edgemanager-d.run.aws-eu-central-1-pr.ice.predix.io");

            Environment.SetEnvironmentVariable("csvFilePath", "C:\\Users\\dev\\Documents\\DeviceStatusExport_06_06_2017.csv");

            string baseUAAUrl = Environment.GetEnvironmentVariable("baseUAAUrl");
            string clientID = Environment.GetEnvironmentVariable("clientID");
            string clientSecret = Environment.GetEnvironmentVariable("clientSecret");
            string edgeManagerBaseUrl = Environment.GetEnvironmentVariable("edgeManagerBaseUrl");
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
                    MainAsync(baseUAAUrl, clientID, clientSecret, edgeManagerBaseUrl, csvFilePath).Wait();
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


        static async Task MainAsync(string baseUAAUrl, string clientID, string clientSecret, string edgeManagerBaseUrl, string csvFilePath)
        {
            logger.Debug("Entering MainAsync");

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
                cleanReturn(ExitCode.EdgeManagerIssue);
                return;
            }

            logInfoWriter("  Found " + deviceList.Devices.Count() + " devices.", ConsoleColor.Green);

            List<DeviceDetails> deviceDetailsList = null;
                         
            deviceDetailsList = new List<DeviceDetails>();
            
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

            var deviceCsvList = from device in deviceList.Devices
                                select DeviceStatusListCSV.FromDevice(device, deviceDetailsList);

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

            cleanReturn(ExitCode.Success);
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