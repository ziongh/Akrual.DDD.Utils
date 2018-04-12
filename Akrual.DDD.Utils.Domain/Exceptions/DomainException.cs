using System;

namespace Akrual.DDD.Utils.Domain.Exceptions
{
    /// <summary>
    /// Base Exception for exceptions related with Domain
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException() { }
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception innerException) : base(message, innerException) { }
    }
}
