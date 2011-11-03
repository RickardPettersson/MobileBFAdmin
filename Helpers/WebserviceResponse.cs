using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.VisualBasic;
using System.Configuration;

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
                sb.Append("<html><head><title>Battlefield 3 Server Admin</title>");
                sb.Append("<link rel=\"stylesheet\" href=\"http://code.jquery.com/mobile/1.0rc2/jquery.mobile-1.0rc2.min.css\" />");
                sb.Append("<script src=\"http://code.jquery.com/jquery-1.6.4.min.js\"></script>");
                sb.Append("<script src=\"http://code.jquery.com/mobile/1.0rc2/jquery.mobile-1.0rc2.min.js\"></script>");
                sb.Append("</head><body>");
                sb.Append("<div data-role=\"page\" id=\"playerlist\">");
                sb.Append("<div data-role=\"header\" data-theme=\"b\"><h1>BF3 Server Admin</h1></div>");
                sb.Append("<div data-role=\"content\" id=\"one\">");
                sb.Append("<h2>Playerlist</h2>");
                sb.Append("<ul data-role=\"listview\" data-inset=\"true\" data-theme=\"d\">");
                if (Program.Playerlist.Count == 0)
                {
                    sb.Append("<li><a href=\"#playertodo\">No players on the server</a></li>");
                }
                else
                {
                    foreach (var p in Program.Playerlist)
                    {
                        sb.Append("<li><a href=\"#\">" + p.Name + "</a></li>");
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
                answer = sb.ToString();
            }
            else if (new Regex("/GetStations").Match(myRequest.URL).Success)
            {
                answer = "ERROR";
            }
        }

        public string GetAnswer()
        {
            return answer;
        }
    }
}
