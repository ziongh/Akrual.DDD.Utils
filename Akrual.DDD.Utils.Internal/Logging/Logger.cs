using System;

namespace Akrual.DDD.Utils.Internal.Logging
{
    public delegate bool Logger(LogLevel logLevel, Func<string> messageFunc, Exception exception = null, params object[] formatParameters);
}