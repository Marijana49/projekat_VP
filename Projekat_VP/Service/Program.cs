using System;
using System.ServiceModel;

namespace Service
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Pokrećem simulaciju prekida veze...");
            ResourceControl.TextManipulation.TestDisposeProof();
            Console.WriteLine("Simulacija završena.\n");

            ServiceHost host = new ServiceHost(typeof(ServiceContract));
            host.Open();

            Console.WriteLine("Service is open, press any key to close it.");
            Console.ReadKey();

            host.Close();
            Console.WriteLine("Service is closed!");
        }
    }
}
