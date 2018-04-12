namespace Akrual.DDD.Utils.Internal.Logging.LogProviders
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    internal abstract class LogProviderBase : ILogProvider
    {
        protected delegate IDisposable OpenNdc(string message);
        protected delegate IDisposable OpenMdc(string key, object value, bool destructure);

        private readonly Lazy<OpenNdc> _lazyOpenNdcMethod;
        private readonly Lazy<OpenMdc> _lazyOpenMdcMethod;
        private static readonly IDisposable NoopDisposableInstance = new DisposableAction();

        protected LogProviderBase()
        {
            _lazyOpenNdcMethod
                = new Lazy<OpenNdc>(GetOpenNdcMethod);
            _lazyOpenMdcMethod
               = new Lazy<OpenMdc>(GetOpenMdcMethod);
        }

        public abstract Logger GetLogger(string name);

        public IDisposable OpenNestedContext(string message)
        {
            return _lazyOpenNdcMethod.Value(message);
        }

        public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
        {
            return _lazyOpenMdcMethod.Value(key, value, destructure);
        }

        protected virtual OpenNdc GetOpenNdcMethod()
        {
            return _ => NoopDisposableInstance;
        }

        protected virtual OpenMdc GetOpenMdcMethod()
        {
            return (_, __, ___) => NoopDisposableInstance;
        }
    }
}