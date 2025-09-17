using Common;
using System;

namespace Service
{
    public class ServiceContract : ISensorContract
    {
        public void EndSession()
        {
            throw new NotImplementedException();
        }

        public void PushSample(SensorSample sample)
        {
            throw new NotImplementedException();
        }

        public void StartSession(string meta)
        {
            throw new NotImplementedException();
        }
    }
}
