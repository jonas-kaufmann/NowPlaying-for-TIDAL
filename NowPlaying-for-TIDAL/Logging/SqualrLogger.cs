using Squalr.Engine.Logging;
using System.Diagnostics;

namespace nowplaying_for_tidal.Logging
{
    class SqualrLogger : ILoggerObserver
    {
        public void OnLogEvent(LogLevel logLevel, string message, string innerMessage)
        {
            message = "Squalr: " + message;

            bool printInnerMsg = false;
            if (!string.IsNullOrEmpty(innerMessage))
            {
                printInnerMsg = true;
                innerMessage = "Squalr: " + innerMessage;
            }

            switch (logLevel)
            {
                case LogLevel.Warn:
                    Trace.TraceWarning(message);
                    if (printInnerMsg)
                        Trace.TraceWarning(innerMessage);
                    break;
                case LogLevel.Error:
                    Trace.TraceError(message);
                    if (printInnerMsg)
                        Trace.TraceError(innerMessage);
                    break;
                case LogLevel.Fatal:
                    Trace.TraceError(message);
                    if (printInnerMsg)
                        Trace.TraceError(innerMessage);
                    break;
                default:
                    Trace.TraceInformation(message);
                    if (printInnerMsg)
                        Trace.TraceInformation(innerMessage);
                    break;
            }
        }
    }
}
