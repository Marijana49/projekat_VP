using System;
using System.ServiceModel;

namespace Service
{
    internal class Service
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Running a disconnection simulation...");
            ResourceControl.TextManipulation.TestDisposeProof();
            Console.WriteLine("Simulation finished.\n");

            ServiceHost host = new ServiceHost(typeof(ServiceContract));
            host.Open();

            ServiceContract service = new ServiceContract();

            service.OnTransferStarted += (s, e) => Console.WriteLine("Event: Transfer has started...");
            service.OnSampleReceived += (s, sample) => Console.WriteLine($"Event: Sample received V={sample.volume}, CO={sample.co}, NO2={sample.no2}, P={sample.pressure}");
            service.OnTransferCompleted += (s, e) => Console.WriteLine("Event: Transfer has completed.");
            service.OnWarningRaised += (s, msg) => Console.WriteLine("Event: Warning: " + msg);

            Console.WriteLine("Service is open, press any key to close it.");
            Console.ReadKey();

            host.Close();
            Console.WriteLine("Service is closed!");
        }
    }
}
