using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Domain.Tests.Utils;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands.Dispatcher;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.UOW;
using MediatR;
using MediatR.Pipeline;
using SimpleInjector;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Domain
{
    public class CommandEventTestsWithSimpleInjector : BaseAggregateRootTests<TabAggregate, TabAggregate>
    {
        private readonly Container container;
        public CommandEventTestsWithSimpleInjector()
        {
            container = new Container();

            container.RegisterSingleton<IMediator, Mediator>();
            
            container.Register(typeof(IRequestHandler<,>), AppDomain.CurrentDomain.GetAssemblies());
            container.Register(typeof(IRequestHandler<>), AppDomain.CurrentDomain.GetAssemblies());

            
            // we have to do this because by default, generic type definitions (such as the Constrained Notification Handler) won't be registered
            var notificationHandlerTypes = container.GetTypesToRegister(typeof(INotificationHandler<>), AppDomain.CurrentDomain.GetAssemblies(), new TypesToRegisterOptions
            {
                IncludeGenericTypeDefinitions = true,
                IncludeComposites = false,
            });
            container.RegisterCollection(typeof(INotificationHandler<>), notificationHandlerTypes);
            
            //Pipeline
            container.RegisterCollection(typeof(IPipelineBehavior<,>), new []
            {
                typeof(RequestPreProcessorBehavior<,>),
                typeof(RequestPostProcessorBehavior<,>),
                typeof(GenericPipelineBehavior<,>)
            });
            
            container.RegisterCollection(typeof(IRequestPreProcessor<>), new [] { typeof(GenericRequestPreProcessor<IDomainCommand<IEnumerable<IDomainEvent>>>) ,typeof(GenericRequestPreProcessor<>) });
            container.RegisterCollection(typeof(IRequestPostProcessor<,>), new[] { typeof(GenericRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>) });


            container.RegisterInstance(new SingleInstanceFactory(container.GetInstance));
            container.RegisterInstance(new MultiInstanceFactory(container.GetAllInstances));


            container.Register(typeof(IHandleDomainEvent<>),
                AppDomain.CurrentDomain.GetAssemblies());

            container.Register(typeof(IHandleDomainCommand<>),
                AppDomain.CurrentDomain.GetAssemblies());


            container.Register<IUnitOfWork,UnitOfWork>();
            container.Register(typeof(IDefaultFactory<>), typeof(DefaultFactory<>));
            container.Register<ICommandDispatcher,CommandDispatcher>();

            
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
        }

        [Fact]
        public async Task CanOpenANewTab()
        {
            var dispatcher = container.GetInstance<ICommandDispatcher>();

            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";
            
            var response = await dispatcher.Dispatch(new OpenTab(testId)
            {
                TableNumber = testTable,
                Waiter = testWaiter
            },CancellationToken.None);
            
            Assert.Equal(1, response.Count());
            Assert.Equal(testId, response.First().AggregateRootId);
            Assert.IsType<TabOpened>(response.First());
        }

    }
}
