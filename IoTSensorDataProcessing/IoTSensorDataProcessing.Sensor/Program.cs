using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoTSensorDataProcessing.Sensor.ServerReference;

namespace IoTSensorDataProcessing.Sensor
{
    class Program
    {
        static void Main(string[] args)
        {
            var location = DateTime.Now.Ticks%10;
            var name = "s" + location;
            var ip = "123.123.123." + location;
            ServerClient client = new ServerClient();
            Console.WriteLine(client.Register(name, location, location, ip, 9889));
            Console.WriteLine(client.StoreMeasurement(name,"wtf",0));
            Thread.Sleep(5000);
            Console.WriteLine(client.SearchNeighbour(name));
        }
    }
}
