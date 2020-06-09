using System.Text;
using Akrual.DDD.Utils.Domain.Utils.Collections.EquallityComparer;

namespace Akrual.DDD.Utils.Domain.ValueObjects
{
    /// <summary>
    ///     Base class for any Value Type (i.e. the 'Value Object' oxymoron of DDD).
    ///     All you have to do is to implement the abstract methods: <see cref="EquatableByValue{T}.GetAllAttributesToBeUsedForEquality"/>
    ///     <remarks><c>All of the object properties must have the setter as Private!</c></remarks>
    ///     <remarks><c>There should be no empty constructor!</c></remarks>
    /// </summary>
    /// <typeparam name="T">Domain type to be 'turned' into a Value Type.</typeparam>
    public abstract class ValueObject<T> : EquatableByValue<T>
    {
        /// <summary>
        /// The Entity Framework needs an empty constructor. But no Value object should have one.
        /// </summary>
        protected ValueObject()
        {
            
        }

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
