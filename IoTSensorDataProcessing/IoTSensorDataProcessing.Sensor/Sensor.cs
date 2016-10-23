using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using IoTSensorDataProcessing.Sensor.ServerReference;
using Microsoft.VisualBasic.FileIO;

namespace IoTSensorDataProcessing.Sensor
{
    public class Sensor
    {
        private TcpListener _listener;
        private TcpClient _client;

        private const int Threads = 5;
        private const int MaxClients = 10;
        private int _activeConnections = 0;
        private static int _instancesCounter = 0;

        public string Name { get; }
        public IPAddress Ip { get; }
        public int Port { get; }
        public double Longitude { get; }
        public double Latitude { get; }

        public ICollection<Measurement> Measurements { get; }

        private Stopwatch _stopwatch;
        private readonly Random _rand;
        #region private static readonly string[] Names 

        private static readonly string[] Names =
        {
            "acrobat", "andy", "backseat", "bagpipe", "bamboo", "bandbox",
            "baseball", "beehive", "bigtop", "birdseye", "blackboard", "blacktop",
            "blowtorch", "blueprint", "boardwalk", "bookstore", "brimstone",
            "broadside", "buckeye", "buckshot", "bulldog", "bungalow",
            "cableboy", "cactus", "candlestick", "carbine", "carnation", "carpet",
            "cartwheel", "challenger", "chandelier", "checkerboard", "checkmate",
            "cloudburst", "cloverleaf", "cobweb", "companion", "crossbow", "crown",
            "curbside", "driftwood", "elm", "fireside", "handshake", "headlight",
            "horsepower", "hudson", "lightfoot", "lizard", "magic", "pacemaker",
            "pavillion", "peninsula", "pincushion", "playground", "professor",
            "ridgeline", "ringside", "roadhouse", "sandstone", "shotgun",
            "skymaster", "tower", "volcano", "warehouse", "windstone", "angel",
            "caliber", "caravan", "cargo", "carousel", "chariot", "cowpuncher",
            "electric", "falcon", "fullback", "halfback", "hedgehog", "holly",
            "horsehide", "huntsman", "kneecap", "nighthawk", "patroller", "pivot",
            "roadrunner", "saturn", "signature", "softpack", "stagecoach", "tracer",
            "tracker", "apollo", "backhoe", "ballfield", "barefoot", "buscuit",
            "bullpen", "bunker", "carbon", "castle", "champion", "citadel",
            "corkscrew", "fable", "fiddler", "fraction", "gimlet", "gladiola",
            "goffer", "hobnail", "kiley", "mustang", "pushbutton", "register",
            "rosebush", "sandbox", "sawhorse", "shadow", "smelter", "spectator",
            "stutter", "sugarfoot", "tailor", "templer", "tinkerbell", "traffic",
            "transit", "walnut"
        };

        #endregion

        private static ServerClient _webServerClient;

        public Action<string> WriteLogAction { get; set; } = s => Console.WriteLine(s);

        public Sensor(string filePath, double longMin = 15.87, 
            double longMax = 16, double latMin = 45.75, double latMax = 45.85)
        {
            // Register sensor on the web service
            _rand = new Random();
            Name = SetName();
            Port = SetPort();
            Longitude = SetPosition(longMin, longMax);
            Latitude = SetPosition(latMin, latMax);
            Measurements = ParseCsvFile(filePath);
            Ip = GetIpAddress();

            // Stopwatch
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            if (_webServerClient == null) _webServerClient = new ServerClient();
            _webServerClient.Register(Name, Latitude, Longitude, Ip.ToString(), Port);
            WriteLogAction("Sensor registered.");

            // Start tcp server
            StartTcpServer(Ip.ToString(), Port);
            WriteLogAction("Started tcp server on port " + Port);
        }

        public void CommunicateWithNeighbour()
        {
            // Search for neighbour
            var neighbour = _webServerClient.SearchNeighbour(Name);

            // Connect to the neighbour as client
            if (neighbour != null)
                StartTcpClient(neighbour.IpAddress, neighbour.Port);
        }

        private void StartTcpClient(string hostname, int port)
        {
            using (_client = new TcpClient(hostname, port))
            {
                using (var stream = _client.GetStream())
                {
                    var sr = new StreamReader(stream);
                    var sw = new StreamWriter(stream) {AutoFlush = true};
                    WriteLogAction(sr.ReadLine());
                    while (true)
                    {
                        var msg = "Send me measurements";
                        sw.WriteLine(msg);
                        WriteLogAction("Client sent: " + msg);
                        // Get measurement from neighbour
                        var response = sr.ReadLine();
                        // Calculate own measurement
                        WriteLogAction("Client received: " + response);
                        // Send measurement to the web server
                    }
                }
            }
        }

