using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class DataFormatFault
    {
        string message;

        public DataFormatFault(string message)
        {
            this.message = message;
        }

        [DataMember]
        public string Message { get => message; set => message = value; }
    }
}
