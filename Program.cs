using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;
using BFAdmin.Models;
using BFAdmin.Helpers;
namespace BFAdmin
{
    class Program
    {
        // Server settings
        private static string serverIP = "XXXXXX";
        private static int serverPort = 47200;
        private static string serverPassword = "XXXXX";

        static void Main(string[] args)
        {
            Log.Info("RCON Socket connecting...");
            

            string consoleText = string.Empty;

            while (true)
            {
                consoleText = Console.ReadLine();

                if (consoleText == "quit")
                {
                    break;
                }
            }
        }

    }
}
