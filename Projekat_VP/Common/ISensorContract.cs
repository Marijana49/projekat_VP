using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface ISensorContract
    {
        [OperationContract]
        void StartSession(string meta);

        [OperationContract]
        void PushSample(SensorSample sample);

        [OperationContract]
        void EndSession();
    }
}