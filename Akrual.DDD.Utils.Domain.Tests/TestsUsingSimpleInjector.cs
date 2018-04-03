using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands.Dispatcher;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.UOW;
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
