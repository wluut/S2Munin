using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace S2.Munin.Plugin
{
    public class Logger : ILogger
    {
        const string loggingConfiguration = @"<log4net>
<root>
  <level value='ALL' />
    <appender-ref ref='Appender' />
</root>
<appender name='Appender' type='log4net.Appender.RollingFileAppender'>
    <file value='S2Munin.log'/>
    <appendToFile value='true'/>
    <rollingStyle value='Size'/>
    <maxSizeRollBackups value='5'/>
    <maximumFileSize value='10MB'/>
    <layout type='log4net.Layout.PatternLayout'>
        <conversionPattern value='%date [%thread] %-5level %logger - %message%newline'/>
    </layout>
    <lockingModel type='log4net.Appender.FileAppender+MinimalLock'/>
</appender>
</log4net>";

        public static ILogger Instance { get; private set; }

        static Logger()
        {
            // initialize log4net;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(loggingConfiguration);
                log4net.Config.XmlConfigurator.Configure(doc.DocumentElement);
                Logger.Instance = new Logger("S2Munin");
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        private readonly log4net.ILog logger;

        public Logger(String loggerName)
        {
            try
            {
                this.logger = log4net.LogManager.GetLogger(loggerName);
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        public void Fatal(object message)
        {
            try
            {
                this.logger.Fatal(message);
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        public void Fatal(object message, Exception exception)
        {
            try
            {
                this.logger.Fatal(message, exception);
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            Fatal(this.FormatMessage(CultureInfo.InvariantCulture, format, args));
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
            Fatal(this.FormatMessage(provider, format, args));
        }

        public void Error(object message)
        {
            try
            {
                this.logger.Error(message);
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        public void Error(object message, Exception exception)
        {
            try
            {
                this.logger.Error(message, exception);
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Error(this.FormatMessage(CultureInfo.InvariantCulture, format, args));
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
            Error(this.FormatMessage(provider, format, args));
        }

        public void Warning(object message)
        {
            try
            {
                this.logger.Warn(message);
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        public void Warning(object message, Exception exception)
        {
            try
            {
                this.logger.Warn(message, exception);
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        public void WarningFormat(string format, params object[] args)
        {
            Warning(this.FormatMessage(CultureInfo.InvariantCulture, format, args), null);
        }

        public void WarningFormat(IFormatProvider provider, string format, params object[] args)
        {
            Warning(this.FormatMessage(provider, format, args), null);
        }

        public void Info(object message)
        {
            try
            {
                this.logger.Info(message);
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        public void Info(object message, Exception exception)
        {
            try
            {
                this.logger.Info(message, exception);
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        public void InfoFormat(string format, params object[] args)
        {
            Info(this.FormatMessage(CultureInfo.InvariantCulture, format, args), null);
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
            Info(this.FormatMessage(provider, format, args), null);
        }

        public void Debug(object message)
        {
            try
            {
                this.logger.Debug(message);
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        public void Debug(object message, Exception exception)
        {
            try
            {
                this.logger.Debug(message, exception);
            }
            catch (Exception)
            {
                // can't log exception with exeption in logger
            }
        }

        public void DebugFormat(string format, params object[] args)
        {
            Debug(this.FormatMessage(CultureInfo.InvariantCulture, format, args), null);
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
            Debug(this.FormatMessage(provider, format, args), null);
        }

        protected string FormatMessage(IFormatProvider provider, string format, params object[] args)
        {
            return string.Format(provider, format, args);
        }
    }
}
