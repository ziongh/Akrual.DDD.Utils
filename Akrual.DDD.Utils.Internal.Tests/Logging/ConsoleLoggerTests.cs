using System;
using System.Collections.Generic;
using System.Text;
using Akrual.DDD.Utils.Internal.Logging;
using Xunit;

namespace Akrual.DDD.Utils.Internal.Tests.Logging
{
    public class ConsoleLoggerTests
    {
        [Fact]
        public void Log_WithLoggerConfiguredToConsole_PrintOnConsole()
        {
            LogProvider.SetCurrentLogProvider(new ColoredConsoleLogProvider());

            var logger = LogProvider.GetCurrentClassLogger();

            logger.Debug("Debug message");
            logger.Fatal("Fatal message");
            Assert.True(true);
        }
    }
}
