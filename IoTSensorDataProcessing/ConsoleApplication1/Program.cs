using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var s = Console.ReadLine();
                int port;
                if (!int.TryParse(s,out port))
                {
                    Console.WriteLine("Invalid port number");
                    continue;
                }
                Console.WriteLine("Port " + port + " is " + (CheckPortAvaliable(port) == false ? "not" : "") + " avaliable.");
            }
        }

        public static bool CheckPortAvaliable(int port)
        {
            var isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

            foreach (var tcpi in tcpConnInfoArray)
            {
                if (tcpi.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }
            return isAvailable;
        }
    }
}
