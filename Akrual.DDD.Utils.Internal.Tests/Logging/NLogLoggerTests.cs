using Akrual.DDD.Utils.Internal.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;
using Xunit;

namespace Akrual.DDD.Utils.Internal.Tests.Logging
{
    public class NLogLoggerTests
    {
        [Fact]
        public void Log_WithLoggerConfiguredToConsole_PrintOnConsole()
        {
            var mockTarget = new MockLogTarget();
            ConfigureNLogConsole(mockTarget);
            LogProvider.SetCurrentLogProvider(null);
            var logger = LogProvider.GetCurrentClassLogger();

            logger.Debug("Debug message");
            Assert.Equal("Debug message", mockTarget.LastLog.Message);

            logger.Fatal("Fatal message");
            Assert.Equal("Fatal message", mockTarget.LastLog.Message);
        }

        private static void ConfigureNLogConsole(Target target)
        {
            var config = new LoggingConfiguration();

            config.AddTarget("Console", target);

            var dbRule = new LoggingRule("*", NLog.LogLevel.Trace, target);

            config.LoggingRules.Add(dbRule);

            LogManager.Configuration = config;
        }

        internal class MockLogTarget : Target
        {
            public LogEventInfo LastLog { get; set; }
            protected override void Write(LogEventInfo logEvent)
            {
                LastLog = logEvent;
                base.Write(logEvent);
            }
        }
    }
}
