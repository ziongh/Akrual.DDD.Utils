using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Akrual.DDD.Utils.Domain.Factories;
using Akrual.DDD.Utils.Domain.Messaging.Coordinator;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands.Dispatcher;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents.Publisher;
using Akrual.DDD.Utils.Domain.Tests.ExampleDomains.NameNumberDate;
using Akrual.DDD.Utils.Domain.UOW;
using Akrual.DDD.Utils.Internal.UsefulClasses;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace Akrual.DDD.Utils.Domain.Tests
{
    public static class ContainerRegistrator
    {
        public static void RegisterAllToContainer(Container container)
        {
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            container.Options.LifestyleSelectionBehavior = new ScopedLifestyleSelectionBehavior();

            //container.RegisterCollection(typeof(IHandleDomainEvent<>),
            //    AppDomain.CurrentDomain.GetAssemblies());

            // registers a list of all those (singleton) tasks.
            container.RegisterCollection(typeof(IHandleDomainEvent<>),
                AppDomain.CurrentDomain.GetAssemblies());

            container.Register(typeof(IHandleDomainCommand<>),
                AppDomain.CurrentDomain.GetAssemblies(), Lifestyle.Scoped);


            container.Register<IUnitOfWork, UnitOfWork>(Lifestyle.Scoped);
            container.Register(typeof(IDefaultFactory<>),  typeof(DefaultFactory<>),Lifestyle.Scoped);
            container.RegisterCollection(typeof(IDefaultEventHandlerFactory<>),  AppDomain.CurrentDomain.GetAssemblies());


            container.Register<IDomainCommandDispatcher, DomainCommandDispatcher>(Lifestyle.Scoped);
            container.Register<IDomainEventPublisher, DomainEventPublisher>(Lifestyle.Scoped);
            container.Register<ICoordinator, Coordinator>(Lifestyle.Scoped);
            
            
            // The following registration is required to allow the injection of Func<T> where T
            // Is some interface already registered in the container.
            container.ResolveUnregisteredType += (o, args) => ResolveFuncOfT(o, args, container);
            container.ResolveUnregisteredType += (o, args) => ResolveIenumerableOfFuncOfT(o, args, container);

            container.Verify();
        }

        private static void ResolveFuncOfT(object s, UnregisteredTypeEventArgs e, Container container)
        {
            var type = e.UnregisteredServiceType;
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Func<>)) return;
            Type serviceType = type.GetGenericArguments().First();
            
            InstanceProducer producer = container.GetRegistration(serviceType, true);
            Type funcType = typeof(Func<>).MakeGenericType(serviceType);
            var factoryDelegate = Expression.Lambda(funcType, producer.BuildExpression()).Compile();
            
            e.Register(Expression.Constant(factoryDelegate));
        }

        static readonly Type TypedConstantExpressionType = Type.GetType("System.Linq.Expressions.TypedConstantExpression");

        /// <summary>
        /// Given a lambda expression that expressed a new object, returns the <see cref="System.Reflection.TypeInfo"/> of what type was expected to be allocated
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static TypeInfo GetTypeInfo(Expression<Action> expression) //Expression<Action> allows the syntax () => where Expression would require a Delgate.
        {
            Expression body = expression.Body;

            if (body is NewExpression)
            {
                NewExpression newExpression = expression.Body as NewExpression;

                return IntrospectionExtensions.GetTypeInfo((expression.Body as NewExpression).Constructor.DeclaringType);
            }
            else if (body is MemberExpression)
            {
                MemberExpression memberExpression = body as MemberExpression;

                return IntrospectionExtensions.GetTypeInfo(memberExpression.Member.DeclaringType);
            }
            else if (body is MethodCallExpression)
            {
                MethodCallExpression methodCallExpression = expression.Body as MethodCallExpression;

                if (methodCallExpression.Object is MemberExpression)
                {
                    return IntrospectionExtensions.GetTypeInfo((methodCallExpression.Object as MemberExpression).Member.DeclaringType);
                }

                var TypedConstantExpressionValueProperty = IntrospectionExtensions.GetTypeInfo(TypedConstantExpressionType).GetProperty("Value");

                //Actually a RuntimeType from a TypedConstantExpression...
                return IntrospectionExtensions.GetTypeInfo((Type)TypedConstantExpressionValueProperty.GetMethod.Invoke(methodCallExpression.Object, null));
            }

            throw new System.NotSupportedException("Please create an issue for your use case.");
        }


        private static void ResolveIenumerableOfFuncOfT(object s, UnregisteredTypeEventArgs e, Container container)
        {
            var type = e.UnregisteredServiceType;
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(IEnumerable<>)) return;

            Type funcOfServiceType = type.GetGenericArguments().First();
            if (!funcOfServiceType.IsGenericType || funcOfServiceType.GetGenericTypeDefinition() != typeof(Func<>)) return;

            Type serviceType = funcOfServiceType.GetGenericArguments().First();
            Type iEnumTypeForServices = typeof(IEnumerable<>).MakeGenericType(serviceType);


            //e.Register(Expression.Constant(Expression<Func<>>));

            InstanceProducer producer = container.GetRegistration(iEnumTypeForServices, true);

            ConstantExpression instanceExpression = producer.BuildExpression() as ConstantExpression;

            Type typeOfListOfFunc = typeof(List<>).MakeGenericType(typeof(Delegate));
            var resultCollection = Expression.Variable(typeOfListOfFunc, "result");
            var assignEmptyList = Expression.Assign(resultCollection, Expression.New(typeOfListOfFunc.GetConstructor(new Type[] { })));

            


            var collection = Expression.Parameter(typeof(IEnumerable<Lazy<InstanceProducer>>), "collection");
            var loopVar = Expression.Parameter(typeof(Lazy<InstanceProducer>), "loopVar");

            // Create body of loop

            // get list of producers from collection
            var type2 = instanceExpression.Value.GetType();

            var producersss = GetInstanceField(type2, instanceExpression.Value, "producers");
            
            var eParameter = Expression.Constant(e);

            // loop body

            var producerValue = Expression.PropertyOrField(loopVar, "Value");

            // InstanceProducer producer = container.GetRegistration(serviceType, true);
            // Type funcType = typeof(Func<>).MakeGenericType(serviceType);
            // var factoryDelegate = Expression.Lambda(funcType, producer.BuildExpression()).Compile();
            // e.Register(Expression.Constant(factoryDelegate));
     
            var callMakeGeneric = Expression.Call(Expression.Constant((Type)typeof(Func<>)), typeof(Func<>).GetType().GetMethod("MakeGenericType"),
                new[] {Expression.Constant(new Type[] {serviceType})});

            var variableFunctype = Expression.Variable(typeof(Type), "funcType");
            var assingvariableFunctype = Expression.Assign(variableFunctype, callMakeGeneric);

            var callToBuildOfProiducer = Expression.Call(producerValue,
                typeof(InstanceProducer).GetMethod("BuildExpression"));

            var callLambdaOfExpression = Expression.Call(typeof(Expression).GetMethod("Lambda", new [] {typeof(Type), typeof(Expression), typeof(ParameterExpression[])}),
                new Expression[] {assingvariableFunctype, callToBuildOfProiducer, Expression.Constant(new ParameterExpression[]{}) });

            var callCompiledfactory = Expression.Call(callLambdaOfExpression, typeof(LambdaExpression).GetMethod("Compile",new Type[] {}));

            var ExpressionAddToList = Expression.Call(resultCollection, typeOfListOfFunc.GetMethod("Add"),
                callCompiledfactory);



            //var constantOfFactory = Expression.Constant(callCompiledfactory);
            //var register = Expression.Call(eParameter, typeof(UnregisteredTypeEventArgs).GetMethod("Register",new [] {typeof(Expression)}), constantOfFactory);


            // Call Loop
            var loop = ForEach(collection, loopVar, ExpressionAddToList);

            var block = Expression.Block(new Expression[]
            {
                assignEmptyList,
                loop,
                Expression.Label(Expression.Label(typeof(List<Delegate>)),resultCollection)
            });

            var compiled = Expression.Lambda<Func<IEnumerable<Lazy<InstanceProducer>>, List<Delegate>>>(block, collection).Compile();

            var handlers = compiled(producersss as IEnumerable<Lazy<InstanceProducer>>);

            e.Register(Expression.Constant(handlers));
        }


        public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Expression.Label("LoopBreak");

            var loop = Expression.Block(new[] { enumeratorVar },
                enumeratorAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block(new[] { loopVar },
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel)
            );

            return loop;
        }



        public static Delegate CreateDelegateWithObjectParameters(object instance, MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters()
                .Select(parameterInfo => new
                {
                    MethodParameterType = parameterInfo.ParameterType,
                    DelegateParameter = Expression.Parameter(typeof(object), parameterInfo.Name)
                })
                .Select(x => new
                {
                    x.DelegateParameter,
                    MethodParameter = Expression.Convert(x.DelegateParameter, x.MethodParameterType)
                });
            MethodCallExpression methodCallExpression = instance == null
                ? Expression.Call(methodInfo, parameters.Select(x => x.MethodParameter).ToArray())
                : Expression.Call(Expression.Constant(instance), methodInfo, parameters.Select(x => x.MethodParameter).ToArray());
            return Expression.Lambda(methodCallExpression, parameters.Select(x => x.DelegateParameter).ToArray()).Compile();
        }

        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        public static void GetFuncOfT()
        {

        }
    }
}