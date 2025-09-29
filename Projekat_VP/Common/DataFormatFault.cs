using System.Runtime.Serialization;
using System.ServiceModel.Channels;

namespace Common
{
    [DataContract]
    public class DataFormatFault
    {
        [DataMember]
        public string Message { get; set; }

        public DataFormatFault(string message)
        {
            Message = message;
        }
    }
}
