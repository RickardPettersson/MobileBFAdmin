using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;

namespace BFAdmin.Helpers
{
    public class Webservice
    {
        // Create HttpListener object and a thread
        private static  HttpListener listener = null;
        private static Thread thread = null;

        public static void Start(string host, int port)
        {
            // Check if listener is null
            if (listener == null)
            {
                // Create new instance of the http listener
                listener = new HttpListener();

                // Setup the endpoint for the listener
                string endpoint = "http://" + host + ":" + port + "/";

                // Setup the endpoint for http listener
                listener.Prefixes.Add(endpoint);

                // Activate Basic Authentication
                listener.AuthenticationSchemes = AuthenticationSchemes.Basic;

                // Start the Http Listener
                listener.Start();

                // Create new thread to handle http requests
                thread = new Thread(() =>
                {
                    for (; ; )
                    {
                        var c = listener.BeginGetContext(new AsyncCallback(HandleRequest), listener);
                        c.AsyncWaitHandle.WaitOne();
                    }

                });

                // Start the thread
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

            // Get the http listener basic identity object
            HttpListenerBasicIdentity identity = (HttpListenerBasicIdentity)context.User.Identity;

            // Check the basic authentiaction with app.config appsettings
            if ((identity.Name == System.Configuration.ConfigurationManager.AppSettings["WebserviceAdminUsername"]) && (identity.Password == System.Configuration.ConfigurationManager.AppSettings["WebserviceAdminPassword"]))
            {
                // Get the URL as string
                string url = request.Url.ToString();

                // Get the raw data for HTTP Posts
                string rawData = new StreamReader(request.InputStream).ReadToEnd();

                // Create the request object
                WebserviceRequest serverRequest = new WebserviceRequest(url, rawData, request);

                // Create the responser, its here the things being done to response
                WebserviceResponse responser = new WebserviceResponse(serverRequest);

                // Get the answer from the responser
                string answer = responser.GetAnswer();

                // Check if we got a answer to redirect to some url
                if (answer.ToLower().Contains("redirect="))
                {
                    // Redirect the user
                    context.Response.Redirect(answer.ToLower().Replace("redirect=", ""));
                }

                // Set up a byte buffer
                byte[] buffer = System.Text.UTF8Encoding.UTF8.GetBytes(answer);

                try
                {
                    // Do the thing to response with html
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
                // Set response status code to 401 (Not authorize)
                context.Response.StatusCode = 401;

                // Do some stuff to say access denied
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
