using Common;
using System.ServiceModel;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChannelFactory<ISensorContract> factory = new ChannelFactory<ISensorContract>("ServiceContract");
        }
    }
}
