using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BFAdmin.Helpers
{
    public class Log
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Write(string logMessage)
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + logMessage);
        }
    }
}
