using Common;
using System.Collections.Generic;
using System.ServiceModel;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<ISensorContract> factory = new ChannelFactory<ISensorContract>("ServiceContract");
            LoadCSV loader = new LoadCSV();
            List<SensorSample> samples = loader.LoadCsv();
        }
    }
}
