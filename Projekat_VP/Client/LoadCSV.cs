using System;
using Common;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;

namespace Client
{
    public class LoadCSV
    {
        public List<SensorSample> LoadCsv()
        {
            string path = ConfigurationManager.AppSettings["datasetPath"];
            if (string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Dataset path not configured in app.config.");
                return new List<SensorSample>();
            }

            if (!File.Exists(path))
            {
                Console.WriteLine("Dataset file doesn't exist in the set path.");
                return new List<SensorSample>();
            }

            var samples = new List<SensorSample>();
            string logPath = ConfigurationManager.AppSettings["logPath"] ?? "invalid_rows.log";

            using (var logWriter = new StreamWriter(logPath, append: true))
            using (var reader = new StreamReader(path))
            {
                string header = reader.ReadLine();
                int rowCount = 0;

                while (!reader.EndOfStream && rowCount < 100)
                {
                    string line = reader.ReadLine();
                    rowCount++;

                    try
                    {
                        var sample = ParseLine(line);
                        samples.Add(sample);
                    }
                    catch (Exception ex)
                    {
                        logWriter.WriteLine($"[Row {rowCount}] {line} -> {ex.Message}");
                    }
                }
                if (!reader.EndOfStream)
                {
                    logWriter.WriteLine($"Dataset has more than a 100 rows. Excess is ignored.");
                }
            }

            Console.WriteLine($"Valid rows read: {samples.Count}");
            return samples;
        }

        private SensorSample ParseLine(string line)
        {
            var parts = line.Split(',');

            return new SensorSample
            {
                volume = double.Parse(parts[1], CultureInfo.InvariantCulture),
                co = double.Parse(parts[8], CultureInfo.InvariantCulture),
                no2 = double.Parse(parts[9], CultureInfo.InvariantCulture),
                pressure = double.Parse(parts[4], CultureInfo.InvariantCulture),
                dateTime = DateTime.Parse(parts[0], CultureInfo.InvariantCulture)
            };
        }
    }
}
