using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Akrual.DDD.Utils.Internal.Logging.LogProviders;

namespace Akrual.DDD.Utils.Internal.Logging
{
    /// <summary>
    /// Provides a mechanism to create instances of <see cref="Akrual.DDD.Utils.Internal.Logging.ILog" /> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class LogProvider
    {
        private const string NullLogProvider = "Current Log Provider is not set. Call SetCurrentLogProvider " +
                                               "with a non-null value first.";
        private static dynamic s_currentLogProvider;
        private static Action<ILogProvider> s_onCurrentLogProviderSet;
        private static Lazy<ILogProvider> s_resolvedLogProvider = new Lazy<ILogProvider>(() => ForceResolveLogProvider());

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static LogProvider()
        {
            IsDisabled = false;
        }

        /// <summary>
        /// Sets the current log provider.
        /// </summary>
        /// <param name="logProvider">The log provider.</param>
        public static void SetCurrentLogProvider(ILogProvider logProvider)
        {
            s_currentLogProvider = logProvider;

            RaiseOnCurrentLogProviderSet();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this is logging is disabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if logging is disabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsDisabled { get; set; }

        /// <summary>
        /// Sets an action that is invoked when a consumer of your library has called SetCurrentLogProvider. It is 
        /// important that hook into this if you are using child libraries (especially ilmerged ones) that are using
        /// LibLog (or other logging abstraction) so you adapt and delegate to them.
        /// <see cref="SetCurrentLogProvider"/> 
        /// </summary>
        internal static Action<ILogProvider> OnCurrentLogProviderSet
        {
            set
            {
                s_onCurrentLogProviderSet = value;
                RaiseOnCurrentLogProviderSet();
            }
        }

        internal static ILogProvider CurrentLogProvider
        {
            get
            {
                return s_currentLogProvider;
            }
        }

        /// <summary>
        /// Gets a logger for the specified type.
        /// </summary>
        /// <typeparam name="T">The type whose name will be used for the logger.</typeparam>
        /// <returns>An instance of <see cref="Akrual.DDD.Utils.Internal.Logging.ILog"/></returns>
        public
            static ILog For<T>()
        {
            return GetLogger(typeof(T));
        }

        /// <summary>
        /// Gets a logger for the current class.
        /// </summary>
        /// <returns>An instance of <see cref="Akrual.DDD.Utils.Internal.Logging.ILog"/></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ILog GetCurrentClassLogger()
        {
            var stackFrame = new StackFrame(1, false);
            return GetLogger(stackFrame.GetMethod().DeclaringType);
        }

        /// <summary>
        /// Gets a logger for the specified type.
        /// </summary>
        /// <param name="type">The type whose name will be used for the logger.</param>
        /// <param name="fallbackTypeName">If the type is null then this name will be used as the log name instead</param>
        /// <returns>An instance of <see cref="Akrual.DDD.Utils.Internal.Logging.ILog"/></returns>
        public static ILog GetLogger(Type type, string fallbackTypeName = "System.Object")
        {
            // If the type passed in is null then fallback to the type name specified
            return GetLogger(type != null ? type.FullName : fallbackTypeName);
        }

        /// <summary>
        /// Gets a logger with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>An instance of <see cref="Akrual.DDD.Utils.Internal.Logging.ILog"/></returns>
        public static ILog GetLogger(string name)
        {
            ILogProvider logProvider = CurrentLogProvider ?? ResolveLogProvider();
            return logProvider == null
                ? NoOpLogger.Instance
                : (ILog)new LoggerExecutionWrapper(logProvider.GetLogger(name), () => IsDisabled);
        }

        /// <summary>
        /// Opens a nested diagnostics context.
        /// </summary>
        /// <param name="message">A message.</param>
        /// <returns>An <see cref="IDisposable"/> that closes context when disposed.</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SetCurrentLogProvider")]
        public static IDisposable OpenNestedContext(string message)
        {
            ILogProvider logProvider = CurrentLogProvider ?? ResolveLogProvider();

            return logProvider == null
                ? new DisposableAction(() => { })
                : logProvider.OpenNestedContext(message);
        }

        /// <summary>
        /// Opens a mapped diagnostics context.
        /// </summary>
        /// <param name="key">A key.</param>
        /// <param name="value">A value.</param>
        /// <returns>An <see cref="IDisposable"/> that closes context when disposed.</returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SetCurrentLogProvider")]
        public static IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            ILogProvider logProvider = CurrentLogProvider ?? ResolveLogProvider();

            return logProvider == null
                ? new DisposableAction(() => { })
                : logProvider.OpenMappedContext(key, value, destructure);
        }

        internal delegate bool IsLoggerAvailable();

        internal delegate ILogProvider CreateLogProvider();

        internal static readonly List<Tuple<IsLoggerAvailable, CreateLogProvider>> LogProviderResolvers =
            new List<Tuple<IsLoggerAvailable, CreateLogProvider>>
            {
                new Tuple<IsLoggerAvailable, CreateLogProvider>(SerilogLogProvider.IsLoggerAvailable, () => new SerilogLogProvider()),
                new Tuple<IsLoggerAvailable, CreateLogProvider>(NLogLogProvider.IsLoggerAvailable, () => new NLogLogProvider()),
                new Tuple<IsLoggerAvailable, CreateLogProvider>(Log4NetLogProvider.IsLoggerAvailable, () => new Log4NetLogProvider()),
                new Tuple<IsLoggerAvailable, CreateLogProvider>(EntLibLogProvider.IsLoggerAvailable, () => new EntLibLogProvider()),
                new Tuple<IsLoggerAvailable, CreateLogProvider>(LoupeLogProvider.IsLoggerAvailable, () => new LoupeLogProvider()),
                new Tuple<IsLoggerAvailable, CreateLogProvider>(ColoredConsoleLogProvider.IsLoggerAvailable, () => new ColoredConsoleLogProvider()),
            };

        private static void RaiseOnCurrentLogProviderSet()
        {
            if (s_onCurrentLogProviderSet != null)
            {
                s_onCurrentLogProviderSet(s_currentLogProvider);
            }
        }

        internal static ILogProvider ResolveLogProvider()
        {
            return s_resolvedLogProvider.Value;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String,System.Object,System.Object)")]
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static ILogProvider ForceResolveLogProvider()
        {
            try
            {
                foreach (var providerResolver in LogProviderResolvers)
                {
                    if (providerResolver.Item1())
                    {
                        return providerResolver.Item2();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Exception occurred resolving a log provider. Logging for this assembly {0} is disabled. {1}",
                    typeof(LogProvider).GetAssemblyPortable().FullName,
                    ex);
            }
            return null;
        }

        [ExcludeFromCodeCoverage]
        internal class NoOpLogger : ILog
        {
            internal static readonly NoOpLogger Instance = new NoOpLogger();

            public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
            {
                return false;
            }
        }
    }
}