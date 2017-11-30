using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Akrual.DDD.Utils.Domain.Exceptions
{
    /// <summary>
    /// <c>Exception related to a contract not being fulfilled</c>
    /// </summary>
    public class ContractException : Exception
    {
        public ContractException()
        {
        }

        public ContractException(string message) : base(message)
        {
        }

        public ContractException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ContractException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
