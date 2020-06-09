using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Akrual.DDD.Utils.Domain.Utils.TypeCache
{
    public interface IDomainTypeFinder
    {
        List<Type> GetAllDomainEventType();
        List<Type> GetAllDomainCommandType();
        Type GetTypeFromString(string typeFullName);
    }

    public class DomainTypeFinder : IDomainTypeFinder
    {
        private readonly List<Type> allDomainCommands;
        private readonly List<Type> allDomainEvents;
        public DomainTypeFinder(params Assembly[] assemblies)
        {
            allDomainCommands = new List<Type>();
            allDomainEvents = new List<Type>();
            foreach (var assembly in assemblies)
            {
                allDomainCommands.AddRange(assembly.GetTypes().Where(s => s.Implements(typeof(IDomainCommand))).ToList());
                allDomainEvents.AddRange(assembly.GetTypes().Where(s => s.Implements(typeof(IDomainEvent))).ToList());
            }
        }

        public List<Type> GetAllDomainCommandType()
        {
            return allDomainCommands;
        }
        public List<Type> GetAllDomainEventType()
        {
            return allDomainEvents;
        }

        public Type GetTypeFromString(string typeFullName)
        {
            return allDomainEvents.FirstOrDefault(s => s.FullName == typeFullName) ?? 
            allDomainEvents.FirstOrDefault(s => s.FullName == typeFullName);
        }
    }
}
