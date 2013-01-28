using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace S2.Munin.Plugin
{
    public abstract class Logger
    {
        protected const string LoggerSource = "S2Munin";
        protected const string LoggerLog = "Application";

        public static void Error(object message)
        {
            Error(message, null);
        }

        public static void Error(object message, Exception e)
        {
            WriteEventLog(EventLogEntryType.Error, message, e);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            Error(FormatMessage(CultureInfo.InvariantCulture, format, args), null);
        }

        public static void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            Error(FormatMessage(provider, format, args), null);
        }

        public static void Warning(object message)
        {
            Warning(message, null);
        }

        public static void Warning(object message, Exception e)
        {
            WriteEventLog(EventLogEntryType.Warning, message, e);
        }

        public static void WarningFormat(string format, params object[] args)
        {
            Warning(FormatMessage(CultureInfo.InvariantCulture, format, args), null);
        }

        public static void WarningFormat(IFormatProvider provider, string format, params object[] args)
        {
            Warning(FormatMessage(provider, format, args), null);
        }

        public static void Info(object message)
        {
            Info(message, null);
        }

        public static void Info(object message, Exception e)
        {
            WriteEventLog(EventLogEntryType.Information, message, e);
        }

        public static void InfoFormat(string format, params object[] args)
        {
            Info(FormatMessage(CultureInfo.InvariantCulture, format, args), null);
        }

        public static void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            Info(FormatMessage(provider, format, args), null);
        }

        protected static string FormatMessage(IFormatProvider provider, string format, params object[] args)
        {
            return string.Format(provider, format, args);
        }

        protected static void WriteEventLog(EventLogEntryType logLevel, object message, Exception e)
        {
            string logMessage = message.ToString();
            if (e != null)
            {
                logMessage += "\n";
                logMessage += e.ToString();
            }
           EventLog.WriteEntry(LoggerSource, logMessage, logLevel);
        }

        internal static void RegisterApplication()
        {
            if (!EventLog.SourceExists(LoggerSource))
            {
                EventLog.CreateEventSource(LoggerSource, LoggerLog);
            }
        }

        internal static void UnRegisterApplication()
        {
            if (EventLog.SourceExists(LoggerSource))
            {
                EventLog.DeleteEventSource(LoggerSource);
            }
        }
    }
}
