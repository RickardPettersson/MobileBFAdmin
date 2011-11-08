using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;

namespace BFAdmin.Helpers
{
    public class Webservice
    {
        private static  HttpListener listener = null;
        private static Thread thread = null;

        public static void Start(int port)
        {
            if (listener == null)
            {
                listener = new HttpListener();

                string endpoint = "http://*:" + port + "/";

                listener.Prefixes.Add(endpoint);
                listener.AuthenticationSchemes = AuthenticationSchemes.Basic;

                listener.Start();

                thread = new Thread(() =>
                {
                    for (; ; )
                    {
                        var c = listener.BeginGetContext(new AsyncCallback(HandleRequest), listener);
                        c.AsyncWaitHandle.WaitOne();
                    }

                });

                thread.Start();

                Log.Info("WebService Listening on " + endpoint);
            }
            else
            {
                Log.Info("WebService already started.");
            }
        }

        public static void Stop()
        {
            thread.Abort();
            thread.Join();
            listener.Close();
            listener = null;
        }

        private static void HandleRequest(IAsyncResult result)
        {
            // Get context and request
            HttpListener listener = (HttpListener)result.AsyncState;
            HttpListenerContext context = listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;

            // Console log the request
            Log.Info("Webservice > Request: " + request.RemoteEndPoint.ToString().Split(':')[0] + " - HTTP " + request.HttpMethod + " - " + request.Url);

            HttpListenerBasicIdentity identity = (HttpListenerBasicIdentity)context.User.Identity;
            if ((identity.Name == System.Configuration.ConfigurationManager.AppSettings["WebserviceAdminUsername"]) && (identity.Password == System.Configuration.ConfigurationManager.AppSettings["WebserviceAdminPassword"]))
            {

                // Get the URL as string
                string url = request.Url.ToString();

                // Get the raw data for HTTP Posts
                string rawData = new StreamReader(request.InputStream).ReadToEnd();

                WebserviceRequest serverRequest = new WebserviceRequest(url, rawData, request);

                WebserviceResponse responser = new WebserviceResponse(serverRequest);

                string answer = responser.GetAnswer();

                byte[] buffer = System.Text.UTF8Encoding.UTF8.GetBytes(answer);

                try
                {
                    HttpListenerResponse response = context.Response;
                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                catch (Exception ex)
                {
                    Log.Error("Webservice > HandleRequest - Exception: " + ex.ToString());
                }
            }
            else
            {
                context.Response.StatusCode = 401;

                context.Response.AddHeader("WWW-Authenticate",
                    "Basic Realm=\"Battlefield Server Admin\""); // show login dialog
                byte[] message = new UTF8Encoding().GetBytes("Access denied");
                context.Response.ContentLength64 = message.Length;
                context.Response.OutputStream.Write(message, 0, message.Length);

                try
                {
                    context.Response.Close();
                }
                catch
                {
                    // client closed connection before the content was sent
                }
            }
        }
    }
}
