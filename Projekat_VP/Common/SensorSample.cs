using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class SensorSample
    {
        public double volume;
        public double co;
        public double no2;
        public double pressure;
        public DateTime dateTime;

        public SensorSample() : this(0, 0, 0, 0, DateTime.Now) { }
        public SensorSample(double volume, double co, double no2, double pressure, DateTime dateTime)
        {
            this.volume = volume;
            this.co = co;
            this.no2 = no2;
            this.pressure = pressure;
            this.dateTime = dateTime;
        }

        [DataMember]
        public double Volume { get => volume; set => volume = value; }
        [DataMember]
        public double CO { get => co; set => co = value; }
        [DataMember]
        public double NO2 { get => no2; set => no2 = value; }
        [DataMember]
        public double Pressure { get => pressure; set => pressure = value; }
        [DataMember]
        public DateTime DateTime { get => dateTime; set => dateTime = value; }
    }
}