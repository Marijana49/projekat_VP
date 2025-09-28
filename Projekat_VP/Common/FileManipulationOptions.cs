using System;
using System.IO;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class FileManipulationOptions : IDisposable
    {
        public FileManipulationOptions() { }

        public FileManipulationOptions(MemoryStream memoryStream, string keyWord)
        {
            MemoryStream = memoryStream;
            KeyWord = keyWord;
        }

        public FileManipulationOptions(string keyWord)
        {
            KeyWord = keyWord;
            MemoryStream = null;
        }

        [DataMember]
        public MemoryStream MemoryStream { get; set; }
        [DataMember]
        public string KeyWord { get; set; }

        public void Dispose()
        {
            if (MemoryStream == null) return;
            MemoryStream.Dispose();
            MemoryStream = null;
        }
    }
}
