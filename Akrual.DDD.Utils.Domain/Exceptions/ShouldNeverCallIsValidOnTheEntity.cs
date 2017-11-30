using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Akrual.DDD.Utils.Domain.Exceptions
{
    /// <summary>
    /// <c>You Should never try to verify validation in the entity. Because the entity should always be valid</c>
    /// </summary>
    public class ShouldNeverCallIsValidOnTheEntityException : Exception
    {
        public ShouldNeverCallIsValidOnTheEntityException()
        {
        }

        public ShouldNeverCallIsValidOnTheEntityException(string message) : base(message)
        {
        }

        public ShouldNeverCallIsValidOnTheEntityException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ShouldNeverCallIsValidOnTheEntityException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
