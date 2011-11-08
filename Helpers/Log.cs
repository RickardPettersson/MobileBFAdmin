using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BFAdmin.Helpers
{
    public class Log
    {
        // Create object of NLog Logger object
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Log information, both to console and text file with NLog
        /// </summary>
        /// <param name="logMessage"></param>
        public static void Info(string logMessage)
        {
            logMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + logMessage;

            Console.WriteLine(logMessage);
            logger.Info(logMessage);
        }

        /// <summary>
        /// Log errors, both to console and text file with NLog
        /// </summary>
        /// <param name="logMessage"></param>
        public static void Error(string logMessage)
        {
            logMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + logMessage;

            Console.WriteLine(logMessage);
            logger.Error(logMessage);
        }
    }
}
