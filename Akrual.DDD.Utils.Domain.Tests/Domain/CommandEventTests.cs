using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akrual.DDD.Domain.Tests.Utils;
using Akrual.DDD.Utils.Domain.Aggregates;
using Akrual.DDD.Utils.Domain.Entities;
using Akrual.DDD.Utils.Domain.Exceptions;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomain;
using Akrual.DDD.Utils.Domain.Utils.UUID;
using MediatR;
using MediatR.Pipeline;
using SimpleInjector;
using Xunit;

namespace Akrual.DDD.Utils.Domain.Tests.Domain
{



    public class CommandEventTestsWithSimpleInjector : BaseAggregateRootTests<TabAggregate, TabAggregate>
    {
        [Fact]
        public async Task CanOpenANewTab()
        {
            var container = new Container();

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
            
            container.RegisterCollection(typeof(IRequestPreProcessor<>), new [] { typeof(GenericRequestPreProcessor<>) });
            container.RegisterCollection(typeof(IRequestPostProcessor<,>), new[] { typeof(GenericRequestPostProcessor<,>), typeof(ConstrainedRequestPostProcessor<,>) });


            container.RegisterSingleton(new SingleInstanceFactory(container.GetInstance));
            container.RegisterSingleton(new MultiInstanceFactory(container.GetAllInstances));


            container.Register(typeof(IHandleDomainEvent<>),
                AppDomain.CurrentDomain.GetAssemblies());

            container.Register(typeof(IHandleDomainCommand<>),
                AppDomain.CurrentDomain.GetAssemblies());

            var handler = container.GetInstance<IHandleDomainCommand<OpenTab>>();

            var mediator = container.GetInstance<IMediator>();



            

            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";

            var response = await mediator.Send(new OpenTab(testId)
            {
                TableNumber = testTable,
                Waiter = testWaiter
            });

            Assert.Equal(1, response.Count());

            Test(new TabAggregate(),
                Given(),
                When(new OpenTab(testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                Then(new TabOpened(testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }));
        }

    }


    public class CommandEventTests : BaseAggregateRootTests<TabAggregate, TabAggregate>
    {
        [Fact]
        public void CanOpenANewTab()
        {
            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";

            Test(new TabAggregate(), 
                Given(),
                When(new OpenTab(testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                Then(new TabOpened(testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }));
        }

        [Fact]
        public void CannotOpenTabTwice()
        {
            var testId = Guid.NewGuid();
            var testTable = 42;
            var testWaiter = "Derek";

            Test(new TabAggregate(), 
                Given(new TabOpened(testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                When(new OpenTab(testId)
                {
                    TableNumber = testTable,
                    Waiter = testWaiter
                }),
                ThenFailWith<TabOpenedTwiceException>());
        }
    }

    public class TabOpened : DomainEvent
    {
        public int TableNumber;
        public string Waiter;

        public TabOpened(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public TabOpened(Guid aggregateRootId) : base(aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return TableNumber;
            yield return Waiter;
        }
    }

    public class OpenTab : DomainCommand
    {
        public int TableNumber;
        public string Waiter;

        public OpenTab(Guid aggregateRootId, long entityVersion) : base(aggregateRootId, entityVersion)
        {
        }

        public OpenTab(Guid aggregateRootId) : base(aggregateRootId)
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            yield return TableNumber;
            yield return Waiter;
        }
    }

    public class TabOpenedTwiceException : DomainException
    {
        
    }


    public class TabAggregate : 
        AggregateRoot<TabAggregate>, 
        IHandleDomainCommand<OpenTab>,
        IHandleDomainEvent<TabOpened>
    {
        public int TableNumber { get; private set; }
        public string Waiter { get; private set; }
        public bool Opened { get; set; }

        public TabAggregate() : base(Guid.Empty)
        {
        }

        public async Task<IEnumerable<IDomainEvent>> Handle(OpenTab command, CancellationToken cancellationToken)
        {
            if (Opened)
                throw new TabOpenedTwiceException();

            return GetEvents(command);
        }

        private IEnumerable<IDomainEvent> GetEvents(OpenTab command)
        {
            yield return new TabOpened(command.AggregateRootId){TableNumber = command.TableNumber, Waiter = command.Waiter};
        }

        public async Task Handle(TabOpened notification, CancellationToken cancellationToken)
        {
            Opened = true;
        }
    }



    public class GenericRequestPreProcessor<TRequest> : IRequestPreProcessor<TRequest>
    {

        public GenericRequestPreProcessor()
        {
        }

        public Task Process(TRequest request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }


    public class GenericRequestPostProcessor<TRequest, TResponse> : IRequestPostProcessor<TRequest, TResponse>
    {
        public GenericRequestPostProcessor()
        {
        }

        public Task Process(TRequest request, TResponse response)
        {
            return Task.CompletedTask;
        }
    }

    public class ConstrainedRequestPostProcessor<TRequest, TResponse>
        : IRequestPostProcessor<TRequest, TResponse>
    {

        public ConstrainedRequestPostProcessor()
        {
        }

        public Task Process(TRequest request, TResponse response)
        {
            return Task.CompletedTask;
        }
    }


    public class GenericPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        public GenericPipelineBehavior()
        {
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var response = await next();
            return response;
        }
    }
}
