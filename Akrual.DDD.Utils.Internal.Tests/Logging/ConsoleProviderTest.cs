using System;
using System.Collections.Generic;
using System.Text;
using Akrual.DDD.Utils.Internal.Logging;
using NLog;
using NLog.Config;
using NLog.Targets;
using Xunit;
using Logger = Akrual.DDD.Utils.Internal.Logging.Logger;
using LogLevel = Akrual.DDD.Utils.Internal.Logging.LogLevel;

namespace Akrual.DDD.Utils.Internal.Tests.Logging
{
    public class ConsoleProviderTest
    {
        [Fact]
        public void Log_WithLoggerConfiguredToConsole_PrintOnConsole()
        {
            var mockLogger = new MockLogProvider();
            LogProvider.SetCurrentLogProvider(mockLogger);
            var logger = LogProvider.GetCurrentClassLogger();

            logger.Debug("Debug message");
            Assert.Equal("Debug message", mockLogger.LastMessage);

            logger.Fatal("Fatal message");
            Assert.Equal("Fatal message", mockLogger.LastMessage);
        }

        internal class MockLogProvider : ILogProvider
        {
            public string LastMessage { get; set; }
            public Logger GetLogger(string name)
            {
                return (level, func, exception, parameters) =>
                {
                    if(func != null)
                        LastMessage = func.Invoke();
                    return true;
                };
            }

            public IDisposable OpenNestedContext(string message)
            {
                return null;
            }

            public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
            {
                return null;
            }
        }
    }
}
