using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.VisualBasic;
using System.Configuration;
using System.IO;

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
                // Create a stringbuilder to put the answer stuff in
                StringBuilder sb = new StringBuilder();
                
                try
                {
                    // Read the html file in to a string
                    answer = File.ReadAllText("www/index.htm");
                }
                catch (Exception ex)
                {
                    string apa = string.Empty;
                }

                // Check if no players on the server
                if (Program.Playerlist.Count == 0)
                {
                    // /do?player=apa
                    sb.Append("<li><a href=\"#\">No players on the server</a></li>");
                }
                else
                {
                    // Loop each player in the player list 
                    foreach (var p in Program.Playerlist)
                    {
                        // Add item to the list
                        sb.Append("<li><a href=\"/do?player=" + p.Name + "\">" + p.Name + "</a></li>");
                    }
                }

                // Add playerlist to the answer with some replace
                answer = answer.Replace("<PLAYERLIST>", sb.ToString());

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

                    // Add the command dictionary to the queue system
                    Program.PlayerCommandsQueue.Enqueue(command);

                    answer = "redirect=/";
                }
            }
        }

        public string GetAnswer()
        {
            return answer;
        }
    }
}
