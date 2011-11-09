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
        private static string webserviceHost = string.Empty;
        private static int webservicePort = 0;

        // Create the rcon client object
        private static RconClient rconClient;

        // Create a timer object
        private static System.Timers.Timer aTimer;

        // Create a player list object
        public static List<Player> Playerlist = new List<Player>();

        // Create a player command queue
        public static Queue<Dictionary<string, string>> PlayerCommandsQueue = new Queue<Dictionary<string, string>>();

        static void Main(string[] args)
        {
            // Check if some settings not set in the app.config
            if (string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["RCONServerIP"]) || string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["RCONServerPort"]) || string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["RCONServerPassword"]) || string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["WebservicePort"]) || string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["WebserviceAdminUsername"]) || string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["WebserviceAdminPassword"]) || string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["WebserviceHost"]))
            {
                Console.WriteLine("Some settings in app.config is not set");
                Console.WriteLine(">> Press any key to quit <<");
                Console.ReadLine();
            }
            else
            {
                // Get server settings from app.config
                serverIP = System.Configuration.ConfigurationManager.AppSettings["RCONServerIP"];
                serverPort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["RCONServerPort"]);
                serverPassword = System.Configuration.ConfigurationManager.AppSettings["RCONServerPassword"];
                webserviceHost = System.Configuration.ConfigurationManager.AppSettings["WebserviceHost"];
                webservicePort = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["WebservicePort"]);

                // Setup the rcon client with events etc and connect
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

                // Setup a timer for every 5 seconds to run the listplayers function that do stuff with the players, like commands etc.
                aTimer = new System.Timers.Timer(5000);
                aTimer.Elapsed += new ElapsedEventHandler(ListPlayers);
                aTimer.Enabled = true;

                // Start the webservice/HTTP Listener to listen on webservice port
                Webservice.Start(webserviceHost, webservicePort);

                // Add some handling of ctrl+c so we stoping the webservice right
                Console.TreatControlCAsInput = false;  // Turn off the default system behavior when CTRL+C is pressed.
                Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs eventArgs)
                {
                    Webservice.Stop();
                };

                Console.ReadLine();
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
            if (e.Player == null)
            {
                Log.Info("PlayerLeft - Player: n/a");
            }
            else
            {
                Log.Info("PlayerLeft - Player: " + e.Player.Name);
            }
        }

        static void rconClient_PlayerKilled(object sender, PlayerKilledEventArgs e)
        {
            if (e.Attacker == null)
            {
                Log.Info("PlayerKilled - Attacker: N/A - Victim: " + e.Victim.Name + " - Weapon: " + e.Weapon + " - Headshot: " + e.Headshot.ToString());
            }
            else
            {
                Log.Info("PlayerKilled - Attacker: " + e.Attacker.Name + " - Victim: " + e.Victim.Name + " - Weapon: " + e.Weapon + " - Headshot: " + e.Headshot.ToString());
            }
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
            // Disable the timer that runs the playerlist
            aTimer.Enabled = false;
            aTimer.Dispose();

            // Stop the webservice
            Webservice.Stop();

            Log.Info("Disconnected - Message: " + e.Message);
        }

        static void rconClient_ConnectError(object sender, ConnectErrorEventArgs e)
        {
            Log.Info("ConnectError - Message: " + e.Message);
        }

        static void rconClient_Connected(object sender, EventArgs e)
        {
            // Send login command to the RCON server
            rconClient.LogOn(serverPassword, true);

            Log.Info("Connected");
        }

        private static void ListPlayers(object source, ElapsedEventArgs e)
        {
            // Get the player collection from rcon library
            PlayerCollection players = rconClient.Players;

            // Convert pplayer collection to playerlist
            Playerlist = players.ToList();

            // Check if we got any players
            if (players.Count == 0)
            {
                Log.Info("No players on the server");

                // If there is no players on the server then only check for players every 5 minutes
                aTimer.Interval = 5 * 60 * 1000;
            }
            else
            {
                // If there is players on the server then heck for players every 5 seconds
                aTimer.Interval = 5000;

                Log.Info(players.Count + " players on the server");

                // Check if we got any player command in the queue
                if (PlayerCommandsQueue.Count > 0)
                {
                    do
                    {
                        // Get the oldest object in the queue and remove it from the queue
                        Dictionary<string, string> command = PlayerCommandsQueue.Dequeue();

                        // Create a player object
                        Player player;

                        try
                        {
                            // Get the player object from rcon library
                            player = rconClient.Players.First(p => p.Name == command.Keys.First());

                            // Check what command and do it
                            switch (command[command.Keys.First()].ToLower())
                            {
                                case "kick":
                                    player.Kick();
                                    break;
                                case "permanentban":
                                    player.PermanentBan(BanTargetType.Name);
                                    break;
                                case "temporaryban":
                                    int seconds = 60;
                                    if (command.Keys.Contains("seconds"))
                                    {
                                        seconds = Convert.ToInt32(command["seconds"]);
                                    }
                                    player.TemporaryBan(BanTargetType.Name, seconds);
                                    break;
                                case "roundban":
                                    player.RoundBan(BanTargetType.Name);
                                    break;
                                case "kill":
                                    player.Kill();
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Info("Exception when try to do commando " + command[command.Keys.First()] + " on player " + command.Keys.First() + " - Exception: " + ex.ToString());
                        }
                    } while (PlayerCommandsQueue.Count != 0);
                }
            }
        }
    }
}
