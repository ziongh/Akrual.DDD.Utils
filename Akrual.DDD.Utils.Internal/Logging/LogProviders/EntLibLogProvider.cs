using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Akrual.DDD.Utils.Internal.Logging.LogProviders
{
    [ExcludeFromCodeCoverage]
    internal class EntLibLogProvider : LogProviderBase
    {
        private const string TypeTemplate = "Microsoft.Practices.EnterpriseLibrary.Logging.{0}, Microsoft.Practices.EnterpriseLibrary.Logging";
        private static bool s_providerIsAvailableOverride = true;
        private static readonly Type LogEntryType;
        private static readonly Type LoggerType;
        private static readonly Type TraceEventTypeType;
        private static readonly Action<string, string, int> WriteLogEntry;
        private static readonly Func<string, int, bool> ShouldLogEntry;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static EntLibLogProvider()
        {
            LogEntryType = Type.GetType(string.Format(CultureInfo.InvariantCulture, TypeTemplate, "LogEntry"));
            LoggerType = Type.GetType(string.Format(CultureInfo.InvariantCulture, TypeTemplate, "Logger"));
            TraceEventTypeType = TraceEventTypeValues.Type;
            if (LogEntryType == null
                || TraceEventTypeType == null
                || LoggerType == null)
            {
                return;
            }
            WriteLogEntry = GetWriteLogEntry();
            ShouldLogEntry = GetShouldLogEntry();
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EnterpriseLibrary")]
        public EntLibLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("Microsoft.Practices.EnterpriseLibrary.Logging.Logger not found");
            }
        }

        public static bool ProviderIsAvailableOverride
        {
            get { return s_providerIsAvailableOverride; }
            set { s_providerIsAvailableOverride = value; }
        }

        public override Logger GetLogger(string name)
        {
            return new EntLibLogger(name, WriteLogEntry, ShouldLogEntry).Log;
        }

        internal static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride
                   && TraceEventTypeType != null
                   && LogEntryType != null;
        }

        private static Action<string, string, int> GetWriteLogEntry()
        {
            // new LogEntry(...)
            var logNameParameter = Expression.Parameter(typeof(string), "logName");
            var messageParameter = Expression.Parameter(typeof(string), "message");
            var severityParameter = Expression.Parameter(typeof(int), "severity");

            MemberInitExpression memberInit = GetWriteLogExpression(
                messageParameter,
                Expression.Convert(severityParameter, TraceEventTypeType),
                logNameParameter);

            //Logger.Write(new LogEntry(....));
            MethodInfo writeLogEntryMethod = LoggerType.GetMethodPortable("Write", LogEntryType);
            var writeLogEntryExpression = Expression.Call(writeLogEntryMethod, memberInit);

            return Expression.Lambda<Action<string, string, int>>(
                writeLogEntryExpression,
                logNameParameter,
                messageParameter,
                severityParameter).Compile();
        }

        private static Func<string, int, bool> GetShouldLogEntry()
        {
            // new LogEntry(...)
            var logNameParameter = Expression.Parameter(typeof(string), "logName");
            var severityParameter = Expression.Parameter(typeof(int), "severity");

            MemberInitExpression memberInit = GetWriteLogExpression(
                Expression.Constant("***dummy***"),
                Expression.Convert(severityParameter, TraceEventTypeType),
                logNameParameter);

            //Logger.Write(new LogEntry(....));
            MethodInfo writeLogEntryMethod = LoggerType.GetMethodPortable("ShouldLog", LogEntryType);
            var writeLogEntryExpression = Expression.Call(writeLogEntryMethod, memberInit);

            return Expression.Lambda<Func<string, int, bool>>(
                writeLogEntryExpression,
                logNameParameter,
                severityParameter).Compile();
        }

        private static MemberInitExpression GetWriteLogExpression(Expression message,
            Expression severityParameter, ParameterExpression logNameParameter)
        {
            var entryType = LogEntryType;
            MemberInitExpression memberInit = Expression.MemberInit(Expression.New(entryType),
                Expression.Bind(entryType.GetPropertyPortable("Message"), message),
                Expression.Bind(entryType.GetPropertyPortable("Severity"), severityParameter),
                Expression.Bind(
                    entryType.GetPropertyPortable("TimeStamp"),
                    Expression.Property(null, typeof(DateTime).GetPropertyPortable("UtcNow"))),
                Expression.Bind(
                    entryType.GetPropertyPortable("Categories"),
                    Expression.ListInit(
                        Expression.New(typeof(List<string>)),
                        typeof(List<string>).GetMethodPortable("Add", typeof(string)),
                        logNameParameter)));
            return memberInit;
        }

        [ExcludeFromCodeCoverage]
        internal class EntLibLogger
        {
            private readonly string _loggerName;
            private readonly Action<string, string, int> _writeLog;
            private readonly Func<string, int, bool> _shouldLog;

            internal EntLibLogger(string loggerName, Action<string, string, int> writeLog, Func<string, int, bool> shouldLog)
            {
                _loggerName = loggerName;
                _writeLog = writeLog;
                _shouldLog = shouldLog;
            }

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                var severity = MapSeverity(logLevel);
                if (messageFunc == null)
                {
                    return _shouldLog(_loggerName, severity);
                }


                messageFunc = LogMessageFormatter.SimulateStructuredLogging(messageFunc, formatParameters);
                if (exception != null)
                {
                    return LogException(logLevel, messageFunc, exception);
                }
                _writeLog(_loggerName, messageFunc(), severity);
                return true;
            }

            public bool LogException(LogLevel logLevel, Func<string> messageFunc, Exception exception)
            {
                var severity = MapSeverity(logLevel);
                var message = messageFunc() + Environment.NewLine + exception;
                _writeLog(_loggerName, message, severity);
                return true;
            }

            private static int MapSeverity(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Fatal:
                        return TraceEventTypeValues.Critical;
                    case LogLevel.Error:
                        return TraceEventTypeValues.Error;
                    case LogLevel.Warn:
                        return TraceEventTypeValues.Warning;
                    case LogLevel.Info:
                        return TraceEventTypeValues.Information;
                    default:
                        return TraceEventTypeValues.Verbose;
                }
            }
        }
    }
}