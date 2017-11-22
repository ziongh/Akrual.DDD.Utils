using System;
using System.Collections.Generic;
using System.Linq;

namespace Akrual.DDD.Utils.Domain.Utils.Collections.EquallityComparer
{
    /// <summary>
    /// Support a by-Value Equality and Unicity.
    /// </summary>
    /// <remarks>This latest implementation has been inspired from Scott Millett's book (Patterns, Principles, and Practices of Domain-Driven Design).</remarks>
    /// <typeparam name="T">Type of the elements.</typeparam>
    public abstract class EquatableByValue<T> : IEquatable<T>
    {
        protected const int Undefined = -1;

        protected volatile int hashCode = Undefined;

        protected void ResetHashCode()
        {
            hashCode = Undefined;
        }

        public static bool operator ==(EquatableByValue<T> x, EquatableByValue<T> y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            return x.Equals(y);
        }

        public static bool operator !=(EquatableByValue<T> x, EquatableByValue<T> y)
        {
            return !(x == y);
        }

        public bool Equals(T other)
        {
            var otherEquatable = other as EquatableByValue<T>;
            if (otherEquatable == null)
            {
                return false;
            }

            return EqualsImpl(otherEquatable);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            T other;

            try
            {
                // we use a static cast here since we can't use the 'as' operator for structs and other value type primitives
                other = (T)obj;
            }
            catch (InvalidCastException e)
            {
                return false;
            }

            return Equals(other);
        }

        /// <summary>
        /// Use Yield to define every property used to diferentiate
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<object> GetAllAttributesToBeUsedForEquality();

        protected virtual bool EqualsImpl(EquatableByValue<T> otherEquatable)
        {
            // Implementation where orders of the elements matters.
            return GetAllAttributesToBeUsedForEquality().SequenceEqual(otherEquatable.GetAllAttributesToBeUsedForEquality());
        }

        public override int GetHashCode()
        {
            // Implementation where orders of the elements matters.
            if (hashCode == Undefined)
            {
                var code = 0;

                foreach (var attribute in GetAllAttributesToBeUsedForEquality())
                {
                    code = (code * 397) ^ (attribute == null ? 0 : attribute.GetHashCode());
                }

                hashCode = code;
            }

            return hashCode;
        }

    }
}
