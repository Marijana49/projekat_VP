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
        private double no2_sum = 0;
        private double co_sum = 0;
        private double pressure_sum = 0;

        private double no2Threshold = double.Parse(ConfigurationManager.AppSettings["NO2_threshold"]);
        private double coThreshold = double.Parse(ConfigurationManager.AppSettings["CO_threshold"]);
        private double pThreshold = double.Parse(ConfigurationManager.AppSettings["P_threshold"]);

        private double? lastPressure = null;
        private double? lastCO = null;
        private double? lastNO2 = null;
        private double runningPressureSum = 0;
        private int runningPressureCount = 0;

        private StreamWriter sessionWriter;
        private StreamWriter rejectsWriter;

        public ServiceContract()
        {
            OnTransferStarted += (s, e) => Console.WriteLine("Event: Transfer has started...");
            OnSampleReceived += (s, sample) => Console.WriteLine($"Event: Sample received V={sample.volume}, CO={sample.co}, NO2={sample.no2}, P={sample.pressure}");
            OnTransferCompleted += (s, e) => Console.WriteLine("Event: Transfer has completed.");
            OnWarningRaised += (s, msg) => Console.WriteLine("Event: Warning: " + msg);
        }

        public void StartSession(string meta)
        {
            Console.WriteLine("Transmission in progress...");
            sampleCount = 0;
            volumeSum = 0;
            no2_sum = 0;
            co_sum = 0;
            pressure_sum = 0;
            lastPressure = null;
            lastCO = null;
            lastNO2 = null;
            runningPressureSum = 0;
            runningPressureCount = 0;


            OnTransferStarted?.Invoke(this, EventArgs.Empty);

            sessionWriter = new StreamWriter(new FileStream("measurements_session.csv", FileMode.Append, FileAccess.Write, FileShare.Read));
            rejectsWriter = new StreamWriter(new FileStream("rejects.csv", FileMode.Append, FileAccess.Write, FileShare.Read));

            sessionWriter.WriteLine("Volume,CO,NO2,Pressure,DateTime");
            sessionWriter.Flush();

            if (new FileInfo("rejects.csv").Length == 0)
            {
                rejectsWriter.WriteLine("Volume,CO,NO2,Pressure,DateTime,Reason");
                rejectsWriter.Flush();
            }
        }

        public void PushSample(SensorSample sample)
        {
            sampleCount++;
            volumeSum += sample.volume;
            no2_sum += sample.no2;
            co_sum += sample.co;
            pressure_sum += sample.pressure;
            double volume_avg = volumeSum / sampleCount;
            double no2_avg = no2_sum / sampleCount;
            double co_avg = co_sum / sampleCount;
            double pressure_avg = pressure_sum / sampleCount;

            OnSampleReceived?.Invoke(this, sample);

            if (sample.no2 > no2Threshold)
                OnWarningRaised?.Invoke(this, $"NO2 over the limit! ({sample.no2})");
            if (sample.co > coThreshold)
                OnWarningRaised?.Invoke(this, $"CO over the limit! ({sample.co})");
            if (sample.pressure > pThreshold)
                OnWarningRaised?.Invoke(this, $"Pressure over the limit! ({sample.pressure})");

            if (Math.Abs(sample.volume - volume_avg) > 0.25 * volume_avg)
                OnWarningRaised?.Invoke(this, $"Volume varies 25% around the average! ({sample.volume}, avg={volume_avg:F2})");
            if (Math.Abs(sample.no2 - no2_avg) > 0.25 * no2_avg)
                OnWarningRaised?.Invoke(this, $"NO2 varies 25% around the average! ({sample.no2}, avg={no2_avg:F2})");
            if (Math.Abs(sample.co - co_avg) > 0.25 * co_avg)
                OnWarningRaised?.Invoke(this, $"CO varies 25% around the average! ({sample.co}, avg={co_avg:F2})");
            if (Math.Abs(sample.pressure - pressure_avg) > 0.25 * pressure_avg)
                OnWarningRaised?.Invoke(this, $"Pressure varies 25% around the average! ({sample.pressure}, avg={pressure_avg:F2})");
            

            try
            {
                Validation(sample);
                sessionWriter.WriteLine($"{sample.volume},{sample.co},{sample.no2},{sample.pressure},{sample.dateTime}");
                sessionWriter.Flush();
            }
            catch (FaultException<ValidationFault> ex)
            {
                Console.WriteLine($"Validation error: {ex.Detail.Message}");
                rejectsWriter.WriteLine($"{sample.volume},{sample.co},{sample.no2},{sample.pressure},{sample.dateTime},{ex.Detail.Message}");
                rejectsWriter.Flush();
            }

            // --- ΔP spike detection ---
            if (lastPressure.HasValue)
            {
                double deltaP = sample.pressure - lastPressure.Value;
                if (Math.Abs(deltaP) > pThreshold)
                {
                    string direction = deltaP > 0 ? "over the expected value" : "under the expected value";
                    OnWarningRaised?.Invoke(this, $"PressureSpike! ΔP={deltaP:F2} ({direction})");
                }
            }
            lastPressure = sample.pressure;

            // --- Running mean check ---
            runningPressureSum += sample.pressure;
            runningPressureCount++;
            double Pmean = runningPressureSum / runningPressureCount;

            if (sample.pressure < 0.75 * Pmean)
            {
                OnWarningRaised?.Invoke(this, $"OutOfBandWarning: Pressure {sample.pressure} is under the expected value (mean={Pmean:F2})");
            }
            else if (sample.pressure > 1.25 * Pmean)
            {
                OnWarningRaised?.Invoke(this, $"OutOfBandWarning: Pressure {sample.pressure} is over the expected value (mean={Pmean:F2})");
            }

            // --- ΔCO spike detection ---
            if (lastCO.HasValue)
            {
                double deltaCO = sample.co - lastCO.Value;
                if (Math.Abs(deltaCO) > coThreshold)
                {
                    string direction = deltaCO > 0 ? "over the expected value" : "under the expected value";
                    OnWarningRaised?.Invoke(this, $"C0Spike! ΔCO={deltaCO:F2} ({direction})");
                }
            }
            lastCO = sample.co;

            // --- ΔNO2 spike detection ---
            if (lastNO2.HasValue)
            {
                double deltaNO2 = sample.no2 - lastNO2.Value;
                if (Math.Abs(deltaNO2) > no2Threshold)
                {
                    string direction = deltaNO2 > 0 ? "over the expected valueg" : "under the expected value";
                    OnWarningRaised?.Invoke(this, $"NO2Spike! ΔNO2={deltaNO2:F2} ({direction})");
                }
            }
            lastNO2 = sample.no2;

        }

        public void EndSession()
        {
            Console.WriteLine("Transmission ended.");
            sessionWriter?.Close();
            rejectsWriter?.Close();
            OnTransferCompleted?.Invoke(this, EventArgs.Empty);
        }

        public void Validation(SensorSample ss)
        {
            if (ss.volume > 100)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! Volume is over 100!"));
            }
            if (ss.no2 > 16000)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! NO2 concentration is over 16000!"));
            }
            if (ss.co > 280000)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! CO concentration is over 280000!"));
            }
            if (ss.pressure > 1028)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! Pressure is over 1028!"));
            }
            // svi podaci su u redu ali u slucaju da nisu:

            if(ss.volume == null ||  ss.no2 == null || ss.co == null || ss.pressure == null || ss.dateTime == null)
            {
                OnWarningRaised?.Invoke(this, $"Data not valid! ({ss.ToString()})");
                throw new FaultException<DataFormatFault>(new DataFormatFault("Warning! Data is not valid!"));
            }
        }
    }
}
