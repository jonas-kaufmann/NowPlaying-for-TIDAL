using DiscordRPC.Logging;
using System.Diagnostics;

namespace discord_rpc_tidal.Logging
{
    class DiscordLogger : ILogger
    {
        LogLevel ILogger.Level { get; set; } = LogLevel.Warning;

        void ILogger.Error(string message, params object[] args)
        {
            Trace.TraceError(message, args);
        }

        void ILogger.Info(string message, params object[] args)
        {
            Trace.TraceInformation(message, args);
        }

        void ILogger.Trace(string message, params object[] args)
        {
            Trace.TraceInformation(message, args);
        }

        void ILogger.Warning(string message, params object[] args)
        {
            Trace.TraceWarning(message, args);
        }
    }
}
