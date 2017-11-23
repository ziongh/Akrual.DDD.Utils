using System;
using Akrual.DDD.Utils.Domain.Utils.Collections.EquallityComparer;

namespace Akrual.DDD.Utils.Domain.Entities
{
    /// <summary>
    ///     Base class for any Entity Type (i.e. the 'Entity Object' oxymoron of DDD).
    ///     All you have to do is to implement the abstract methods: <see cref="EquatableByValue{T}.GetAllAttributesToBeUsedForEquality"/>
    /// </summary>
    /// <typeparam name="T">Domain type to be 'turned' into a Value Type.</typeparam>
    public abstract class Entity<T> : IEntity
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Entity"/> class.
        ///     Every constructor have to satisfy:
        ///     <remarks><c>If the entity is not an Aggregate Root, then it cannot be created without passing the owner of this entity (Some entity inside the Aggregate, maybe the Aggregate root)!</c></remarks>
        ///     <remarks><c>GUID. This must be unique inside the Aggegate.!</c></remarks>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="owner"></param>
        protected Entity(Guid id, IEntity owner)
        {
            if (id == default(Guid))
            {
                throw new ArgumentException("Id must be defined.", "id");
            }

            Id = id;
        }

        /// <summary>
        /// It should never be possible to Create an Entity without and ID and an Owner.
        /// </summary>
        private Entity()
        {
            
        }

        public Guid Id { get; protected set; }

        public override bool Equals(object obj)
        {
            var compareTo = obj as Entity<T>;

            if (ReferenceEquals(this, compareTo)) return true;
            if (ReferenceEquals(null, compareTo)) return false;

            return Id.Equals(compareTo.Id);
        }

        public static bool operator ==(Entity<T> a, Entity<T> b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
                return true;

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity<T> a, Entity<T> b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return (GetType().GetHashCode() * 907) + Id.GetHashCode();
        }

        public override string ToString()
        {
            return GetType().Name + " [Id=" + Id + "]";
        }
    }
}
