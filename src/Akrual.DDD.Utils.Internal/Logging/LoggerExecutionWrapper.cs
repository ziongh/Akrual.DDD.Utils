using System;
using System.Diagnostics.CodeAnalysis;

namespace Akrual.DDD.Utils.Internal.Logging
{
    internal class LoggerExecutionWrapper : ILog
    {
        private readonly Logger _logger;
        private readonly ICallSiteExtension _callsiteLogger;
        private readonly Func<bool> _getIsDisabled;
        internal const string FailedToGenerateLogMessage = "Failed to generate log message";

        Func<string> _lastExtensionMethod;

        internal LoggerExecutionWrapper(Logger logger, Func<bool> getIsDisabled = null)
        {
            _logger = logger;
            _callsiteLogger = new CallSiteExtension();
            _getIsDisabled = getIsDisabled ?? (() => false);
        }

        internal Logger WrappedLogger
        {
            get { return _logger; }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters)
        {
            if (_getIsDisabled())
            {
                return false;
            }
            if (messageFunc == null)
            {
                return _logger(logLevel, null);
            }

            // Callsite HACK - Using the messageFunc to provide the callsite-logger-type
            var lastExtensionMethod = _lastExtensionMethod;
            if (lastExtensionMethod == null || !lastExtensionMethod.Equals(messageFunc))
            {
                // Callsite HACK - Cache the last validated messageFunc as Equals is faster than type-check
                lastExtensionMethod = null;
                var methodType = messageFunc.Method.DeclaringType;
                if (methodType == typeof(LogExtensions) || (methodType != null && methodType.DeclaringType == typeof(LogExtensions)))
                {
                    lastExtensionMethod = messageFunc;
                }
            }

            if (lastExtensionMethod != null)
            {
                // Callsite HACK - LogExtensions has called virtual ILog interface method to get here, callsite-stack is good
                _lastExtensionMethod = lastExtensionMethod;
                return _logger(logLevel, LogExtensions.WrapLogSafeInternal(this, messageFunc), exception, formatParameters);
            }
            else
            {
                Func<string> wrappedMessageFunc = () =>
                {
                    try
                    {
                        return messageFunc();
                    }
                    catch (Exception ex)
                    {
                        _logger(LogLevel.Error, () => FailedToGenerateLogMessage, ex);
                    }
                    return null;
                };

                // Callsite HACK - Need to ensure proper callsite stack without inlining, so calling the logger within a virtual interface method
                return _callsiteLogger.Log(_logger, logLevel, wrappedMessageFunc, exception, formatParameters);
            }
        }

        interface ICallSiteExtension
        {
            bool Log(Logger logger, LogLevel logLevel, Func<string> messageFunc, Exception exception, object[] formatParameters);
        }

        class CallSiteExtension : ICallSiteExtension
        {
            bool ICallSiteExtension.Log(Logger logger, LogLevel logLevel, Func<string> messageFunc, Exception exception, object[] formatParameters)
            {
                return logger(logLevel, messageFunc, exception, formatParameters);
            }
        }
    }
}