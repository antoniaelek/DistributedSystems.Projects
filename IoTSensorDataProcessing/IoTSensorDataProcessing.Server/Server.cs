using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace IoTSensorDataProcessing
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Server : IServer
    {
        public ISet<UserAddress> Sensors { get; set; }
        public ICollection<Measurement> Measurements { get; set; }

        public Server()
        {
            Sensors = new HashSet<UserAddress>();
            Measurements = new List<Measurement>();
        }

        public bool Register(string username, double latitude,
            double longitude, string ipAddress, int port)
        {
            // if sensor is already registered
            if (Sensors.FirstOrDefault(s => s.Username == username) != null)
            {
                WriteLog("Unable to register sensor " + username 
                    + " because it has already been registered.");
                return false;
            }

            // if not, register it
            Sensors.Add(new UserAddress
            {
                Username = username,
                Latitude = latitude,
                Longitude = longitude,
                IpAddress = ipAddress,
                Port = port
            });
            WriteLog("Registered sensor " + username + ".");
            return true;
        }

        public UserAddress SearchNeighbour(string username)
        {
            var sensor1 = Sensors.FirstOrDefault(s => s.Username == username);
            if (sensor1 == null)
            {
                throw new ApplicationException("Sensor with name " + username
                                                + " is not registered;");
            }

            double? minDistance = null;
            UserAddress closestNeighbour = null;
            foreach (var sensor2 in Sensors)
            {
                if (sensor1 == sensor2) continue;
                var currDistance = Distance(sensor1, sensor2);
                if (minDistance == null || currDistance < minDistance.Value)
                {
                    minDistance = currDistance;
                    closestNeighbour = sensor2;
                }
            }

            if (closestNeighbour == null)
            {
                var errorMessage = "Unable to find closest neighbour"
                                   + " to sensor " + sensor1.Username + ".";
                WriteLog(errorMessage);
                return null;
            }

            var message = "Distance between sensor "
                        + sensor1.Username + " and its closest neighbour "
                        + closestNeighbour.Username
                        + " equals " + minDistance + ".";
            WriteLog(message);
            return closestNeighbour;
        }

        public bool StoreMeasurement(string username, string parameter,
            float averageValue)
        {
            try
            {
                Measurements.Add(new Measurement
                {
                    Username = username,
                    Parameter = parameter,
                    AverageValue = averageValue
                });
                var message = "Saved measurement (server: " + username
                            + ",\tparameter: " + parameter
                            + ",\taverage: " + averageValue + ").";
                WriteLog(message);
            }
            catch (NotSupportedException)
            {
                var message = "Unable to save measurement (server : " + username
                            + ",\tparameter: " + parameter
                            + ",\ttaverage: " + averageValue + ").";
                WriteLog(message);
                return false;
            }
            return true;
        }

        #region Helpers

        private static void WriteLog(string message)
        {
            Console.WriteLine(message);
        }

        private static double Distance(UserAddress sensor1, UserAddress sensor2)
        {
            const int r = 6371;
            var dlon = sensor2.Longitude - sensor1.Longitude;
            var dlat = sensor2.Latitude - sensor1.Latitude;
            var a = Math.Pow(Math.Sin(dlat / 2), 2)
                    + Math.Cos(sensor1.Latitude)
                    * Math.Cos(sensor2.Latitude)
                    * Math.Pow(Math.Sin(dlon / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = r * c;
            return d;
        }
        #endregion
    }
}
