using System;
using System.Runtime.Serialization;

namespace Akrual.DDD.Utils.Internal.Exceptions
{
    /// <summary>
    /// <c>Exception related to a contract not being fulfilled</c>
    /// </summary>
    public class ContractExceptionWithProperty : ContractException
    {
        public string PropertyName { get; set; }
        public object AttemptedValue { get; set; }
        public object ExpectedValue { get; set; }
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
