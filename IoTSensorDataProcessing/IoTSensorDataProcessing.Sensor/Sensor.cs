using System;
using System.Collections.Generic;
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
        private static ServerClient _webServerClient;

        private bool _active;
        private int _activeConnections;

        private const double LongMin = 15.87;
        private const double LongMax = 16;
        private const double LatMin = 45.75;
        private const double LatMax = 45.85;
        private const int MaxPortNumber = 65535;

        private static readonly object LockObj = new object();
        private readonly Stopwatch _stopwatch;
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
        
        public int Threads { get; } = 5;

        public string Name { get; }
        public IPAddress Ip { get; }
        public int Port { get; }
        public double Longitude { get; }
        public double Latitude { get; }

        public ICollection<Measurement> Measurements { get; }
       
        public Action<string> WriteLogAction { get; set; }
        public Action<string> UpdateActiveConnAction { get; set; }

        public int ActiveConnections
        {
            get { return _activeConnections; }
            set
            {
                lock (LockObj)
                {
                    _activeConnections = value;
                }
                UpdateActiveConnAction(value.ToString());
            }
        }

        #region Public methods
        public Sensor(string filePath)
        {
            // Register sensor on the web service
            WriteLogAction = WriteLogAction ?? Console.WriteLine;
            UpdateActiveConnAction = WriteLogAction ?? Console.WriteLine;

            _rand = new Random();
            Port = SetPort();
            Name = SetName(Port);
            Longitude = SetPosition(LongMin, LongMax);
            Latitude = SetPosition(LatMin, LatMax);
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

        public void ConnectToNeighbour()
        {
            // Search for neighbour
            var neighbour = _webServerClient.SearchNeighbour(Name);
            _active = true;

            // Connect to the neighbour as client
            if (neighbour != null)
                StartTcpClient(neighbour.IpAddress, neighbour.Port);
        }
        
        public void ServerLoop()
        {
            while (true)
            {
                using (var socket = _listener.AcceptSocket())
                {
                    try
                    {
                        //socket.SetSocketOption(SocketOptionLevel.Socket,
                        //        SocketOptionName.ReceiveTimeout, 10000);
                        ActiveConnections += 1;
                        using (var s = new NetworkStream(socket))
                        {
                            var sr = new StreamReader(s);
                            var sw = new StreamWriter(s) { AutoFlush = true };

                            sw.WriteLine("");
                            while (true)
                            {
                                string request = sr.ReadLine();
                                if (request != null && request.ToLower().Equals("stop"))
                                {
                                    sw.WriteLine("OK");
                                    WriteLogAction("server < req: STOP");
                                    WriteLogAction("server > resp: OK");
                                    socket.Close();
                                    break;
                                }
                                WriteLogAction("server < req: FETCH");
                                Thread.Sleep(1000);
                                // Calculate own measurement
                                var time = (int)Math.Round((decimal)_stopwatch.ElapsedMilliseconds / 1000);
                                WriteLogAction("server : elapsed " + time + "s");

                                var index = GetMeasurement(time);
                                var line = GetCsvLineNumber(index);
                                WriteLogAction("server : line " + line);

                                var ownMeasure = Measurements.ElementAtOrDefault(index);
                                var msg = ConstructMessage(ownMeasure);
                                WriteLogAction("server > resp: " + msg);
                                
                                // Send own measurement
                                sw.WriteLine(msg);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WriteLogAction(e.Message);
                        return;
                    }
                    finally
                    {
                        ActiveConnections -= 1;
                    }
                }
            }
        }

        private int GetCsvLineNumber(int index)
        {
            return index + 2; // header and because enumeration n file starts from 1, not from 0
        }

        public void StopNeighbourCommmunication()
        {
            _active = false;
        }
        #endregion

        #region Private Methods
        private void StartTcpClient(string hostname, int port)
        {
            using (_client = new TcpClient(hostname, port))
            {
                using (var stream = _client.GetStream())
                {
                    var sr = new StreamReader(stream);
                    var sw = new StreamWriter(stream) { AutoFlush = true };
                    //WriteLogAction(
                    sr.ReadLine();
                    while (_active)
                    {
                        // Calculate own measurement
                        var time = (int)Math.Round((decimal)_stopwatch.ElapsedMilliseconds / 1000);
                        WriteLogAction("client : elapsed " + time +"s");

                        var index = GetMeasurement(time);
                        var line = GetCsvLineNumber(index);
                        WriteLogAction("client : line " + line);

                        var ownMeasure = Measurements.ElementAtOrDefault(index);
                        WriteLogAction("client : own: " + ConstructMessage(ownMeasure));

                        // Get measurement from neighbour
                        var msg = "FETCH";
                        WriteLogAction("client > req: " + msg);
                        sw.WriteLine(msg);

                        var response = sr.ReadLine();
                        WriteLogAction("client < resp: " + response);

                        var neighbourMeasure = RecreateMeasurement(response);

                        // Calculate average measurement
                        var measurement = AverageMeasurement(ownMeasure, neighbourMeasure);

                        // Send measurement to the web server
                        if (measurement.Temperature != null)
                            _webServerClient.StoreMeasurement(Name, "Temperature", measurement.Temperature.Value);
                        if (measurement.Pressure != null)
                            _webServerClient.StoreMeasurement(Name, "Pressure", measurement.Pressure.Value);
                        if (measurement.Humidity != null)
                            _webServerClient.StoreMeasurement(Name, "Humidity", measurement.Humidity.Value);
                        if (measurement.Co != null)
                            _webServerClient.StoreMeasurement(Name, "Co", measurement.Co.Value);
                        if (measurement.No2 != null)
                            _webServerClient.StoreMeasurement(Name, "No2", measurement.No2.Value);
                        if (measurement.So2 != null)
                            _webServerClient.StoreMeasurement(Name, "So2", measurement.So2.Value);

                        Thread.Sleep(5000);
                    }

                    var msgStop = "STOP";
                    WriteLogAction("client > req: " + msgStop);
                    sw.WriteLine(msgStop);
                    var responseStop = sr.ReadLine();
                    WriteLogAction("client < resp: " + responseStop);
                }
            }
        }

        private Measurement AverageMeasurement(Measurement own, Measurement neighbour)
        {
            return new Measurement()
            {
                Temperature = AverageValue(own.Temperature, neighbour.Temperature),
                Humidity = AverageValue(own.Humidity, neighbour.Humidity),
                Pressure = AverageValue(own.Pressure, neighbour.Pressure),
                Co = AverageValue(own.Co, neighbour.Co),
                No2 = AverageValue(own.No2, neighbour.No2),
                So2 = AverageValue(own.So2, neighbour.So2)
            };
        }

        private float? AverageValue(float? own, float? neighbour)
        {
            return own == null ? neighbour
                               : (neighbour == null ? own : (own + neighbour) / 2);
        }

        private void StartTcpServer(string ipAdr, int port)
        {
            _listener = new TcpListener(
                new IPEndPoint(IPAddress.Parse(ipAdr), port));
            _listener.Start();

            for (int i = 0; i < Threads; i++)
            {
                Thread t = new Thread(ServerLoop) { IsBackground = true };
                t.Start();
            }

        }

        private Measurement RecreateMeasurement(string message)
        {
            var col = message.Split(',');
            if (col.Length < 6)
            {
                return new Measurement();
            }

            float tmp;
            float? temp = float.TryParse(col[0], out tmp) ? tmp : default(float);
            float? press = float.TryParse(col[1], out tmp) ? tmp : default(int);
            float? hum = float.TryParse(col[2], out tmp) ? tmp : default(int);
            var co = float.TryParse(col[3], out tmp) ? tmp : default(float?);
            var no2 = float.TryParse(col[4], out tmp) ? tmp : default(float?);
            var so2 = float.TryParse(col[5], out tmp) ? tmp : default(float?);
            return new Measurement()
            {
                Temperature = temp,
                Pressure = press,
                Humidity = hum,
                Co = co,
                No2 = no2,
                So2 = so2
            };

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

        private int GetMeasurement(int elapsedSeconds)
        {
            var index = elapsedSeconds % 100;
            return index;
        }

        private int SetPort()
        {
            int port = 2055;
            while (true)
            {
                if (CheckPortAvaliable(port)) return port;
                port++;
                if (port > MaxPortNumber) throw new ApplicationException("Unable to find free port.");
            }
        }

        private bool CheckPortAvaliable(int port)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (var tcpi in tcpConnInfoArray)
            {
                if (tcpi.Port == port)
                {
                    return false;
                }
            }

            var tcpConnInfoArray2 = ipGlobalProperties.GetActiveTcpConnections();

            foreach (var tcpi in tcpConnInfoArray2)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    return false;
                }
            }
            return true;
        }

        private string SetName(int? appendNumber = null)
        {
            var index = Convert.ToInt32(
                Math.Floor(SetPosition(0, Names.Length - 1)));
            var part1 = Names[index];
            index = Convert.ToInt32(
                Math.Floor(SetPosition(0, Names.Length - 1)));
            var part2 = Names[index];
            return part1 + "_" + part2 + ((appendNumber == null) ? "" : "_" + appendNumber);
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

        private static IPAddress GetIpAddress()
        {
            return IPAddress.Parse("127.0.0.1");
        } 
        #endregion
    }

    public class Measurement
    {
        public float? Temperature { get; set; }
        public float? Pressure { get; set; }
        public float? Humidity { get; set; }
        public float? Co { get; set; }
        public float? No2 { get; set; }
        public float? So2 { get; set; }
    }

}
