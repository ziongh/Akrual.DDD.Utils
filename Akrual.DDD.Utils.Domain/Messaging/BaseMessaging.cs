using System;
using System.Text;
using Akrual.DDD.Utils.Domain.Utils.Collections.EquallityComparer;

namespace Akrual.DDD.Utils.Domain.Messaging
{
    public abstract class BaseMessaging<T> : EquatableByValue<T> where T : IMessaging
    {
        public DateTime TimeStamp { get; protected set; }
        public Guid SagaId { get; protected set; }

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
