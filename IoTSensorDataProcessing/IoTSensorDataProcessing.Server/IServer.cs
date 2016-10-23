using System.Runtime.Serialization;
using System.ServiceModel;

namespace IoTSensorDataProcessing
{
    [ServiceContract]
    public interface IServer
    {
        [OperationContract]
        bool Register(string username, double latitude,
            double longitude, string ipAddress, int port);

        [OperationContract]
        UserAddress SearchNeighbour(string username);

        [OperationContract]
        bool StoreMeasurement(string username, 
            string parameter, float averageValue);
    }

    [DataContract]
    public class UserAddress
    {
        [DataMember]
        public string Username { get; set; } 

        [DataMember]
        public double Longitude { get; set; }

        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public string IpAddress { get; set; }

        [DataMember]
        public int Port { get; set; }
    }

    [DataContract]
    public class Measurement
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Parameter { get; set; }

        [DataMember]
        public float AverageValue { get; set; }
    }
}
