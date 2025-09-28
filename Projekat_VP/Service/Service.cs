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

            using (ServiceHost host = new ServiceHost(typeof(ServiceContract)))
            {
                host.Open();
                Console.WriteLine("Service is open, press any key to close it.");
                Console.ReadKey();
                host.Close();
                Console.WriteLine("Service is closed!");
            }
        }
    }
}
