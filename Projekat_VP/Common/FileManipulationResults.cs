using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public enum ResultType
    {
        [EnumMember]
        Success,
        [EnumMember]
        Warning,
        [EnumMember]
        Failed
    }
    [DataContract]
    public class FileManipulationResults : IDisposable
    {
        public FileManipulationResults()
        {
            ResultType = ResultType.Success;
            MemoryStreamCollection = new Dictionary<string, MemoryStream>();
        }
        [DataMember]
        public string ResultMessage { get; set; }
        [DataMember]
        public ResultType ResultType { get; set; }
        [DataMember]
        public Dictionary<string, MemoryStream> MemoryStreamCollection { get; set; }

        public void Dispose()
        {
            if (MemoryStreamCollection == null) return;
            foreach (KeyValuePair<string, MemoryStream> kvp in MemoryStreamCollection)
            {
                kvp.Value?.Dispose();
            }
            MemoryStreamCollection.Clear();
        }
    }
}
