using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IFileHandling
    {
        [OperationContract]
        FileManipulationResults SendFile(FileManipulationOptions options);
        [OperationContract]
        FileManipulationResults GetFiles(FileManipulationOptions options);
    }
}
