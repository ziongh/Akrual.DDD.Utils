using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Akrual.DDD.Utils.Domain.Exceptions
{
    /// <summary>
    /// <c>Exception related to a contract not being fulfilled</c>
    /// </summary>
    public class ContractExceptionWithProperty : ContractException
    {
        public string PropertyName { get; set; }
        public object AttemptedValue { get; set; }
        public ContractExceptionWithProperty()
        {
        }

        public ContractExceptionWithProperty(string message) : base(message)
        {
        }

        public ContractExceptionWithProperty(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ContractExceptionWithProperty(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