        private void StartTcpServer(string ipAdr, int port)
        {
            _listener = new TcpListener(
                new IPEndPoint(IPAddress.Parse(ipAdr),port));
            _listener.Start();

            for (int i = 0; i < Threads; i++)
            {
                Thread t = new Thread(new ThreadStart(Loop));
                t.Start();
            }

        }

        public void Loop()
        {
            while (true)
            {
                using (var socket = _listener.AcceptSocket())
                {
                    socket.SetSocketOption(SocketOptionLevel.Socket,
                        SocketOptionName.ReceiveTimeout, 10000);
                    try
                    {
                        using (var s = new NetworkStream(socket))
                        {
                            var sr = new StreamReader(s);
                            var sw = new StreamWriter(s) {AutoFlush = true};

                            sw.WriteLine("");
                            while (true)
                            {
                                string request = sr.ReadLine();
                                WriteLogAction("Server received: " + request);
                                //if (string.IsNullOrEmpty(name)) break;
                                var msg = ConstructMessage(GetMeasurement());
                                WriteLogAction("Server sent: " + msg);
                                sw.WriteLine(msg);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WriteLogAction(e.Message);
                        break;
                    }
                }
            }
        }

        private string ConstructMessage(Measurement measurement)
        {
            return measurement.Temperature + ","
                   + measurement.Pressure + ","
                   + measurement.Humidity + ","
                   + (measurement.Co == default(int?) 
                      ? "" : measurement.Co.ToString()) 
                   + ","
                   + (measurement.No2 == default(int?) 
                      ? "" : measurement.No2.ToString()) 
                   + ","
                   + (measurement.So2 == default(int?) 
                      ? "" : measurement.So2.ToString());
        }

        private Measurement GetMeasurement()
        {
            var time = (int)
                Math.Round((decimal) _stopwatch.ElapsedMilliseconds/1000);
            var index = time % 100 + 2;
            return Measurements.ElementAtOrDefault(index);
        }

        private int SetPort()
        {
            int port = 2055;
            WriteLogAction(Port.ToString());
            while (true)
            {
                if (CheckPortAvaliable(port)) return port;
                port++;
            }
            //return port;
            // TO-DO max port number?
        }

        private bool CheckPortAvaliable(int port)
        {
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (var tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }
            return isAvailable;
        }

        private string SetName()
        {
            var index = Convert.ToInt32(
                Math.Floor(SetPosition(0, Names.Length - 1)));
            var part1 = Names[index];
            index = Convert.ToInt32(
                Math.Floor(SetPosition(0, Names.Length - 1)));
            var part2 = Names[index];
            return part1 + "_" + part2;
        }

        private double SetPosition(double min, double max)
        {
            var range = max - min;
            return _rand.NextDouble() * range + min;
        }

        private ICollection<Measurement> ParseCsvFile(string path)
        {
            var measurements = new List<Measurement>();
            using (var parser = new TextFieldParser(path))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                var rowCount = -1;
                while (!parser.EndOfData)
                {
                    rowCount += 1;

                    var fields = parser.ReadFields();
                    if (fields == null || fields.Length < 6 || rowCount == 0)
                        continue;

                    int temperature; int preassure = 0; int humidity = 0;
                    var error = !int.TryParse(fields[0], out temperature);
                    error = error || !int.TryParse(fields[1], out preassure);
                    error = error || !int.TryParse(fields[2], out humidity);
                    if (error) continue;

                    int tmp;
                    var co = int.TryParse(fields[3], out tmp) ? tmp : default(int?);
                    var no2 = int.TryParse(fields[4], out tmp) ? tmp : default(int?);
                    var so2 = int.TryParse(fields[5], out tmp) ? tmp : default(int?);

                    measurements.Add(new Measurement()
                    {
                        Temperature = temperature,
                        Pressure = preassure,
                        Humidity = humidity,
                        Co = co,
                        No2 = no2,
                        So2 = so2
                    });
                }
            }
            return measurements;
        }

        private static IPAddress GetIpAddress(params string[] names)
        {
            //string name = (names.Length < 1) ? Dns.GetHostName() : names[0];
            //try
            //{
            //    var hostEntry = Dns.GetHostEntry(name);
            //    IPAddress[] addrs = hostEntry.AddressList;
            //    //foreach (IPAddress addr in addrs)
            //    //    Console.WriteLine("{0}/{1}", name, addr);
            //    return addrs[0];
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //    return null;
            //}
            return IPAddress.Parse("127.0.0.1");
        }
    }

    public class Measurement
    {
        public int Temperature { get; set; }
        public int Pressure { get; set; }
        public int Humidity { get; set; }
        public int? Co { get; set; }
        public int? No2 { get; set; }
        public int? So2 { get; set; }
    }

}
