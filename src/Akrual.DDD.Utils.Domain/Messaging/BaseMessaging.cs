﻿using System;
using System.Text;
using Akrual.DDD.Utils.Domain.Messaging.DomainCommands;
using Akrual.DDD.Utils.Domain.Messaging.DomainEvents;
using Akrual.DDD.Utils.Domain.Utils.Collections.EquallityComparer;
using MessagePack;

namespace Akrual.DDD.Utils.Domain.Messaging
{
    [MessagePackObject]
    [Union(0, typeof(DomainEvent))]
    [Union(1, typeof(DomainCommand))]
    public abstract class BaseMessaging<T> : EquatableByValue<T> where T : IMessaging
    {
        
        [Key(2)]
        public Guid SagaId { get; protected set; }
        [Key(3)]
        public DateTime TimeStamp { get; protected set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(" [");
            foreach (var attr in GetAllAttributesToBeUsedForEquality())
            {
                sb.Append(attr.GetType().Name);
                sb.Append("=");
                sb.Append(attr.ToString());
                sb.Append("; ");
            }
            sb.Append("]");

            return GetType().Name + sb.ToString();
        }
    }
}
