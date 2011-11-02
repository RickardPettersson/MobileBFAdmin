using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;
using BFAdmin.Helpers;
using System.Net.Battlefield3;
using System.Timers;
namespace BFAdmin
{
    class Program
    {
        // Server settings
        private static string serverIP = string.Empty;
        private static int serverPort = 0;
        private static string serverPassword = string.Empty;

        private static RconClient rconClient;
        private static System.Timers.Timer aTimer;

        public static List<Player> Playerlist = new List<Player>();

        static void Main(string[] args)
        {
            if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["RCONServerIP"]) || string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["RCONServerPort"]) || string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["RCONServerPassword"]))
            {
                Console.WriteLine("Some settings in app.config is not set");
                Console.WriteLine(">> Press any key to quit <<");
                Console.ReadLine();
            }
            else
            {
                serverIP = System.Configuration.ConfigurationManager.AppSettings["RCONServerIP"];
                serverPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["RCONServerPort"]);
                serverPassword = System.Configuration.ConfigurationManager.AppSettings["RCONServerPassword"];

                rconClient = new RconClient();
                rconClient.Address = serverIP;
                rconClient.Port = serverPort;
                rconClient.Connected += new EventHandler(rconClient_Connected);
                rconClient.ConnectError += new EventHandler<ConnectErrorEventArgs>(rconClient_ConnectError);
                rconClient.Disconnected += new EventHandler<DisconnectedEventArgs>(rconClient_Disconnected);
                rconClient.LevelLoaded += new EventHandler<LevelLoadedEventArgs>(rconClient_LevelLoaded);
                rconClient.LoggedOn += new EventHandler(rconClient_LoggedOn);
                rconClient.PlayerAuthenticated += new EventHandler<PlayerAuthenticatedEventArgs>(rconClient_PlayerAuthenticated);
                rconClient.PlayerChat += new EventHandler<PlayerChatEventArgs>(rconClient_PlayerChat);
                rconClient.PlayerJoined += new EventHandler<PlayerEventArgs>(rconClient_PlayerJoined);
                rconClient.PlayerJoining += new EventHandler<PlayerJoiningEventArgs>(rconClient_PlayerJoining);
                rconClient.PlayerKilled += new EventHandler<PlayerKilledEventArgs>(rconClient_PlayerKilled);
                rconClient.PlayerLeft += new EventHandler<PlayerEventArgs>(rconClient_PlayerLeft);
                rconClient.PlayerMoved += new EventHandler<PlayerMovedEventArgs>(rconClient_PlayerMoved);
                rconClient.PlayerSpawned += new EventHandler<PlayerEventArgs>(rconClient_PlayerSpawned);
                rconClient.PunkBusterMessage += new EventHandler<PunkBusterMessageEventArgs>(rconClient_PunkBusterMessage);
                rconClient.RawRead += new EventHandler<RawReadEventArgs>(rconClient_RawRead);
                rconClient.Response += new EventHandler<ResponseEventArgs>(rconClient_Response);
                rconClient.RoundOver += new EventHandler(rconClient_RoundOver);
                rconClient.Connect();

                aTimer = new System.Timers.Timer(2000);
                aTimer.Elapsed += new ElapsedEventHandler(ListPlayers);
                aTimer.Enabled = true;

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

        static void rconClient_RoundOver(object sender, EventArgs e)
        {
            Log.Info("RoundOver");
        }

        static void rconClient_Response(object sender, ResponseEventArgs e)
        {
            Log.Info("Response - ClientCommand: " + e.ClientCommand + " - Sequence: " + e.Sequence + " - " + string.Join(" :: ", e.Words.ToArray()));
        }

        static void rconClient_RawRead(object sender, RawReadEventArgs e)
        {
            //Log.Info("RawRead - IsFromServer: " + e.Packet.IsFromServer + " - IsResponse: " + e.Packet.IsResponse + " - Sequence: " + e.Packet.Sequence + " - Words: " + string.Join(" :: ", e.Packet.Words.ToArray()));
        }

        static void rconClient_PunkBusterMessage(object sender, PunkBusterMessageEventArgs e)
        {
            Log.Info("PunkBusterMessage - " + e.Message);
        }

        static void rconClient_PlayerSpawned(object sender, PlayerEventArgs e)
        {
            Log.Info("PlayerSpawned - Player: " + e.Player);
        }

        static void rconClient_PlayerMoved(object sender, PlayerMovedEventArgs e)
        {
            Log.Info("PlayerMoved - Player: " + e.Player.Name + " - New Team: " + e.NewTeam + " - New Squad: " + e.NewSquad + " - Old Team: " + e.OldTeam + " - Old Squad: " + e.OldSquad);
        }

        static void rconClient_PlayerLeft(object sender, PlayerEventArgs e)
        {
            Log.Info("PlayerLeft - Player: " + e.Player.Name);
        }

        static void rconClient_PlayerKilled(object sender, PlayerKilledEventArgs e)
        {
            Log.Info("PlayerKilled - Attacker: " + e.Attacker.Name + " - Victim: " + e.Victim.Name + " - Weapon: " + e.Weapon + " - Headshot: " + e.Headshot.ToString());
        }

        static void rconClient_PlayerJoined(object sender, PlayerEventArgs e)
        {
            Log.Info("PlayerJoined - Player: " + e.Player.Name);
        }

        static void rconClient_PlayerJoining(object sender, PlayerJoiningEventArgs e)
        {
            Log.Info("PlayerJoining - Player: " + e.Name + " - GUID: " + e.Guid);
        }

        static void rconClient_PlayerChat(object sender, PlayerChatEventArgs e)
        {
            Log.Info("PlayerChat - Source; " + e.Source + " - Player: " + e.Player.Name + " - Message: " + e.Message);
        }

        static void rconClient_PlayerAuthenticated(object sender, PlayerAuthenticatedEventArgs e)
        {
            if (e.Player != null)
            {
                Log.Info("PlayerAuthenticated - Player: " + e.Player.Name);
            }
            else
            {
                Log.Info("PlayerAuthenticated - Player: N/A");
            }
        }

        static void rconClient_LoggedOn(object sender, EventArgs e)
        {
            Log.Info("RCON Client Logged On");
        }

        static void rconClient_LevelLoaded(object sender, LevelLoadedEventArgs e)
        {
            Log.Info("LevelLoaded - Level: " + e.Level + " - GameMode: " + e.GameMode + " - Round (" + e.RoundsPlayed + "/" + e.RoundsTotal + ")");
        }

        static void rconClient_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Log.Info("Disconnected - Message: " + e.Message);
        }

        static void rconClient_ConnectError(object sender, ConnectErrorEventArgs e)
        {
            Log.Info("ConnectError - Message: " + e.Message);
        }

        static void rconClient_Connected(object sender, EventArgs e)
        {
            rconClient.LogOn(serverPassword, true);
            Log.Info("Connected");
        }

        private static void ListPlayers(object source, ElapsedEventArgs e)
        {
            PlayerCollection players = rconClient.Players;
            Playerlist = players.ToList();


            if (players.Count == 0)
            {
                Log.Info("No players on the server");
            }
            else
            {
                Log.Info("Playerlist - Start");

                foreach (Player player in players)
                {
                    Console.WriteLine(player.Name + " - TeamId: " + player.TeamId + " - SquadId: " + player.SquadId + " - Score: " + player.Score + " - Kills: " + player.Kills + " - Deaths: " + player.Deaths);
                }

                Log.Info("Playerlist - End");
            }
        }
    }
}
