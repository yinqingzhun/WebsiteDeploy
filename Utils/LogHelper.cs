using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using log4net;

namespace WebDeploy.Utils
{
    public class LogHelper
    {
        static LogHelper()
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config")));
        }
        public static void Debug(string msg, Exception ex, string loggerName = "DefaultLog")
        {
            Log(LogLevel.Debug, msg, ex, loggerName);
        }
        public static void Info(string msg, Exception ex, string loggerName = "DefaultLog")
        {
            Log(LogLevel.Info, msg, ex, loggerName);
        }
        public static void Warn(string msg, Exception ex, string loggerName = "DefaultLog")
        {
            Log(LogLevel.Warn, msg, ex, loggerName);
        }
        public static void Error(string msg, Exception ex, string loggerName = "DefaultLog")
        {
            Log(LogLevel.Error, msg, ex, loggerName);
        }
        public static void Fatal(string msg, Exception ex, string loggerName = "DefaultLog")
        {
            Log(LogLevel.Fatal, msg, ex, loggerName);
        }
        private static void Log(LogLevel logLevel, string msg, Exception ex, string loggerName)
        {
            var logger = LogManager.GetLogger(loggerName);
            switch (logLevel)
            {
                case LogLevel.Debug:
                    logger.Debug(msg, ex);
                    break;
                case LogLevel.Info:
                    logger.Info(msg, ex);
                    break;
                case LogLevel.Warn:
                    logger.Warn(msg, ex);
                    break;
                case LogLevel.Error:
                    logger.Error(msg, ex);
                    break;
                case LogLevel.Fatal:
                    logger.Fatal(msg, ex);
                    break;
                default:
                    break;
            }
        }

        private enum LogLevel
        {
            Debug, Info, Warn, Error, Fatal
        }
    }
}
