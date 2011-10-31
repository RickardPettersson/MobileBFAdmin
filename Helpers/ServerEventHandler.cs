using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BFAdmin.Models;

namespace BFAdmin.Helpers
{
    public class ServerEventHandler
    {
        public static void HandleEvents(packetData packetParts)
        {
            List<string> w = packetParts.Words;
            if (packetParts.isResponse)
            {
                if (Program.sequenceTracker.Keys.Contains(packetParts.sequence))
                {
                    Log.Info("Response to command \"" + Program.sequenceTracker[packetParts.sequence] + "\":");
                    Log.Info(" - " + string.Join(" :: ", w.ToArray()));
                }
                else
                {
                    Log.Info("Response to command with sequence " + packetParts.sequence + ":");
                    if (w.Count > 0)
                    {
                        Log.Info(" - " + string.Join(" :: ", w.ToArray()));
                    }
                    else
                    {
                        Log.Info(" - No words");
                    }
                }
            }
            else
            {
                switch (w[0])
                {
                    case "player.onJoin":
                        Log.Info("Soldier \"" + w[1] + "\" has joined the server");
                        break;
                    case "player.onAuthenticated":
                        if (w.Count > 2)
                        {
                            Log.Info("Soldier \"" + w[1] + "\" has been authenticated. GUID=\"" + w[2] + "\"");
                        }
                        else
                        {
                            Log.Info("Soldier \"" + w[1] + "\" has been authenticated.");
                        }
                        break;
                    case "player.onLeave":
                        Log.Info("Soldier \"" + w[1] + "\" leaved the sever - " + string.Join(" - ", w.ToArray()));
                        break;
                    case "player.onKill":
                        string headshot = string.Empty;

                        if (Convert.ToBoolean(w[4]))
                        {
                            headshot = " (headshot)";
                        }

                        Log.Info("Soldier \"" + w[1] + "\" killed \"" + w[2] + "\" with weapon \"" + w[3] + "\"" + headshot);
                        break;
                    case "player.onChat":
                        List<string> subset = new List<string>();
                        for (int i = 3; i < w.Count; i++)
                        {
                            if (w[i].Length == 0)
                            {
                                subset.Add("N/A");
                            }
                            else
                            {
                                subset.Add(w[i]);
                            }
                        }
                        onChat(w[1], w[2], subset);
                        break;
                    case "player.onKicked":
                        Log.Info("Soldier \"" + w[1] + "\" was kicked from server. Reason: " + w[2]);
                        break;
                    case "player.onSquadChange":
                        Log.Info("Soldier \"" + w[1] + "\" changed Squad to " + w[3] + " on team " + w[2]);
                        break;
                    case "player.onTeamChange":
                        Log.Info("Soldier \"" + w[1] + "\" changed Team to " + w[2] + " and Squad " + w[3]);
                        break;
                    case "punkBuster.onMessage":
                        Log.Info("PunBuster message: " + string.Join(" :: ", w.ToArray()));
                        break;
                    case "server.onLoadingLevel":
                        Log.Info("Loading level " + w[1] + " (" + w[2] + "/" + w[3] + ")");
                        break;
                    case "server.onLevelStarted":
                        Log.Info("Level started");
                        break;
                    case "server.onRoundOver":
                        Log.Info("Round over, team " + w[1] + " won!");
                        break;
                    case "server.onRoundOverPlayers":
                        // TODO: log player stats when round is over
                        Log.Info("server.onRoundOverPlayers");
                        break;
                    case "server.onRoundOverTeamScores":
                        // TODO: log team stats when round is over
                        Log.Info("server.onRoundOverTeamScores");
                        break;
                    case "player.onSpawn":
                         Log.Info("Soldier \"" + w[1] + "\" spawned. Kit: " + w[2]);
                         if (w.Count > 3)
                         {
                             Log.Info("- Weapons: " + w[2] + ", " + w[3] + ", " + w[4]);
                             Log.Info("- Gadgets: " + w[5] + ", " + w[6] + ", " + w[7]);
                         }
                        break;
                    default:
                        if (packetParts.isFromServer)
                        {
                            Log.Info("From Server (Sequence=" + packetParts.sequence + "):");
                            Log.Info(" - " + string.Join(" :: ", w.ToArray()));
                        }
                        else
                        {
                            Log.Info("FromServer=" + packetParts.isFromServer.ToString() + " | IsResponse=" + packetParts.isResponse.ToString() + " | Sequence=" + packetParts.sequence + " | Words=" + string.Join(" :: ", w.ToArray()));
                        }
                        break;
                }

                //Program.SendToSocket("OK", packetParts.sequence);
            }
        }

        public static void onChat(string soldier, string message, List<string> subset)
        {
            if (subset.Count == 0)
            {
                Log.Info("Chat: " + soldier + " - " + message);
            }
            else
            {
                switch (subset[0])
                {
                    case "all":
                        Log.Info("Chat: «" + soldier + "» " + message);
                        break;
                    case "team":
                        Log.Info("Chat: TEAM " + subset[1] + " «" + soldier + "» " + message);
                        break;
                    case "squad":
                        Log.Info("Chat: TEAM " + subset[1] + " SQUAD " + subset[1] + " «" + soldier + "» " + message);
                        break;
                    case "player":
                        Log.Info("Chat: «" + soldier + "» -> «" + subset[1] + "» " + message);
                        break;
                    default:
                        Log.Info("OnChat - subset[0] = " + subset[0] + " - " + soldier + " - " + message);
                        break;
                }
            }
        }
    }
}
