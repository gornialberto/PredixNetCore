using log4net;
using log4net.Config;
using PredixCommon;
using PredixCommon.Entities;
using PredixCommon.Entities.EdgeManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DeployDeviceModels
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
            logInfoWriter(" Deploy Device Models v" + versionNumber);
            logInfoWriter("-------------------------------------------");
     


            string baseUAAUrl = Environment.GetEnvironmentVariable("baseUAAUrl");
            string clientID = Environment.GetEnvironmentVariable("clientID");
            string clientSecret = Environment.GetEnvironmentVariable("clientSecret");
            string edgeManagerBaseUrl = Environment.GetEnvironmentVariable("edgeManagerBaseUrl");
            string deviceModelCsvPath = Environment.GetEnvironmentVariable("deviceModelCsvPath");
            string imageCsvPath = Environment.GetEnvironmentVariable("imageCsvPath");
            string iconCsvPath = Environment.GetEnvironmentVariable("iconCsvPath");

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

            if (string.IsNullOrEmpty(edgeManagerBaseUrl))
            {
                string errMsg = string.Format("Edge Manager Base Url parameter is empty");
                logFatalWriter(errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(deviceModelCsvPath))
            {
                string errMsg = string.Format("Device Model CSV Path parameter is empty");
                logFatalWriter(errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(imageCsvPath))
            {
                string errMsg = string.Format("Image CSV Path parameter is empty");
                logFatalWriter(errMsg);
                inputValid = false;
            }

            if (string.IsNullOrEmpty(iconCsvPath))
            {
                string errMsg = string.Format("Icon CSV Path parameter is empty");
                logFatalWriter(errMsg);
                inputValid = false;
            }

            if (inputValid)
            {
                try
                {
                    //now execute the async part...              
                    MainAsync(baseUAAUrl, clientID, clientSecret, edgeManagerBaseUrl, deviceModelCsvPath, imageCsvPath, iconCsvPath).Wait();
                }
                catch (Exception ex)
                {
                    logFatalWriter("There was an error during the execution of the tool.\n" + ex.ToString());
                }
            }
            else
            {
                string errMsg = string.Format("Some parameters is missing. Cannot execute the tool!");
                logFatalWriter(errMsg);
            }
        }

        static async Task MainAsync(string baseUAAUrl, string clientID, string clientSecret, string edgeManagerBaseUrl, 
            string deviceModelCsvPath, string imageCsvPath, string iconCsvPath)
        {
            logger.Debug("Entering MainAsync");
           
            List<DeviceModelCSV> deviceModelCSVList;

            #region Read DeviceModel list CSV

            try
            {
                using (var csvFileStream = System.IO.File.OpenRead(deviceModelCsvPath))
                {
                    using (var csvFileReader = new System.IO.StreamReader(csvFileStream))
                    {
                        using (CsvHelper.CsvReader csvReader = new CsvHelper.CsvReader(csvFileReader))
                        {
                            deviceModelCSVList = csvReader.GetRecords<DeviceModelCSV>().ToList();
                        }
                    }
                }

                logInfoWriter("Device Model CSV parsed properly!");
            }
            catch (Exception ex)
            {
                var msg = string.Format("An error occurred reading Device Model CSV file.\n{0}", ex);
                logFatalWriter(msg);
                throw;
            }

            #endregion

            List<Base64DataCSV> imageCSVList;

            #region Read Images list CSV

            try
            {
                using (var csvFileStream = System.IO.File.OpenRead(imageCsvPath))
                {
                    using (var csvFileReader = new System.IO.StreamReader(csvFileStream))
                    {
                        using (CsvHelper.CsvReader csvReader = new CsvHelper.CsvReader(csvFileReader))
                        {
                            imageCSVList = csvReader.GetRecords<Base64DataCSV>().ToList();
                        }
                    }
                }

                logInfoWriter("Image CSV parsed properly!");
            }
            catch (Exception ex)
            {
                var msg = string.Format("An error occurred reading Image CSV file.\n{0}", ex);
                logFatalWriter(msg);
                throw;
            }

            #endregion

            List<Base64DataCSV> iconCSVList;

            #region Read Icon list CSV

            try
            {
                using (var csvFileStream = System.IO.File.OpenRead(imageCsvPath))
                {
                    using (var csvFileReader = new System.IO.StreamReader(csvFileStream))
                    {
                        using (CsvHelper.CsvReader csvReader = new CsvHelper.CsvReader(csvFileReader))
                        {
                            iconCSVList = csvReader.GetRecords<Base64DataCSV>().ToList();
                        }
                    }
                }

                logInfoWriter("Icon CSV parsed properly!");
            }
            catch (Exception ex)
            {
                var msg = string.Format("An error occurred reading Icon CSV file.\n{0}", ex);
                logFatalWriter(msg);
                throw;
            }

            #endregion

            if (deviceModelCSVList != null && imageCSVList != null && iconCSVList != null)
            {
                try
                {
                    logInfoWriter("Getting Access Token for ClientID: " + clientID);

                    UAAToken accessToken = await UAAHelper.GetClientCredentialsGrantAccessToken(baseUAAUrl, clientID, clientSecret);

                    logInfoWriter("Token obtained!");
                    
                    foreach (var deviceModelCsv in deviceModelCSVList)
                    {
                        //the ToDeviceModel internally use the static DeviceImageCatalog object prefilled with the images before in the code
                        var deviceModel = deviceModelCsv.ToDeviceModel(imageCSVList,iconCSVList);

                        await EdgeManagerHelper.AddOrUpdateDeviceModel(edgeManagerBaseUrl, accessToken, deviceModel);
                    }
                }
                catch (Exception ex)
                {
                    logger.Fatal("An error occurred working with the Cloud.", ex);
                    throw;
                }
            }
            
        }

        private static void logInfoWriter(string content)
        {
            Console.WriteLine(content);
            logger.Info(content);
        }

        private static void logFatalWriter(string content)
        {
            Console.WriteLine(content);
            logger.Fatal(content);
        }
        
    }
}