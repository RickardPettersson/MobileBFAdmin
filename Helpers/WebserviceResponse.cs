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
        private string answer = "Page not found";

        public WebserviceResponse(WebserviceRequest myRequest)
        {
            string id = string.Empty;

            try
            {
                id = myRequest.Data["id"];
            }
            catch { }

            // TODO: Fix maybe stats function - Helpers.DB.AddStats(id, myRequest.RawRequest.RemoteEndPoint.ToString().Split(':')[0], myRequest.RawRequest.UserAgent, myRequest.URL, protocolVersion);

            // Try to find path
            if (new Regex("/$").Match(myRequest.URL).Success)
            {
                StringBuilder sb = new StringBuilder();
                /*sb.Append("<html><head><title>Battlefield 3 Server Admin</title>");
                sb.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
                sb.Append("<link rel=\"stylesheet\" href=\"http://code.jquery.com/mobile/1.0rc2/jquery.mobile-1.0rc2.min.css\" />");
                sb.Append("<script src=\"http://code.jquery.com/jquery-1.6.4.min.js\"></script>");
                sb.Append("<script src=\"http://code.jquery.com/mobile/1.0rc2/jquery.mobile-1.0rc2.min.js\"></script>");
                sb.Append("<script src=\"https://raw.github.com/jblas/jquery-mobile-plugins/master/page-params/jqm.page.params.js\"></script>");
                sb.Append("</head><body>");
                sb.Append("<div data-role=\"page\" id=\"playerlist\">");
                sb.Append("<div data-role=\"header\" data-theme=\"b\"><h1>BF3 Server Admin</h1></div>");
                sb.Append("<div data-role=\"content\" id=\"one\">");
                sb.Append("<h2>Playerlist</h2>");
                sb.Append("<ul data-role=\"listview\" data-inset=\"true\" data-theme=\"d\">");
                if (Program.Playerlist.Count == 0)
                {
                    sb.Append("<li><a href=\"#\">No players on the serve    r</a></li>");
                }
                else
                {
                    foreach (var p in Program.Playerlist)
                    {
                        sb.Append("<li><a href=\"#playertodo\">" + p.Name + "</a></li>");
                    }
                }
                sb.Append("</ul>");
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("<div data-role=\"page\" id=\"playertodo\">");
                sb.Append("<div data-role=\"header\" data-theme=\"b\"><h1>BF3 Server Admin</h1></div>");
                sb.Append("<div data-role=\"content\" id=\"one\">");
                sb.Append("<h2>Do with player XXX</h2>");
                sb.Append("<ul data-role=\"listview\" data-inset=\"true\" data-theme=\"d\">");
                sb.Append("<li><a href=\"#\">Kick</a></li>");
                sb.Append("<li><a href=\"#\">Ban</a></li>");
                sb.Append("</ul>");
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("</body></html>");
                answer = sb.ToString();*/
                try
                {
                    answer = File.ReadAllText("www/index.htm");
                }
                catch (Exception ex)
                {
                    string apa = string.Empty;
                }

                if (Program.Playerlist.Count == 0)
                {
                    sb.Append("<li><a href=\"/do?player=apa\">No players on the server</a></li>");
                }
                else
                {
                    foreach (var p in Program.Playerlist)
                    {
                        sb.Append("<li><a href=\"/do?player=" + p.Name + "\">" + p.Name + "</a></li>");
                    }
                }

                answer = answer.Replace("<PLAYERLIST>", sb.ToString());

            }
            else if (new Regex("/do").Match(myRequest.URL).Success)
            {
                answer = File.ReadAllText("www/do.htm").Replace("REPLACETHISWITHPLAYERNAME", myRequest.Data["player"]);
            }
            else if (new Regex("/command").Match(myRequest.URL).Success)
            {
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

                    answer = "OK";
                }
            }
        }

        public string GetAnswer()
        {
            return answer;
        }
    }
}
