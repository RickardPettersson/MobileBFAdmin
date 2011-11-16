using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.VisualBasic;
using System.Configuration;
using System.IO;
using System.Net.Battlefield3;

namespace BFAdmin.Helpers
{
    public class WebserviceResponse
    {
        // Create the answer and set default answer
        private string answer = "Page not found";

        public WebserviceResponse(WebserviceRequest myRequest)
        {
            // TODO: Fix maybe stats function - Helpers.DB.AddStats(id, myRequest.RawRequest.RemoteEndPoint.ToString().Split(':')[0], myRequest.RawRequest.UserAgent, myRequest.URL, protocolVersion);

            // Try to find path
            if (new Regex("/$").Match(myRequest.URL).Success)
            {
                try
                {
                    // Read the html file in to a string
                    answer = File.ReadAllText("www/index.htm");
                }
                catch (Exception ex)
                {
                    string apa = string.Empty;
                }

                StringBuilder sb = new StringBuilder();

                string serverName = string.Empty;
                int serverSlots = -1;

                Packet serverInfo = Program.rconClient.SendRequestSync("serverInfo");
                if (serverInfo.Success())
                {
                    serverName = serverInfo.Words[1];
                    serverSlots = Convert.ToInt32(serverInfo.Words[3]);
                }

                if (string.IsNullOrEmpty(serverName))
                {
                    sb.Append("<h3>Server status</h3>");
                }
                else
                {
                    sb.Append("<h3>" + serverName + "</h3>");
                }

                MapCollection mapCollextion = Program.rconClient.Maps;
                Map currentMap = mapCollextion.CurrentMap;

                sb.Append("<div class=\"ui-grid-a\">");

                try
                {
                    if (currentMap != null)
                    {
                        sb.Append("<div class=\"ui-block-a\">Current map:</div><div class=\"ui-block-b\">" + currentMap.FriendlyName + "</div>");
                        sb.Append("<div class=\"ui-block-a\">Current mode:</div><div class=\"ui-block-b\">" + currentMap.FriendlyMode + "</div>");
                        sb.Append("<div class=\"ui-block-a\">Round:</div><div class=\"ui-block-b\">" + currentMap.CurrentRound + "/" + currentMap.TotalRounds + "</div>");
                    }
                }
                catch (Exception ex)
                {
                    string tmp = string.Empty;
                }

                if (serverSlots > -1)
                {
                    sb.Append("<div class=\"ui-block-a\">Number of players:</div><div class=\"ui-block-b\">" + Program.rconClient.Players.Count + "/" + serverSlots + "</div>");
                }
                else
                {
                    sb.Append("<div class=\"ui-block-a\">Number of players:</div><div class=\"ui-block-b\">" + Program.rconClient.Players.Count + "</div>");
                }
                
                sb.Append("</div>");
                
                sb.Append("<ul data-role=\"listview\" data-inset=\"true\" data-theme=\"d\">");
                sb.Append("<li><a href=\"/Playerlist\">Playerlist</a></li>");
                sb.Append("<li><a href=\"/Maplist\">Maplist</a></li>");
                sb.Append("</ul>");

                // Add playerlist to the answer with some replace
                answer = answer.Replace("<CONTENT>", sb.ToString());
            }
            else if (new Regex("/Playerlist").Match(myRequest.URL).Success)
            {
                // Create a stringbuilder to put the answer stuff in
                StringBuilder sb = new StringBuilder();

                sb.Append("<h2>Playerlist</h2>");

                try
                {
                    // Read the html file in to a string
                    answer = File.ReadAllText("www/playerlist.htm");
                }
                catch (Exception ex)
                {
                    string apa = string.Empty;
                }

                // Check if no players on the server
                if (Program.Playerlist.Count == 0)
                {
                    // /do?player=apa
                    sb.Append("<ul data-role=\"listview\" data-inset=\"true\" data-theme=\"d\">");
                    sb.Append("<li><a href=\"#\">No players on the server</a></li>");
                    sb.Append("</ul>");
                }
                else
                {
                    var players = from player in Program.Playerlist orderby player.TeamId, player.Score descending select player;

                    int teamId = 0; 

                    // Loop each player in the player list 
                    foreach (var p in players)
                    {
                        if (teamId == 0)
                        {
                            sb.Append("<h3>Team 1</h3><ul data-role=\"listview\" data-inset=\"true\" data-theme=\"d\">");
                            teamId = p.TeamId;
                        }
                        else if (teamId != p.TeamId)
                        {
                            sb.Append("</ul><br/><h3>Team 2</h3>");
                            sb.Append("<ul data-role=\"listview\" data-inset=\"true\" data-theme=\"d\">");
                        }
                        // Add item to the list
                        sb.Append("<li><a href=\"/do?player=" + p.Name + "\">" + p.Score + " - " + p.Name + " - " + p.Kills + "/" + p.Deaths + "</a></li>");
                    }
                    sb.Append("</ul>");
                }

                // Add playerlist to the answer with some replace
                answer = answer.Replace("<CONTENT>", sb.ToString());

            }
            else if (new Regex("/do").Match(myRequest.URL).Success)
            {
                // Get the html file to string and replace the text with the player name
                answer = File.ReadAllText("www/do.htm").Replace("REPLACETHISWITHPLAYERNAME", myRequest.Data["player"]);
            }
            else if (new Regex("/command").Match(myRequest.URL).Success)
            {
                // Check if some parameters/querystrings not there
                if ((!myRequest.Data.Keys.Contains("player")) || (!myRequest.Data.Keys.Contains("do")))
                {
                    answer = "ERROR";
                }
                else
                {
                    // Get player and todo querystrings
                    string player = myRequest.Data["player"];
                    string todo = myRequest.Data["do"];

                    // Create disctionary to put the command on
                    Dictionary<string, string> command = new Dictionary<string, string>();

                    // Add player and command to the dictionary
                    command.Add(player, todo.ToLower());

                    // Check if we got a temporaryban to do
                    if (todo.ToLower() == "temporaryban")
                    {
                        // Check if we got number of seconds as querystring
                        if (myRequest.Data.Keys.Contains("no"))
                        {
                            command.Add("seconds", myRequest.Data["no"]);
                        }
                        else
                        {
                            command.Add("seconds", "60");
                        }
                    }

                    // Check if we got a roundban to do
                    if (todo.ToLower() == "roundban")
                    {
                        // Check if we got number of rounds as querystring
                        if (myRequest.Data.Keys.Contains("no"))
                        {
                            command.Add("rounds", myRequest.Data["no"]);
                        }
                        else
                        {
                            command.Add("rounds", "1");
                        }
                    }

                    // Add the command dictionary to the queue system
                    Program.PlayerCommandsQueue.Enqueue(command);

                    answer = "redirect=/Playerlist";
                }
            }
            else if (new Regex("/Maplist").Match(myRequest.URL).Success)
            {
                try
                {
                    // Read the html file in to a string
                    answer = File.ReadAllText("www/page.htm");
                }
                catch (Exception ex)
                {
                    string apa = string.Empty;
                }

                StringBuilder sb = new StringBuilder();

                sb.Append("<h2>Maplist</h2>");
                sb.Append("Yellow = Current map, Blue = Next map");
                sb.Append("<ul data-role=\"listview\" data-inset=\"true\" data-theme=\"d\">");

                MapCollection mapCollextion = Program.rconClient.Maps;
                Map currentMap = mapCollextion.CurrentMap;
                Map nextMap = mapCollextion.NextMap;
                
                foreach (Map map in mapCollextion)
                {
                    if ((map.FriendlyName == currentMap.FriendlyName) && (map.FriendlyMode == currentMap.FriendlyMode) && (map.CurrentRound == currentMap.CurrentRound) && (map.TotalRounds == currentMap.TotalRounds))
                    {
                        sb.Append("<li data-theme=\"e\"><a href=\"#\">" + map.FriendlyName + " - " + map.FriendlyMode + " <span class=\"ui-li-count\">" + map.CurrentRound + "/" + map.TotalRounds + "</span></a></li>");
                    }
                    else if ((map.FriendlyName == nextMap.FriendlyName) && (map.FriendlyMode == nextMap.FriendlyMode) && (map.CurrentRound == nextMap.CurrentRound) && (map.TotalRounds == nextMap.TotalRounds))
                    {
                        sb.Append("<li data-theme=\"b\"><a href=\"#\">" + map.FriendlyName + " - " + map.FriendlyMode + " <span class=\"ui-li-count\">" + map.CurrentRound + "/" + map.TotalRounds + "</span></a></li>");
                    }
                    else
                    {
                        sb.Append("<li><a href=\"#\">" + map.FriendlyName + " - " + map.FriendlyMode + " <span class=\"ui-li-count\">" + map.CurrentRound + "/" + map.TotalRounds + "</span></a></li>");
                    }
                }

                sb.Append("</ul>");

                sb.Append("<h2>Commands</h2>");
                sb.Append("<ul data-role=\"listview\" data-inset=\"true\" data-theme=\"d\">");
                sb.Append("<li><a href=\"/MapCommand?cmd=endround&no=1\">End round (Team 1 wins)</a></li>");
                sb.Append("<li><a href=\"/MapCommand?cmd=endround&no=2\">End round (Team 2 wins)</a></li>");
                sb.Append("<li><a href=\"/MapCommand?cmd=runnextround\">Run next round</a></li>");
                sb.Append("<li><a href=\"/MapCommand?cmd=restartround\">Restart round</a></li>");
                sb.Append("<li><a href=\"/SetNextMap\">Set next map</a></li>");
                sb.Append("</ul>");

                // Add content to the page
                answer = answer.Replace("<BACKURL>", "/").Replace("<CONTENT>", sb.ToString());
            }
            else if (new Regex("/SetNextMap").Match(myRequest.URL).Success)
            {
                try
                {
                    // Read the html file in to a string
                    answer = File.ReadAllText("www/page.htm");
                }
                catch (Exception ex)
                {
                    string apa = string.Empty;
                }

                StringBuilder sb = new StringBuilder();

                sb.Append("<h2>Set next map</h2>");
                sb.Append("Yellow = Current map, Blue = Next map");
                sb.Append("<ul data-role=\"listview\" data-inset=\"true\" data-theme=\"d\">");

                MapCollection mapCollextion = Program.rconClient.Maps;
                Map currentMap = mapCollextion.CurrentMap;
                Map nextMap = mapCollextion.NextMap;

                int mapIndex = 0;

                foreach (Map map in mapCollextion)
                {
                    if ((map.FriendlyName == currentMap.FriendlyName) && (map.FriendlyMode == currentMap.FriendlyMode) && (map.CurrentRound == currentMap.CurrentRound) && (map.TotalRounds == currentMap.TotalRounds))
                    {
                        sb.Append("<li data-theme=\"e\"><a href=\"/MapCommand?cmd=setnextmap&no=" + mapIndex + "\">" + map.FriendlyName + " - " + map.FriendlyMode + " <span class=\"ui-li-count\">" + map.CurrentRound + "/" + map.TotalRounds + "</span></a></li>");
                    }
                    else if ((map.FriendlyName == nextMap.FriendlyName) && (map.FriendlyMode == nextMap.FriendlyMode) && (map.CurrentRound == nextMap.CurrentRound) && (map.TotalRounds == nextMap.TotalRounds))
                    {
                        sb.Append("<li data-theme=\"b\"><a href=\"/MapCommand?cmd=setnextmap&no=" + mapIndex + "\">" + map.FriendlyName + " - " + map.FriendlyMode + " <span class=\"ui-li-count\">" + map.CurrentRound + "/" + map.TotalRounds + "</span></a></li>");
                    }
                    else
                    {
                        sb.Append("<li><a href=\"/MapCommand?cmd=setnextmap&no=" + mapIndex + "\">" + map.FriendlyName + " - " + map.FriendlyMode + " <span class=\"ui-li-count\">" + map.CurrentRound + "/" + map.TotalRounds + "</span></a></li>");
                    }
                    mapIndex += 1;
                }

                sb.Append("</ul>");

                // Add content to the page
                answer = answer.Replace("<BACKURL>", "/Maplist").Replace("<CONTENT>", sb.ToString());
            }
            else if (new Regex("/MapCommand").Match(myRequest.URL).Success)
            {
                // Set some default values
                string cmd = string.Empty;
                int cmdNumber = 1;

                // Check if we got command querystring
                if (myRequest.Data.Keys.Contains("cmd"))
                {
                    cmd = myRequest.Data["cmd"];
                }
                
                // Check if we got command number querystring
                if (myRequest.Data.Keys.Contains("no"))
                {
                    cmdNumber = Convert.ToInt32(myRequest.Data["no"]);
                }

                MapCollection mapCollextion = Program.rconClient.Maps;

                switch (cmd.ToLower())
                {
                    case "endround":
                        mapCollextion.EndRound(cmdNumber);
                        break;
                    case "restartround":
                        mapCollextion.RestartRound();
                        break;
                    case "runnextround":
                        mapCollextion.RunNextRound();
                        break;
                    case "setnextmap":
                        mapCollextion.SetNextMap(cmdNumber);
                        break;
                }

                answer = "redirect=/Maplist";
            }
        }

        public string GetAnswer()
        {
            return answer;
        }
    }
}
