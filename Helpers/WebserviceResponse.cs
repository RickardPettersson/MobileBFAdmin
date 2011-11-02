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
                answer = @"
                    <html>
                        <head>
                            <title>HEJ</title>
                        </head>
                        <body>
                            <h1>HEJ</h1>
                        </body>
                    </html>
                ";
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
