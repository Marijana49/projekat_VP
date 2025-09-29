using System.Runtime.Serialization;
using System.ServiceModel.Channels;

namespace Common
{
    [DataContract]
    public class ValidationFault
    {
        [DataMember]
        public string Message { get; set; }

        public ValidationFault(string message)
        {
            Message = message;
        }
    }
}
