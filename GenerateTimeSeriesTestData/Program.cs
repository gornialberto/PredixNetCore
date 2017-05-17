using IngestCSVDataIntoTimeSeries;
using log4net;
using log4net.Config;
using PredixCommon;
using PredixCommon.Entities;
using PredixCommon.Entities.TimeSeries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using CsvHelper;

namespace GenerateTimeSeriesTestData
{
    class Program
    {
        private static ILog logger = LogManager.GetLogger(typeof(Program));

        private static string _csvFilePath;

        private static int _fileSize;

        static void Main(string[] args)
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            string versionNumber = "1.0";

            logger.Debug("App Started");

            Console.WriteLine("-------------------------------------------");
            Console.WriteLine(" Data Generator Test for TimeSeries v " + versionNumber);
            Console.WriteLine("-------------------------------------------");
            Console.WriteLine();


            //Environment.SetEnvironmentVariable("csvFilePath", "C:\\Users\\dev\\Documents\\TimeSeriesData");
            //Environment.SetEnvironmentVariable("startTime", "2017/05/01");
            //Environment.SetEnvironmentVariable("endTime", "2017/05/15");
            //Environment.SetEnvironmentVariable("numberOfTags", "1000");
            //Environment.SetEnvironmentVariable("eventPeriod", "30.0"); //every X seconds it happens an event
            //Environment.SetEnvironmentVariable("fileSize", "100000");



            int numberOfTags = int.Parse(Environment.GetEnvironmentVariable("numberOfTags"));
            double eventPeriod = double.Parse(Environment.GetEnvironmentVariable("eventPeriod"));
           
            _csvFilePath = Environment.GetEnvironmentVariable("csvFilePath");
            _fileSize = int.Parse(Environment.GetEnvironmentVariable("fileSize"));
            
            DateTime startTime = DateTime.Parse(Environment.GetEnvironmentVariable("startTime"));
            DateTime endTime = DateTime.Parse(Environment.GetEnvironmentVariable("endTime"));

            Random rnd = new Random();

              
            for (int tagId = 0; tagId < numberOfTags; tagId++)
            {  
                var tagName = string.Format("Sample-Tag-{0}", tagId);

                DateTime currentTime = startTime;

                while (currentTime < endTime)
                {
                    //generate a new sample!
                    var value = 200.0 * rnd.NextDouble() - 100.0;

                    TimeSeriesDataCSV sample = new TimeSeriesDataCSV();
                    sample.TagName = tagName;
                    sample.TimeStamp = (long)DateTimeHelper.DateTimeToUnixTime(currentTime);
                    sample.Value = value.ToString(CultureInfo.InvariantCulture);

                    writeSampleInFile(sample);

                    currentTime += TimeSpan.FromSeconds(eventPeriod);
                }
            }

            logInfoWriter("Data Generated!");
        }


        private static int _dataFileIndex = 0;

        private static FileStream _currentFileStream;
        private static StreamWriter _currentCsvFileWriter;
        private static CsvHelper.CsvWriter _currentCsvWriter;
        

        private static void writeSampleInFile(TimeSeriesDataCSV sample)
        {
            if (_currentFileStream == null)
            {
                //create all for the very fist time...

                openDataFile();
            }

            //write data
            _currentCsvWriter.WriteRecord<TimeSeriesDataCSV>(sample);
           
            var actualFileSize = _currentCsvFileWriter.BaseStream.Length;

            if (actualFileSize >= _fileSize)
            {
                flushData();
               
                //recreate new stuff!!
                _dataFileIndex++;

                openDataFile();
            }
        }

        private static void openDataFile()
        {
            var fileName = string.Format("DataFile-{0}", _dataFileIndex);

            _currentFileStream = System.IO.File.Create(Path.Combine(_csvFilePath, fileName));
            _currentCsvFileWriter = new System.IO.StreamWriter(_currentFileStream);
            _currentCsvWriter = new CsvHelper.CsvWriter(_currentCsvFileWriter);

            _currentCsvWriter.WriteHeader<TimeSeriesDataCSV>();
        }

        private static void flushData()
        {
            _currentCsvFileWriter.Flush();
            _currentFileStream.Flush();

            _currentCsvFileWriter.Dispose();
            _currentFileStream.Dispose();

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