using Common;
using System.Collections.Generic;
using System.ServiceModel;

namespace Client
{
    internal class Client
    {
        static void Main(string[] args)
        {
            ChannelFactory<ISensorContract> factory = new ChannelFactory<ISensorContract>("ServiceContract");
            LoadCSV loader = new LoadCSV();
            List<SensorSample> samples = loader.LoadCsv();

            ISensorContract proxy = factory.CreateChannel();

            proxy.StartSession("Test session");

            foreach (var sample in samples)
            {
                proxy.PushSample(sample);
                System.Threading.Thread.Sleep(1000); 
            }

            proxy.EndSession();

        }
    }
}
