using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S2.Munin.Plugin
{
    public interface ILogger
    {
        void Fatal(object message);
        void Fatal(object message, Exception exception);

        void FatalFormat(string format, params object[] args);
        void FatalFormat(IFormatProvider provider, string format, params object[] args);

        void Error(object message);
        void Error(object message, Exception exception);

        void ErrorFormat(string format, params object[] args);
        void ErrorFormat(IFormatProvider provider, string format, params object[] args);

        void Warning(object message);
        void Warning(object message, Exception exception);

        void WarningFormat(string format, params object[] args);
        void WarningFormat(IFormatProvider provider, string format, params object[] args);

        void Info(object message);
        void Info(object message, Exception exception);

        void InfoFormat(string format, params object[] args);
        void InfoFormat(IFormatProvider provider, string format, params object[] args);

        void Debug(object message);
        void Debug(object message, Exception exception);

        void DebugFormat(string format, params object[] args);
        void DebugFormat(IFormatProvider provider, string format, params object[] args);
    }
}
