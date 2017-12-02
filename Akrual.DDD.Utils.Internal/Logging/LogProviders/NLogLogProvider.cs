using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Akrual.DDD.Utils.Internal.Logging.LogProviders
{
    [ExcludeFromCodeCoverage]
    internal class NLogLogProvider : LogProviderBase
    {
        private readonly Func<string, object> _getLoggerByNameDelegate;
        private static bool s_providerIsAvailableOverride = true;

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "LogManager")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "NLog")]
        public NLogLogProvider()
        {
            if (!IsLoggerAvailable())
            {
                throw new InvalidOperationException("NLog.LogManager not found");
            }
            _getLoggerByNameDelegate = GetGetLoggerMethodCall();
        }

        public static bool ProviderIsAvailableOverride
        {
            get { return s_providerIsAvailableOverride; }
            set { s_providerIsAvailableOverride = value; }
        }

        public override Logger GetLogger(string name)
        {
            return new NLogLogger(_getLoggerByNameDelegate(name)).Log;
        }

        public static bool IsLoggerAvailable()
        {
            return ProviderIsAvailableOverride && GetLogManagerType() != null;
        }

        protected override OpenNdc GetOpenNdcMethod()
        {
            Type ndcContextType = Type.GetType("NLog.NestedDiagnosticsContext, NLog");
            MethodInfo pushMethod = ndcContextType.GetMethodPortable("Push", typeof(string));
            ParameterExpression messageParam = Expression.Parameter(typeof(string), "message");
            MethodCallExpression pushMethodCall = Expression.Call(null, pushMethod, messageParam);
            return Expression.Lambda<OpenNdc>(pushMethodCall, messageParam).Compile();
        }

        protected override OpenMdc GetOpenMdcMethod()
        {
            Type mdcContextType = Type.GetType("NLog.MappedDiagnosticsContext, NLog");

            MethodInfo setMethod = mdcContextType.GetMethodPortable("Set", typeof(string), typeof(string));
            MethodInfo removeMethod = mdcContextType.GetMethodPortable("Remove", typeof(string));
            ParameterExpression keyParam = Expression.Parameter(typeof(string), "key");
            ParameterExpression valueParam = Expression.Parameter(typeof(string), "value");

            MethodCallExpression setMethodCall = Expression.Call(null, setMethod, keyParam, valueParam);
            MethodCallExpression removeMethodCall = Expression.Call(null, removeMethod, keyParam);

            Action<string, string> set = Expression
                .Lambda<Action<string, string>>(setMethodCall, keyParam, valueParam)
                .Compile();
            Action<string> remove = Expression
                .Lambda<Action<string>>(removeMethodCall, keyParam)
                .Compile();

            return (key, value, _) =>
            {
                set(key, value.ToString());
                return new DisposableAction(() => remove(key));
            };
        }

        private static Type GetLogManagerType()
        {
            return Type.GetType("NLog.LogManager, NLog");
        }

        private static Func<string, object> GetGetLoggerMethodCall()
        {
            Type logManagerType = GetLogManagerType();
            MethodInfo method = logManagerType.GetMethodPortable("GetLogger", typeof(string));
            ParameterExpression nameParam = Expression.Parameter(typeof(string), "name");
            MethodCallExpression methodCall = Expression.Call(null, method, nameParam);
            return Expression.Lambda<Func<string, object>>(methodCall, nameParam).Compile();
        }

        [ExcludeFromCodeCoverage]
        internal class NLogLogger
        {
            private readonly dynamic _logger;

            private static Func<string, object, string, Exception, object> _logEventInfoFact;

            private static readonly object _levelTrace;
            private static readonly object _levelDebug;
            private static readonly object _levelInfo;
            private static readonly object _levelWarn;
            private static readonly object _levelError;
            private static readonly object _levelFatal;

            static NLogLogger()
            {
                try
                {
                    var logEventLevelType = Type.GetType("NLog.LogLevel, NLog");
                    if (logEventLevelType == null)
                    {
                        throw new InvalidOperationException("Type NLog.LogLevel was not found.");
                    }

                    var levelFields = logEventLevelType.GetFieldsPortable().ToList();
                    _levelTrace = levelFields.First(x => x.Name == "Trace").GetValue(null);
                    _levelDebug = levelFields.First(x => x.Name == "Debug").GetValue(null);
                    _levelInfo = levelFields.First(x => x.Name == "Info").GetValue(null);
                    _levelWarn = levelFields.First(x => x.Name == "Warn").GetValue(null);
                    _levelError = levelFields.First(x => x.Name == "Error").GetValue(null);
                    _levelFatal = levelFields.First(x => x.Name == "Fatal").GetValue(null);

                    var logEventInfoType = Type.GetType("NLog.LogEventInfo, NLog");
                    if (logEventInfoType == null)
                    {
                        throw new InvalidOperationException("Type NLog.LogEventInfo was not found.");
                    }

                    ConstructorInfo loggingEventConstructor =
                        logEventInfoType.GetConstructorPortable(logEventLevelType, typeof(string), typeof(IFormatProvider), typeof(string), typeof(object[]), typeof(Exception));

                    ParameterExpression loggerNameParam = Expression.Parameter(typeof(string));
                    ParameterExpression levelParam = Expression.Parameter(typeof(object));
                    ParameterExpression messageParam = Expression.Parameter(typeof(string));
                    ParameterExpression exceptionParam = Expression.Parameter(typeof(Exception));
                    UnaryExpression levelCast = Expression.Convert(levelParam, logEventLevelType);

                    NewExpression newLoggingEventExpression =
                        Expression.New(loggingEventConstructor,
                            levelCast,
                            loggerNameParam,
                            Expression.Constant(null, typeof(IFormatProvider)),
                            messageParam,
                            Expression.Constant(null, typeof(object[])),
                            exceptionParam
                        );

                    _logEventInfoFact = Expression.Lambda<Func<string, object, string, Exception, object>>(newLoggingEventExpression,
                        loggerNameParam, levelParam, messageParam, exceptionParam).Compile();
                }
                catch { }
            }

            internal NLogLogger(dynamic logger)
            {
                _logger = logger;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                if (messageFunc == null)
                {
                    return IsLogLevelEnable(logLevel);
                }

                var callsiteMessageFunc = messageFunc;
                messageFunc = LogMessageFormatter.SimulateStructuredLogging(messageFunc, formatParameters);

                if (_logEventInfoFact != null)
                {
                    if (IsLogLevelEnable(logLevel))
                    {
                        Type callsiteLoggerType = typeof(NLogLogger);
                        // Callsite HACK - Extract the callsite-logger-type from the messageFunc
                        var methodType = callsiteMessageFunc.Method.DeclaringType;
                        if (methodType == typeof(LogExtensions) || (methodType != null && methodType.DeclaringType == typeof(LogExtensions)))
                        {
                            callsiteLoggerType = typeof(LogExtensions);
                        }
                        else if (methodType == typeof(LoggerExecutionWrapper) || (methodType != null && methodType.DeclaringType == typeof(LoggerExecutionWrapper)))
                        {
                            callsiteLoggerType = typeof(LoggerExecutionWrapper);
                        }
                        var nlogLevel = this.TranslateLevel(logLevel);
                        var nlogEvent = _logEventInfoFact(_logger.Name, nlogLevel, messageFunc(), exception);
                        _logger.Log(callsiteLoggerType, nlogEvent);
                        return true;
                    }
                    return false;
                }

                if (exception != null)
                {
                    return LogException(logLevel, messageFunc, exception);
                }

                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.Debug(messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Info:
                        if (_logger.IsInfoEnabled)
                        {
                            _logger.Info(messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Warn:
                        if (_logger.IsWarnEnabled)
                        {
                            _logger.Warn(messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Error:
                        if (_logger.IsErrorEnabled)
                        {
                            _logger.Error(messageFunc());
                            return true;
                        }
                        break;
                    case LogLevel.Fatal:
                        if (_logger.IsFatalEnabled)
                        {
                            _logger.Fatal(messageFunc());
                            return true;
                        }
                        break;
                    default:
                        if (_logger.IsTraceEnabled)
                        {
                            _logger.Trace(messageFunc());
                            return true;
                        }
                        break;
                }
                return false;
            }

            [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
            private bool LogException(LogLevel logLevel, Func<string> messageFunc, Exception exception)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        if (_logger.IsDebugEnabled)
                        {
                            _logger.DebugException(messageFunc(), exception);
                            return true;
                        }
                        break;
                    case LogLevel.Info:
                        if (_logger.IsInfoEnabled)
                        {
                            _logger.InfoException(messageFunc(), exception);
                            return true;
                        }
                        break;
                    case LogLevel.Warn:
                        if (_logger.IsWarnEnabled)
                        {
                            _logger.WarnException(messageFunc(), exception);
                            return true;
                        }
                        break;
                    case LogLevel.Error:
                        if (_logger.IsErrorEnabled)
                        {
                            _logger.ErrorException(messageFunc(), exception);
                            return true;
                        }
                        break;
                    case LogLevel.Fatal:
                        if (_logger.IsFatalEnabled)
                        {
                            _logger.FatalException(messageFunc(), exception);
                            return true;
                        }
                        break;
                    default:
                        if (_logger.IsTraceEnabled)
                        {
                            _logger.TraceException(messageFunc(), exception);
                            return true;
                        }
                        break;
                }
                return false;
            }

            private bool IsLogLevelEnable(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Debug:
                        return _logger.IsDebugEnabled;
                    case LogLevel.Info:
                        return _logger.IsInfoEnabled;
                    case LogLevel.Warn:
                        return _logger.IsWarnEnabled;
                    case LogLevel.Error:
                        return _logger.IsErrorEnabled;
                    case LogLevel.Fatal:
                        return _logger.IsFatalEnabled;
                    default:
                        return _logger.IsTraceEnabled;
                }
            }

            private object TranslateLevel(LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Trace:
                        return _levelTrace;
                    case LogLevel.Debug:
                        return _levelDebug;
                    case LogLevel.Info:
                        return _levelInfo;
                    case LogLevel.Warn:
                        return _levelWarn;
                    case LogLevel.Error:
                        return _levelError;
                    case LogLevel.Fatal:
                        return _levelFatal;
                    default:
                        throw new ArgumentOutOfRangeException("logLevel", logLevel, null);
                }
            }
        }
    }
}