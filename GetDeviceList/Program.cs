using log4net;
using PredixCommon;
using PredixCommon.Entities;
using PredixCommon.Entities.EdgeManager;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GetDeviceList
{
    class Program
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            string versionNumber = "1.0";

            logger.Debug("App Started");

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(" Get Device Listv" + versionNumber);
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine();
            
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

            if ( inputValid)
            {
                try
                {
                    //now execute the async part...              
                    MainAsync(baseUAAUrl, clientID, clientSecret, edgeManagerBaseUrl, csvFilePath).Wait();
                }
                catch (Exception ex)
                {
                    logger.Fatal("Fatal Error see logs for details.",ex);
                    logger.Debug(ex.ToString());

                    Console.WriteLine("There was an error during the execution of the tool.\n" + ex.ToString());
                }
            }
            else
            {
                string errMsg = string.Format("Some parameters is missing. Cannot execute the tool!");
                logger.Fatal(errMsg);
                Console.WriteLine();
                Console.WriteLine(errMsg);
            }
        }


        static async Task MainAsync(string baseUAAUrl, string clientID,string clientSecret,string edgeManagerBaseUrl, string csvFilePath)
        {
            logger.Debug("Entering MainAsync");

            Console.WriteLine("Getting Access Token for ClientID: " + clientID);

            UAAToken accessToken = await UAAHelper.GetClientCredentialsGrantAccessToken(baseUAAUrl, clientID, clientSecret);

            Console.WriteLine("Token obtained!");

            Console.WriteLine("Querying Edge Manager for Device List: " + edgeManagerBaseUrl);

            //get the list of tags
            DeviceList deviceList = await EdgeManagerHelper.GetDeviceList(edgeManagerBaseUrl,accessToken);

            Console.WriteLine("Found " + deviceList.Devices.Count() + " devices.");

            var deviceCsvList = from device in deviceList.Devices
                                select DeviceListCSV.FromDevice(device);

            try
            {
                using (var csvFileStream = System.IO.File.Create(csvFilePath))
                {
                    using (var csvFileWriter = new System.IO.StreamWriter(csvFileStream))
                    {
                        using (CsvHelper.CsvWriter csvWriter = new CsvHelper.CsvWriter(csvFileWriter))
                        {
                            csvWriter.WriteHeader<DeviceListCSV>();

                            foreach (var deviceCsv in deviceCsvList)
                                csvWriter.WriteRecord<DeviceListCSV>(deviceCsv);

                            csvFileWriter.Flush();
                            csvFileStream.Flush();
                        }
                    }
                }

                Console.WriteLine("Work done!");
            }
            catch (Exception ex)
            {
                logger.Fatal("An error occurred writing CSV file.", ex);
                throw;
            }
        }

    }
}