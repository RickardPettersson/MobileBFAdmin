using System;
using System.Collections.Generic;
using System.Web;
using System.Net;

namespace BFAdmin.Helpers
{
    public class WebserviceRequest
    {
        public string URL = string.Empty;
        public string RawData = string.Empty;
        public Dictionary<string, string> Data = new Dictionary<string, string>();
        public HttpListenerRequest RawRequest;

        public WebserviceRequest(string url, string rawdata, HttpListenerRequest request)
        {
            URL = url;
            RawData = rawdata;
            RawRequest = request;
            ParseData();
        }

        private void ParseData()
        {
            // Reset the Data dictionary to be empty if its not was it alrady
            Data = new Dictionary<string, string>();

            // Parse RawData (HTTP POST)
            ParseDataToDictionary(RawData);

            // Try to get query strings
            string tmpURL = URL.Replace("http://", "");
            string[] separator1 = { "/" };
            string[] separator2 = { "?" };
            string[] tmp = tmpURL.Split(separator1, StringSplitOptions.RemoveEmptyEntries);
            string[] tmp2 = tmp[tmp.Length - 1].Split(separator2, StringSplitOptions.RemoveEmptyEntries);

            // Check if we got any querystring
            if (tmp2.Length > 0)
            {
                // Parse Querystrings
                ParseDataToDictionary(tmp2[tmp2.Length - 1]);
            }
        }

        private void ParseDataToDictionary(string inputData)
        {
            string[] seperator1 = { "&" };
            string[] seperator2 = { "=" };
            string[] inputs = inputData.Split(seperator1, StringSplitOptions.RemoveEmptyEntries);

            foreach (string input in inputs)
            {
                string[] inputsplited = input.Split(seperator2, StringSplitOptions.RemoveEmptyEntries);
                if (inputsplited.Length == 2)
                {
                    string key = inputsplited[0];
                    string value = inputsplited[1];

                    Data.Add(key, System.Web.HttpUtility.UrlDecode(value));
                }
            }
        }
    }
}
