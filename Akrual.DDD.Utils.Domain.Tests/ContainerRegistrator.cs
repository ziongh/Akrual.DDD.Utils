using System;
using System.Linq;
using System.Linq.Expressions;
using Akrual.DDD.Utils.Domain.EventStorage;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Factories.InstanceFactory;
using Akrual.DDD.Utils.Domain.Messaging.Coordinator;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands.Dispatcher;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents.Publisher;
using Akrual.DDD.Utils.Domain.Messaging.Saga;
using Akrual.DDD.Utils.Domain.UOW;
using Akrual.DDD.Utils.Internal.UsefulClasses;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Akrual.DDD.Utils.Domain.Tests
{
    public class ContainerRegistrator
    {
        public static void RegisterAllToContainer(Container container)
        {
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Options.LifestyleSelectionBehavior = new ScopedLifestyleSelectionBehavior();

            container.Register(typeof(IInstantiator<>),typeof(Instantiator<>), Lifestyle.Scoped);

            container.Register(typeof(IHandleDomainEvent<>),
                AppDomain.CurrentDomain.GetAssemblies(), Lifestyle.Scoped);

            container.Register(typeof(IHandleDomainCommand<>),
                AppDomain.CurrentDomain.GetAssemblies(), Lifestyle.Scoped);

            container.Register(typeof(IProcessManagerRedirect<>),
                AppDomain.CurrentDomain.GetAssemblies(), Lifestyle.Scoped);


            container.Register<IEventStore, InMemoryEventStore>(Lifestyle.Singleton);
            container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
            container.Register(typeof(IDefaultFactory<>),  typeof(DefaultFactory<>),Lifestyle.Scoped);

            container.Register<IDomainCommandDispatcher, DomainCommandDispatcher>(Lifestyle.Scoped);
            container.Register<IDomainEventPublisher, DomainEventPublisher>(Lifestyle.Scoped);
            container.Register<ICoordinator, Coordinator>(Lifestyle.Scoped);
            
            
            // The following registration is required to allow the injection of Func<T> where T
            // Is some interface already registered in the container.
            container.ResolveUnregisteredType += (s, e) =>
            {
                var type = e.UnregisteredServiceType;
                if (!type.IsGenericType ||
                    type.GetGenericTypeDefinition() != typeof(Func<>))
                    return;
                Type serviceType = type.GetGenericArguments().First();

                InstanceProducer producer = container.GetRegistration(serviceType, true);
                Type funcType = typeof(Func<>).MakeGenericType(serviceType);
                var factoryDelegate =
                    Expression.Lambda(funcType, producer.BuildExpression()).Compile();
                e.Register(Expression.Constant(factoryDelegate));
            };

            container.Verify();
        }
    }
}