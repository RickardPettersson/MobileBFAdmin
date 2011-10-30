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
        private static string serverIP = "178.236.68.211";
        private static int serverPort = 47200;
        private static string serverPassword = "jolt";

        // Socket connection object
        private static Socket rconSocket;

        // Thread for reading data from socket
        private static Thread readThread;

        // Boolean to see if we can write or not to the socket
        private static Boolean canWrite = false;
        
        static void Main(string[] args)
        {
            Log.Write("RCON Socket connecting...");
            try
            {
                // Try convert server ip string to a IPAdress object
                IPAddress x;
                IPAddress.TryParse(serverIP, out x);

                // Setup the socket connection
                rconSocket = new Socket(x.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                SocketOptionLevel socketLevel = SocketOptionLevel.Tcp;

                if (Environment.OSVersion.Version.Major > 5)
                {
                    socketLevel = SocketOptionLevel.Socket;
                }

                rconSocket.SetSocketOption(socketLevel, SocketOptionName.KeepAlive, 1);
                Byte[] optionInValue = new Byte[] { 1, 0, 0, 0, 224, 147, 4, 0, 1, 0, 0, 0 };
                rconSocket.IOControl(IOControlCode.KeepAliveValues, optionInValue, null);

                // Connect
                rconSocket.Connect(serverIP, serverPort);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Log.Write("Connect error: Port Fail - Exception: " + ex.ToString());
                CloseSocketConnection();
                Console.ReadLine();
                return;
            }
            catch (ObjectDisposedException ex)
            {
                Log.Write("Connect error: Socket Closed - Exception: " + ex.ToString());
                CloseSocketConnection();
                Console.ReadLine();
                return;
            }
            catch (Exception ex)
            {
                Log.Write("Connect error - Exception: " + ex.ToString());
                CloseSocketConnection();
                Console.ReadLine();
                return;
            }

            if (!rconSocket.Connected)
            {
                Log.Write("Connect failed");
                Console.ReadLine();
                return;
            }

            SendToSocket("login.hashed");
            readThread = new Thread(new ThreadStart(ReadSocket));
            readThread.IsBackground = true;
            readThread.Start();

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

        private static void ReadSocket()
        {
            Log.Write("Started read socket");
            while (true)
            {
                if (!rconSocket.Connected)
                {
                    Disconnect();
                }

                packetData packetParts;
                try
                {
                    packetParts = SocketTools.DecodePacket(receivePacket());
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Log.Write("ReadSocket > DecodePacket - buffer too small - Exception: " + ex.ToString());
                    Disconnect();
                    break;
                }
                catch (ObjectDisposedException ex)
                {
                    Log.Write("ReadSocket > DecodePacket - Socket Closed - Exception: " + ex.ToString());
                    Disconnect();
                    break;
                }
                catch (Exception ex)
                {
                    Log.Write("ReadSocket > DecodePacket - Exception: " + ex.ToString());
                    Disconnect();
                    break;
                }

                if (!canWrite)
                {
                    if (packetParts.sequence == 0)
                    {
                        if ((packetParts.Words.Count > 0) && (packetParts.Words[0] != "OK"))
                        {
                            Disconnect();
                            break;
                        }

                        Log.Write("Connected");

                        string[] sendStrings = { "login.hashed", SocketTools.generatePasswordHash(packetParts.Words[1], serverPassword) };
                        SendToSocket(sendStrings);
                    }
                    else if (packetParts.sequence == 1)
                    {
                        if ((packetParts.Words.Count > 0) && (packetParts.Words[0] != "OK"))
                        {
                            Log.Write("Auth Failed");
                            Disconnect();
                            break;
                        }

                        string[] sendStrings = { "admin.eventsEnabled", "true" };
                        SendToSocket(sendStrings);

                        canWrite = true;
                        Log.Write("Authenticated");
                    }
                    else if (packetParts.sequence == 1)
                    {
                        if ((packetParts.Words.Count > 0) && (packetParts.Words[0] != "OK"))
                        {
                            Disconnect();
                            break;
                        }
                    }
                }

                List<string> w = packetParts.Words;

                ServerEventHandler.HandleEvents(w);
            }
        }
        
        public static void SendToSocket(string word)
        {
            String[] stringArray = new String[] { word };

            SendToSocket(stringArray);
        }

        public static void SendToSocket(string[] words)
        {
            Log.Write("Sent command to socket: " + string.Join(" ", words));
            rconSocket.Send(SocketTools.EncodeClientRequest(words));
        }

        private static Byte[] receivePacket()
        {
            List<Byte> receiveBuffer = new List<byte>();

            while (!SocketTools.containsCompletePacket(receiveBuffer))
            {
                Byte[] tempBuffer = new Byte[4096];
                int numBytes = rconSocket.Receive(tempBuffer, 4096, SocketFlags.None);

                for (int i = 0; i < numBytes; i++)
                {
                    receiveBuffer.Add(tempBuffer[i]);
                }
            }

            Byte[] byteArray = new Byte[] { receiveBuffer[4], receiveBuffer[5], receiveBuffer[6], receiveBuffer[7] };

            int packetSize = (int)SocketTools.DecodeInt32(byteArray);

            Byte[] packet = new Byte[packetSize];

            for (int i = 0; i < packetSize - 1; i++)
            {
                packet[i] = receiveBuffer[i];
            }

            receiveBuffer.RemoveRange(0, packetSize);

            return packet;
        }

        private static void Disconnect()
        {
            Log.Write("Disconnected (ReadSocket)");
            CloseSocketConnection();
            readThread.Abort();
        }

        private static void CloseSocketConnection()
        {
            rconSocket.Close();
            rconSocket = null;
        }

       
    }
}
