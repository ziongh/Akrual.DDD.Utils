using System;
using System.Diagnostics.CodeAnalysis;

namespace Akrual.DDD.Utils.Internal.Logging.LogProviders
{
    [ExcludeFromCodeCoverage]
    internal class DisposableAction : IDisposable
    {
        private readonly Action _onDispose;

        public DisposableAction(Action onDispose = null)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (_onDispose != null)
            {
                _onDispose();
            }
        }
    }
}