using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class ValidationFault
    {
        string message;

        public ValidationFault(string message)
        {
            this.message = message;
        }

        [DataMember]
        public string Message { get => message; set => message = value; }
    }
}
