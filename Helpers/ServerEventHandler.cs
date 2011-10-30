using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BFAdmin.Helpers
{
    public class ServerEventHandler
    {
        public static void HandleEvents(List<string> w)
        {
            switch (w[0])
            {
                case "player.onJoin":
                    Log.Write("Soldier \"" + w[1] + "\" has joined the server");
                    break;
                case "player.onAuthenticated":
                    Log.Write("Soldier \"" + w[1] + "\" has been authenticated. GUID=\"" + w[2] + "\"");
                    break;
                case "player.onLeave":
                    Log.Write("Soldier \"" + w[1] + "\" leaved the sever - " + string.Join(" - ", w.ToArray()));
                    break;
                case "player.onKill":
                    string headshot = string.Empty;

                    if (Convert.ToBoolean(w[4]))
                    {
                        headshot = " (headshot)";
                    }

                    Log.Write("Soldier \"" + w[1] + "\" killed \"" + w[2] + "\" with weapon \"" + w[3] + "\"" + headshot);
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
            }
        }

        public static void onChat(string soldier, string message, List<string> subset)
        {
            switch (subset[0])
            {
                case "all":
                    Log.Write("«" + soldier + "» " + message);
                    break;
                case "team":
                    Log.Write("TEAM " + subset[1] + " «" + soldier + "» " + message);
                    break;
                case "squad":
                    Log.Write("TEAM " + subset[1] + " SQUAD " + subset[1] + " «" + soldier + "» " + message);
                    break;
                case "player":
                    Log.Write("«" + soldier + "» -> «" + subset[1] + "» " + message);
                    break;
                default:
                    Log.Write("OnChat - subset[0] = " + subset[0] + " - " + soldier + " - " + message);
                    break;
            }
        }
    }
}
