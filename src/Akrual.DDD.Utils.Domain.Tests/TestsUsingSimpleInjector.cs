using System;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Akrual.DDD.Utils.Domain.Tests
{
    public class TestsUsingSimpleInjector : IDisposable
    {
        internal readonly Container _container;
        internal readonly Scope _scope;

        public TestsUsingSimpleInjector()
        {
            _container = new Container();
            ContainerRegistrator.RegisterAllToContainer(_container);

            _scope = AsyncScopedLifestyle.BeginScope(_container);
        }

        public void Dispose()
        {
            _scope?.Dispose();
            _container?.Dispose();
        }
    }
}
