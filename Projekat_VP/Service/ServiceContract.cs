using Common;
using System;
using System.ServiceModel;

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

        public void Validation(SensorSample ss)
        {
            if(ss.volume <= 90)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! Volume is over 90!"));
            }
            if(ss.no2 <= 16000)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! NO2 concetration is over 16000!"));
            }
            if (ss.co <= 280000)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! CO concetration is over 280000!"));
            }
            if (ss.pressure <= 1028)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Warning! Pressure is over 1028!"));
            }
            //in case of textbox add DataFormat warning
        }
    }
}
