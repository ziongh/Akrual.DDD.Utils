using System;
using EventStore.ClientAPI;

namespace Akrual.DDD.Utils.Data.EventStore.Logger
{
    class NLoggerImpl: ILogger
        {
            private readonly NLog.ILogger _logger;

            public NLoggerImpl(NLog.ILogger logger)
            {
                _logger = logger;
            }
            /// <summary>
            /// Writes an error to the logger
            /// </summary>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Error(string format, params object[] args)
            {
                _logger.Error(format,args);
            }

            /// <summary>
            /// Writes an error to the logger
            /// </summary>
            /// <param name="ex">A thrown exception.</param>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Error(Exception ex, string format, params object[] args)
            {
                _logger.Error(ex,format,args);
            }

            /// <summary>
            /// Writes a debug message to the logger
            /// </summary>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Debug(string format, params object[] args)
            {
                _logger.Debug(format,args);
            }

            /// <summary>
            /// Writes a debug message to the logger
            /// </summary>
            /// <param name="ex">A thrown exception.</param>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Debug(Exception ex, string format, params object[] args)
            {
                _logger.Debug(ex,format,args);
            }

            /// <summary>
            /// Writes an information message to the logger
            /// </summary>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Info(string format, params object[] args)
            {
                _logger.Info(format,args);
            }

            /// <summary>
            /// Writes an information message to the logger
            /// </summary>
            /// <param name="ex">A thrown exception.</param>
            /// <param name="format">Format string for the log message.</param>
            /// <param name="args">Arguments to be inserted into the format string.</param>
            public void Info(Exception ex, string format, params object[] args)
            {
                _logger.Info(ex,format,args);
            }
        }
}
