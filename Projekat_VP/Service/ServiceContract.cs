using Common;
using System;
using System.Configuration;
using System.IO;
using System.ServiceModel;

namespace Service
{
    public class ServiceContract : ISensorContract
    {
        public delegate void TransferEventHandler(object sender, EventArgs e);
        public delegate void SampleEventHandler(object sender, SensorSample e);
        public delegate void WarningEventHandler(object sender, string message);

        public event TransferEventHandler OnTransferStarted;
        public event SampleEventHandler OnSampleReceived;
        public event TransferEventHandler OnTransferCompleted;
        public event WarningEventHandler OnWarningRaised;

        private int sampleCount = 0;
        private double volumeSum = 0;

        private double no2Threshold = double.Parse(ConfigurationManager.AppSettings["NO2_threshold"]);
        private double coThreshold = double.Parse(ConfigurationManager.AppSettings["CO_threshold"]);
        private double pThreshold = double.Parse(ConfigurationManager.AppSettings["P_threshold"]);


        private static string sessionFilePath;
        private static string rejectsFilePath;
        private static StreamWriter sessionWriter;
        private static StreamWriter rejectsWriter;

        public void EndSession()
        {
            Console.WriteLine("Transmission ended.");

            sessionWriter?.Close();
            rejectsWriter?.Close();
        }

        public void PushSample(SensorSample sample)
        {
            sampleCount++;
            volumeSum += sample.volume;

            double avg = volumeSum / sampleCount;

            OnSampleReceived?.Invoke(this, sample);

            if (sample.no2 > no2Threshold)
                OnWarningRaised?.Invoke(this, $"NO2 over the limit! ({sample.no2})");
            if (sample.co > coThreshold)
                OnWarningRaised?.Invoke(this, $"CO over the limit! ({sample.co})");
            if (sample.pressure > pThreshold)
                OnWarningRaised?.Invoke(this, $"Pressure over the limit! ({sample.pressure})");

            //we can do this for others????
            if (Math.Abs(sample.volume - avg) > 0.25 * avg)
                OnWarningRaised?.Invoke(this, $"Volume varies  25% around the average! ({sample.volume}, avg={avg:F2})");

            try
            {
                Validation(sample);

                sessionWriter.WriteLine($"{sample.volume},{sample.co},{sample.no2},{sample.pressure},{sample.dateTime}");
                sessionWriter.Flush();
            }
            catch (FaultException<ValidationFault> ex)
            {
                rejectsWriter.WriteLine($"{sample.volume},{sample.co},{sample.no2},{sample.pressure},{sample.dateTime},{ex.Detail.Message}");
                rejectsWriter.Flush();
            }
        }

        public void StartSession(string meta)
        {
            Console.WriteLine("Transmission in progress...");

            sampleCount = 0;
            volumeSum = 0;

            OnTransferStarted?.Invoke(this, EventArgs.Empty);

            sessionFilePath = "measurements_session.csv";
            rejectsFilePath = "rejects.csv";

            sessionWriter = new StreamWriter(new FileStream(sessionFilePath, FileMode.Append, FileAccess.Write, FileShare.Read));
            rejectsWriter = new StreamWriter(new FileStream(rejectsFilePath, FileMode.Append, FileAccess.Write, FileShare.Read));

            sessionWriter.WriteLine("Volume,CO,NO2,Pressure,DateTime");
            sessionWriter.Flush();

            if (new FileInfo(rejectsFilePath).Length == 0)
            {
                rejectsWriter.WriteLine("Volume,CO,NO2,Pressure,DateTime,Reason");
                rejectsWriter.Flush();
            }
        }

        public void Validation(SensorSample ss)
        {
            if (ss.volume > 90)
            {
                OnWarningRaised?.Invoke(this, $"Volume over the limit! ({ss.volume})");
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! Volume is over 90!"));
            }
            if (ss.no2 > 16000)
            {
                OnWarningRaised?.Invoke(this, $"NO2 over the limit! ({ss.no2})");
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! NO2 concentration is over 16000!"));
            }
            if (ss.co > 280000)
            {
                OnWarningRaised?.Invoke(this, $"CO over the limit! ({ss.co})");
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! CO concentration is over 280000!"));
            }
            if (ss.pressure > 1028)
            {
                OnWarningRaised?.Invoke(this, $"Pressure over the limit! ({ss.pressure})");
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! Pressure is over 1028!"));
            }
            // in case of textbox add DataFormat warning
        }
    }
}
