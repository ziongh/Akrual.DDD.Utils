using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Akrual.DDD.Utils.Internal.Logging.LogProviders
{
    [ExcludeFromCodeCoverage]
    internal class LoupeLogProvider : LogProviderBase
    {
        /// <summary>
        /// The form of the Loupe Log.Write method we're using
        /// </summary>
        internal delegate void WriteDelegate(
            int severity,
            string logSystem,
            int skipFrames,
            Exception exception,
            bool attributeToException,
            int writeMode,
            string detailsXml,
            string category,
            string caption,
            string description,
            params object[] args
        );

        private static bool s_providerIsAvailableOverride = true;
        private readonly WriteDelegate _logWriteDelegate;

        public LoupeLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("Gibraltar.Agent.Log (Loupe) not found");
            }

            _logWriteDelegate = GetLogWriteDelegate();
        }

        /// <summary>
        /// Gets or sets a value indicating whether [provider is available override]. Used in tests.
        /// </summary>
        /// <value>
        /// <c>true</c> if [provider is available override]; otherwise, <c>false</c>.
        /// </value>
        public static bool ProviderIsAvailableOverride
        {
            get { return s_providerIsAvailableOverride; }
            set { s_providerIsAvailableOverride = value; }
        }

        public override Logger GetLogger(string name)
        {
            return new LoupeLogger(name, _logWriteDelegate).Log;
        }

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        private static Type GetLogManagerType()
        {
            return Type.GetType("Gibraltar.Agent.Log, Gibraltar.Agent");
        }

        private static WriteDelegate GetLogWriteDelegate()
        {
            Type logManagerType = GetLogManagerType();
            Type logMessageSeverityType = Type.GetType("Gibraltar.Agent.LogMessageSeverity, Gibraltar.Agent");
            Type logWriteModeType = Type.GetType("Gibraltar.Agent.LogWriteMode, Gibraltar.Agent");

            MethodInfo method = logManagerType.GetMethodPortable(
                "Write",
                logMessageSeverityType, typeof(string), typeof(int), typeof(Exception), typeof(bool),
                logWriteModeType, typeof(string), typeof(string), typeof(string), typeof(string), typeof(object[]));

            var callDelegate = (WriteDelegate)method.CreateDelegate(typeof(WriteDelegate));
            return callDelegate;
        }

        [ExcludeFromCodeCoverage]
        internal class LoupeLogger
        {
            private const string LogSystem = "LibLog";

            private readonly string _category;
            private readonly WriteDelegate _logWriteDelegate;
            private readonly int _skipLevel;

            internal LoupeLogger(string category, WriteDelegate logWriteDelegate)
            {
                _category = category;
                _logWriteDelegate = logWriteDelegate;
                _skipLevel = 2;
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                if (messageFunc == null)
                {
                    //nothing to log..
                    return true;
                }

                messageFunc = LogMessageFormatter.SimulateStructuredLogging(messageFunc, formatParameters);

                _logWriteDelegate(ToLogMessageSeverity(logLevel), LogSystem, _skipLevel, exception, true, 0, null,
                    _category, null, messageFunc.Invoke());

                return true;
            }

            private static int ToLogMessageSeverity(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        return TraceEventTypeValues.Verbose;
                    case LogLevel.Debug:
                        return TraceEventTypeValues.Verbose;
                    case LogLevel.Info:
                        return TraceEventTypeValues.Information;
                    case LogLevel.Warn:
                        return TraceEventTypeValues.Warning;
                    case LogLevel.Error:
                        return TraceEventTypeValues.Error;
                    case LogLevel.Fatal:
                        return TraceEventTypeValues.Critical;
                    default:
                        throw new ArgumentOutOfRangeException("logLevel");
                }
            }
        }
    }
}