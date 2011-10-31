using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BFAdmin.Helpers
{
    public class Log
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Info(string logMessage)
        {
            logMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + logMessage;

            Console.WriteLine(logMessage);
            logger.Info(logMessage);
        }

        public static void Error(string logMessage)
        {
            logMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + logMessage;

            Console.WriteLine(logMessage);
            logger.Error(logMessage);
        }
    }
}
