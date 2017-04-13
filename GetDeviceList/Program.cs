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

            //This tool requires some parameters:
            //0 - TenantInformation
            //1 - the url of the file with the output analisys

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(" Edge Manager v" + versionNumber);
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine();

            string baseUAAUrl = Environment.GetEnvironmentVariable("baseUAAUrl");
            string clientID = Environment.GetEnvironmentVariable("clientID");
            string clientSecret = Environment.GetEnvironmentVariable("clientSecret");
            string edgeManagerBaseUrl = Environment.GetEnvironmentVariable("edgeManagerBaseUrl");
            string csvFilePath = Environment.GetEnvironmentVariable("csvFilePath");

            try
            {
                //now execute the async part...              
                MainAsync(baseUAAUrl, clientID, clientSecret, edgeManagerBaseUrl, csvFilePath).Wait();
            }
            catch (Exception ex)
            {
                logger.Fatal("Fatal Error see logs for details.",ex);
                logger.Debug(ex.ToString());
            }
        }


        static async Task MainAsync(string baseUAAUrl, string clientID,string clientSecret,string edgeManagerBaseUrl, string csvFilePath)
        {
            System.Diagnostics.Debug.WriteLine("Entering MainAsync");

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
            }
            catch (Exception ex)
            {
                logger.Fatal("An error occurred writing CSV file.", ex);
                throw;
            }

            Console.WriteLine("Work done!");
        }

    }
}